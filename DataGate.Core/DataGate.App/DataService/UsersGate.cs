using DataGate.Com;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DataGate.App.DataService
{
    public class UsersGate : ISubmitDataGate
    {
        string GetLUKey(IDictionary<string, object> ps, string key)
        {
            return ps.Keys.FirstOrDefault(k => k.Equals(key, StringComparison.OrdinalIgnoreCase));
        }

        public void OnAdd(DataGateKey gkey, IDictionary<string, object> ps)
        {
            string passKey = GetLUKey(ps, "Password");
            string pwd = null;
            if (passKey != null)
            {
                pwd = (string)ps[passKey];
            }
            else
            {
                passKey = "Password";
            }
            if (pwd.IsEmpty())
            {
                pwd = "123456";
            }
            gkey.MainTable.Fields.Add(new FieldMeta { Name = "Password" });
            gkey.MainTable.Fields.Add(new FieldMeta { Name = "PasswordSalt" });
            ps["PasswordSalt"] = CommOp.NewId();

            ps[passKey] = Encryption.MD5(pwd + (string)ps["PasswordSalt"]);
            ps["createDate"] = DateTime.Now;
        }

        public void OnChange(DataGateKey gkey, IDictionary<string, object> ps)
        {
            string passKey = GetLUKey(ps, "Password");
            if (passKey == null)
            {
                return;
            }
            string pwd = (string)ps[passKey];
            ps["PasswordSalt"] = CommOp.NewId();
            ps[passKey] = Encryption.MD5(pwd + (string)ps["PasswordSalt"]);
            gkey.MainTable.Fields.Add(new FieldMeta { Name = "Password" });
            gkey.MainTable.Fields.Add(new FieldMeta { Name = "PasswordSalt" });
        }

        public void OnRemove(DataGateKey gkey, IDictionary<string, object> ps)
        {
        }
    }
}
