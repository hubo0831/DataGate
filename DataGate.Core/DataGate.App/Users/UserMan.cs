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

        public async Task<AppUser> GetAsync(string account)
        {
            AppUser user = null;
            user = await Helper.GetModelByWhereAsync<AppUser>("account=@account", Helper.CreateParameter("account", account.ToLower()));
            if (user != null)
            {
                return user;
            }
            if (CommOp.IsEmail(account))
            {
                user = await Helper.GetModelByWhereAsync<AppUser>("email=@email", Helper.CreateParameter("email", account.ToLower()));
            }
            else if (CommOp.IsPhoneNumber(account))
            {
                user = await Helper.GetModelByWhereAsync<AppUser>("tel=@tel", Helper.CreateParameter("tel", account.ToLower()));
            }
            return user;
        }

        public async Task<AppUser> GetByIdAsync(string id)
        {
            var appUser = await Helper.GetModelByIdAsync<AppUser>(id);
            return appUser;
        }

    }
}
