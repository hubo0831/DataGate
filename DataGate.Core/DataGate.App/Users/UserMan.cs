using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataGate.Com.DB;
using DataGate.App.Models;
using DataGate.App.DataService;

namespace DataGate.App
{
    public class UserMan : DBCrud<AppUser>
    {
        public UserMan() : base(DBFactory.CreateDBHelper("Default")) { }

        public async Task<AppUser> GetAsync(string account)
        {
            var appUser = await Helper.GetModelByWhereAsync<AppUser>("account=@account", Helper.CreateParameter("account", account.ToLower()));
            return appUser;
        }

        public async Task<AppUser> GetByIdAsync(string id)
        {
            var appUser = await Helper.GetModelByIdAsync<AppUser>(id);
            return appUser;
        }

    }
}
