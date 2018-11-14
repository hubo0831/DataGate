using DataGate.Com;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataGate.App.DataService
{
    public class UsersGate : IDataGate
    {
        public void OnAdd(List<string> fields, IDictionary<string, object> ps)
        {
            ps["PasswordSalt"] = CommOp.NewId();
            ps["Password"] = Encryption.MD5("123456" +  ps["PasswordSalt"]);
            fields.Add("Password");
            fields.Add("PasswordSalt");
            ps["createDate"] = DateTime.Now;
        }

        public void OnChange(List<string> fields, IDictionary<string, object> ps)
        {
            if (!ps.ContainsKey("id"))
            {
                
            }
        }

        public void OnRemove(IDictionary<string, object> ps)
        {
        }
    }
}
