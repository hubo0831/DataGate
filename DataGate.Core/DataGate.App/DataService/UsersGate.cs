using DataGate.Com;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DataGate.App.DataService
{
    public class UsersGate : ISubmitDataGate
    {
        public void OnAdd(DataGateKey gkey, IDictionary<string, object> ps)
        {
            if (!ps.TryGetValue("Password", out object pwd) && !ps.TryGetValue("password", out pwd))
            {
                pwd = "123456";
            }
            ps["PasswordSalt"] = CommOp.NewId();

            ps["Password"] = Encryption.MD5(pwd + (string)ps["PasswordSalt"]);
            ps["createDate"] = DateTime.Now;
        }

        public void OnChange(DataGateKey gkey, IDictionary<string, object> ps)
        {
            if (!ps.TryGetValue("Password", out object pwd) && !ps.TryGetValue("password", out pwd))
            {
                return;
            }
            ps["PasswordSalt"] = CommOp.NewId();
            ps["Password"] = Encryption.MD5(pwd + (string)ps["PasswordSalt"]);
        }

        public void OnRemove(DataGateKey gkey, IDictionary<string, object> ps)
        {
        }
    }
}
