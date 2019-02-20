using DataGate.Com;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DataGate.App.DataService
{
    public class UsersGate : ISubmitDataGate
    {
        private void TestField( List<string> fields, string f)
        {
            var fo = fields.FirstOrDefault(ff => ff.Equals(f, StringComparison.OrdinalIgnoreCase));
            if (fo != null)
            {
                fields.Remove(fo);
            }
            fields.Add(f);
        }

        public void OnAdd(DataGateKey gkey, List<string> fields, IDictionary<string, object> ps)
        {
            if (!ps.TryGetValue("Password", out object pwd) && !ps.TryGetValue("password", out pwd))
            {
                pwd = "123456";
            }
            TestField(fields, "Password");
            ps["PasswordSalt"] = CommOp.NewId();
            fields.Add("PasswordSalt");

            ps["Password"] = Encryption.MD5(pwd + (string)ps["PasswordSalt"]);
            ps["createDate"] = DateTime.Now;
        }

        public void OnChange(DataGateKey gkey, List<string> fields, IDictionary<string, object> ps)
        {
            if (!ps.TryGetValue("Password", out object pwd) && !ps.TryGetValue("password", out pwd))
            {
                return;
            }
            TestField(fields, "Password");
            ps["PasswordSalt"] = CommOp.NewId();
            fields.Add("PasswordSalt");

            ps["Password"] = Encryption.MD5(pwd + (string)ps["PasswordSalt"]);
        }

        public void OnRemove(DataGateKey gkey, IDictionary<string, object> ps)
        {
        }
    }
}
