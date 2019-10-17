using DataGate.App.DataService;
using DataGate.App.Models;
using DataGate.Com;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGate.App
{
    /// <summary>
    /// 会话管理类，以Token作为Key的字典来管理会话
    /// </summary>
    public class SessionProvider : ISingleton, ISessionProvider
    {
        UserMan _user;
        MenuMan _menu;
        int Expires = 30; //session过期时间（分钟）
        //int SlidingExpiration = 60; //session滑动过期时间（分钟）
        int OnlineSpan = 5; //在线判定标准（分钟）

        private IMemoryCache MemoryCache { get; }
        private IDistributedCache DistributedCache { get; }

        public SessionProvider(IMemoryCache memoryCache, IDistributedCache distributedCache, UserMan userMan, MenuMan menu)
        {
            this.MemoryCache = memoryCache;
            this.DistributedCache = distributedCache;
            _user = userMan;
            _menu = menu;
            Expires = Consts.Config.GetSection("Session:Timeout").Value.ToInt();
            OnlineSpan = Consts.Config.GetSection("Session:OnlineSpan").Value.ToInt();
            if (Expires <= 0) Expires = 30;
            if (OnlineSpan <= 0) OnlineSpan = 5;
        }

        /// <summary>会话缓存键</summary>
        private string GetCacheKey(string token)
        {
            return $"{typeof(UserSession).FullName}:{token}";
        }

        /// <summary>
        /// 主动移除token对应的session,通常是用户发起logout请求时。
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<bool> Remove(string token)
        {
            var cacheKey = GetCacheKey(token);
            await this.DistributedCache.RemoveAsync(cacheKey);
            this.MemoryCache.Remove(cacheKey);
            return true;
        }

        /// <summary>
        /// 获取用户的Session,当用户sesssion过期时，返回空值
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<UserSession> Get(string token)
        {
            var cacheKey = GetCacheKey(token);
            if (this.MemoryCache.TryGetValue(cacheKey, out var sessionObj))
            {
                return sessionObj as UserSession;
            }
            var sessionBytes = await this.DistributedCache.GetAsync(cacheKey);
            if (sessionBytes == null) return null;
            var session = Encoding.UTF8.GetString(sessionBytes).FromJson<UserSession>();
            this.MemoryCache.Set(cacheKey, session, new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = session.LastOpTime + TimeSpan.FromMinutes(this.Expires)
            });
            return session;
        }

        /// <summary>
        /// 根据token获取用户信息,将用户表所有字段（除密码等信息外）返回给客户端
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<object> GetUserAsync(string token)
        {
            var userSession = await Get(token);
            if (userSession == null)
            {
                return MSG.SessionExpired;
            }
            DataGateService ds = Consts.Get<DataGateService>();
            var user = await ds.QueryAsync("GetUser", new { userSession.Id }) as Dictionary<string, object>;
            if (user == null)
            {
                return MSG.UserNotExists;
            }

            user["menus"] = await GetUserMenus(userSession.Id);
            return user;
        }

        //在角色管理里自动勾选的父级菜单不会保存到AppRoleMenu表，所以这里
        //要重新取出子级被授权的父级菜单
        private async Task<IList<AppMenu>> GetUserMenus(string userId)
        {
            var allMenus = await _menu.GetAllAsync();
            DataGateService ds = Consts.Get<DataGateService>();
            var userMenuIds = ((JArray)(await ds.QueryAsync("GetUserMenuIds", new { userId })))
                .Select(dr => (string)dr.First);

            userMenuIds = userMenuIds.Select(id => GetParentIds(allMenus, id)).UnionAll();
            return allMenus.Where(m => userMenuIds.Contains(m.Id)).ToList();
        }

        //往上找出所有的上级ID
        private IEnumerable<string> GetParentIds(IEnumerable<AppMenu> menus, string id)
        {
            while (!id.IsEmpty())
            {
                yield return id;
                var p = menus.FirstOrDefault(m => m.Id == id);
                if (p != null)
                {
                    id = p.ParentId;
                }
                else break;
            }
        }

        /// <summary>
        /// 登录,根据用户名，手机，邮箱来登录，当同一手机，邮箱不止一个用户使用时，将登录不成功
        /// </summary>
        /// <param name="request"></param>
        /// <param name="validate">验证密码</param>
        /// <returns></returns>
        public async Task<LoginResult> Login(LoginRequest request, bool validate = true)
        {
            LoginResult result = new LoginResult();
            string requestPass = null;

            //登录时回传的记住我的信息，从记住我的信息恢复用户的登录用户名密码
            if (request.Remember?.Length > 10)
            {
                RestoreFormRemember(request);
                requestPass = request.Password;
            }
            AppUser user = null;
            request.Account = request.Account.ToLower();
            if (user == null && CommOp.IsEmail(request.Account))
            {
                user = await _user.GetByEmailAsync(request.Account);
            }
            if (user == null && CommOp.IsPhoneNumber(request.Account))
            {
                user = await _user.GetByTelAsync(request.Account);
            }
            if (user == null)
            {
                user = await _user.GetAsync(request.Account);
            }
            if (user == null)
            {
                return MSG.UserNotExists;
            }

            if (requestPass == null)
            {
                requestPass = Encryption.MD5(request.Password + user.PasswordSalt);
            }
            if (user.Password != requestPass && validate)
            {
                return MSG.PasswordError;
            }
            UserSession session = new UserSession
            {
                Token = CommOp.NewId(),
                Account = user.Account,
                Id = user.Id,
                LastOpTime = DateTime.Now
            };
            var cacheKey = GetCacheKey(session.Token);
            var sessionBytes = Encoding.UTF8.GetBytes(session.ToJson(false));
            await this.DistributedCache.SetAsync(cacheKey, sessionBytes, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(this.Expires)
            });
            this.MemoryCache.Set(cacheKey, session);

            DataGateService ds = Consts.Get<DataGateService>();

            //写最后登录时间，并进一步判断用户是否存在
            await ds.UpdateAsync("UpdateLastLoginTime", new
            {
                id = user.Id,
                LastLoginDate = session.LastOpTime
            });

            //要求“记住我”时，将登录信息加密回传,根据服务端的加密
            if (request.Remember == "1")
            {
                request.Remember = Encryption.Encrypt(String.Join("|", user.Account, user.Password));
            }
            return new LoginResult
            {
                ExpireIn = Expires,
                Token = session.Token,
                Remember = request.Remember
            };
        }

        async Task<AppUser> CreateTempUser(string tempId)
        {
            DataGateService dataSvc = Consts.Get<DataGateService>();
            var newUser = new AppUser
            {
                Id = tempId,
                Account = tempId,
                Name = tempId,
                Password = CommOp.NewId().Substring(8, 16)
            };

            await dataSvc.SubmitAsync("SaveUser", new
            {
                Added = new object[] { newUser }
            });

            return newUser;
        }

        /// <summary>
        /// 使用临时信息登录游客账号
        /// </summary>
        /// <param name="tempId"></param>
        /// <returns></returns>
        public async Task<LoginResult> TempLogin(string tempId)
        {
            AppUser user = await _user.GetModelByIdAsync(tempId);
            if (user == null)
            {
                user = await CreateTempUser(tempId);
            }
            else if (user.Account != user.Id) //这是正式用户
            {
                return MSG.NotLogined;
            }
            return await Login(new LoginRequest
            {
                Account = user.Account,
                Password = user.Password,
                Remember = "1"
            }, false);
        }

        private void RestoreFormRemember(LoginRequest request)
        {
            string decode = Encryption.Decrypt(request.Remember);
            string[] userPasswords = decode.Split(new char[] { '|' }, 2);
            if (userPasswords.Length != 2) return;
            request.Account = userPasswords[0];
            request.Password = userPasswords[1];
        }

        /// <summary>
        /// 检查用户信息合法性
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<ApiResult> CheckNewUserAsync(AppUser user)
        {
            ApiResult result = new ApiResult();
            user.Account = user.Account.ToLower();
            AppUser existsUser = await _user.GetAsync(user.Account);
            if (existsUser != null)
            {
                throw new Exception("账号已存在，请换一个名称");
            }

            if (!user.Tel.IsEmpty())
            {
                user.Tel = user.Tel.ToLower();
                existsUser = await _user.GetByTelAsync(user.Tel);
            }
            if (existsUser != null)
            {
                throw new Exception("该电话号码被注册过了");
            }

            if (!user.Email.IsEmpty())
            {
                user.Email = user.Email.ToLower();
                existsUser = await _user.GetByEmailAsync(user.Email);
            }
            if (existsUser != null)
            {
                throw new Exception("该邮箱被注册过了");
            }
            return result;
        }

        /// <summary>
        /// 注册新用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<ApiResult> RegisterAsync(AppUser user)
        {
            var result = await CheckNewUserAsync(user);
            DataGateService dataSvc = Consts.Get<DataGateService>();
            var existsUser = await _user.GetModelByIdAsync(user.Id);
            if (existsUser == null)
            {
                throw new Exception("非法的注册信息");
            }
            await dataSvc.SubmitAsync("SaveUser", new
            {
                Changed = new object[] { user }
            });
            result.Message = "注册成功";
            return result;
        }
    }
}
