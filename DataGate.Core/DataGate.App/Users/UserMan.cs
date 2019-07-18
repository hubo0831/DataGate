using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataGate.Com.DB;
using DataGate.App.Models;
using DataGate.App.DataService;
using DataGate.Com;

namespace DataGate.App
{
    public class UserMan : DBCrud<AppUser>
    {
        public UserMan() : base(DBFactory.CreateDBHelper("Default")) { }

        /// <summary>
        /// 根据账号返回用户
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public virtual async Task<AppUser> GetAsync(string account)
        {
            return await GetModelByWhereAsync("account=@account", new { account });
        }

        /// <summary>
        /// 根据Email返回单个用户，有多个则返回空
        /// </summary>
        /// <param name="tel"></param>
        /// <returns></returns>
        public virtual async Task<AppUser> GetByTelAsync(string tel)
        {
            return await GetModelByWhereAsync("tel=@tel", new { tel });
        }

        /// <summary>
        /// 根据电话号码返回单个用户，有多个则返回空
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public virtual async Task<AppUser> GetByEmailAsync(string email)
        {
            return await GetModelByWhereAsync("email=@email", new { email });
        }

        /// <summary>
        /// 根据用户名、手机或邮箱获取用户,这要求这三者在数据库都唯一
        /// </summary>
        /// <param name="info"></param>
        /// <returns>用户名、手机或邮箱</returns>
        public virtual async Task<AppUser> GetByAllAsync(string info)
        {
            return await GetModelByWhereAsync("account=@info OR email=@info OR tel=@info", new { info });
        }
    }
}
