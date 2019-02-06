using System;
using System.Web.Http;
using DataGate.App;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DataGate.Com;
using DataGate.App.Models;

namespace DataGate.Api.Controllers
{
    /// <summary>
    ///  sso验证
    /// <para>其他站点通过后台Post来认证</para>
    /// </summary>
    [Route("api/[controller]/[action]")]
    public class CheckController : BaseController
    {
        SessionProvider _session;
        UserMan _user;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="session"></param>
        /// <param name="user"></param>
        public CheckController(SessionProvider session, UserMan user)
        {
            _session = session;
            _user = user;
        }

        /// <summary>
        /// 根据token获取用户及用户可访问的所有资源
        /// </summary>
        /// <param name="token"></param>
        [HttpGet]
        public async Task<UserInfoResult> GetUser()
        {
            return await _session.GetUserAsync(Token);
        }

        /// <summary>
        /// 修改密码时，远程检查旧密码
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public async Task<bool> Password(string p)
        {
            var userSession = _session.Get(Token);

            var user = await _user.GetAsync(userSession.Account);
            //  ps["Password"] = Encryption.MD5("123456" + ps["PasswordSalt"]);
            return Encryption.MD5(p + user.PasswordSalt) == user.Password;
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> ChangePassword(string p)
        {
            var userSession = _session.Get(Token);

            var user = await _user.GetAsync(userSession.Account);
            //  ps["Password"] = Encryption.MD5("123456" + ps["PasswordSalt"]);
            user.PasswordSalt = CommOp.NewId();
            user.Password = Encryption.MD5(p + user.PasswordSalt);
            await _user.UpdateAsync(user);
            return true;
        }

        /// <summary>
        /// 修改个人信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="tel"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<bool> ChangeProfile(AppUser u)
        {
            var userSession = _session.Get(Token);

            var user = await _user.GetAsync(userSession.Account);
            //  ps["Password"] = Encryption.MD5("123456" + ps["PasswordSalt"]);
            user.Name = u.Name;
            user.Email = u.Email;
            user.Tel = u.Tel;
            await _user.UpdateAsync(user);
            return true;
        }

        /// <summary>
        /// 登录接口
        /// </summary>
        /// <param name="request">登录参数</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<LoginResult> Login(LoginRequest request)
        {
            var result = await _session.Login(request);
            Log.Account = request.Account;
            Log.Abstract = result.Code == 0 ? "登录成功" : "登录失败";
            return result;
        }

        /// <summary>
        /// 注销登录
        /// </summary>
        [HttpPost]
        public bool Logout()
        {
            return _session.Remove(Token);
        }
    }
}