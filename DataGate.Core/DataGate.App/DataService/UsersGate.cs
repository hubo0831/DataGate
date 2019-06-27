using DataGate.Com;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DataGate.App.Models;

namespace DataGate.App.DataService
{
    public class UsersGate : ISubmitedDataGate
    {
        string GetLUKey(IDictionary<string, object> ps, string key)
        {
            return ps.Keys.FirstOrDefault(k => k.Equals(key, StringComparison.OrdinalIgnoreCase));
        }

        public void OnAdded(DataGateKey gkey, IDictionary<string, object> ps)
        {
            string pwdKey = GetLUKey(ps, "password");
            string pwd = "123456";
            if (!pwdKey.IsEmpty())
            {
                pwd = (string)ps[pwdKey];
            }
            string passwordSalt = CommOp.NewId();
            string password = Encryption.MD5(pwd + passwordSalt);
            DateTime createDate = DateTime.Now;
            string id = (string)ps[GetLUKey(ps, "id")];
            gkey.DataService.DB.ExecNonQuery(@"UPDATE APP_USER SET PASSWORD=@password,
CREATE_DATE=@createDate,PASSWORD_SALT=@passwordSalt WHERE ID=@id", gkey.DataService.DB.GetParameter(new
            {
                password,
                createDate,
                passwordSalt,
                id
            }).ToArray());
        }

        public void OnChanged(DataGateKey gkey, IDictionary<string, object> ps)
        {
            string pwdKey = GetLUKey(ps, "password");
            if (pwdKey.IsEmpty())
            {
                return;
            }

            string pwd = (string)ps[pwdKey];
            string passwordSalt = CommOp.NewId();
            string password = Encryption.MD5(pwd + passwordSalt);
            DateTime createDate = DateTime.Now;
            string id = (string)ps["id"];
            gkey.DataService.DB.ExecNonQuery(@"UPDATE APP_USER SET PASSWORD=@password, PASSWORD_SALT=@passwordSalt WHERE ID=@id", gkey.DataService.DB.GetParameter(new
            {
                password,
                passwordSalt,
                id
            }).ToArray());
        }

        public void OnRemoved(DataGateKey gkey, IDictionary<string, object> ps)
        {
        }
    }
}
