using DataGate.Com;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGate.Com.DB
{
    /// <summary>
    /// 将Pascal命名规则的属性名转成带下划线全大写的字段名
    /// </summary>
    public class UpperNameConverter : INameConverter
    {
        /// <summary>
        /// 将属性Pascal命名方式转为全大写，并添加下划线在原来大写字母前面
        /// </summary>
        /// <param name="pascalName"></param>
        /// <returns></returns>
        public string ConvertToDBName(string pascalName)
        {
            List<char> chars = new List<char>();
            for (int i = 0; i < pascalName.Length; i++)
            {
                var c = pascalName[i];
                var cc = c.ToUpper();
                if (c.IsLower() && chars.Count > 1
                    && pascalName[i - 1].IsUpper() && chars[chars.Count - 2] != '_')
                {
                    chars.Insert(chars.Count - 1, '_');
                }
                else if (chars.Count > 1 && pascalName[i - 1].IsLower() && c.IsUpper())
                {
                    chars.Add('_');
                }
                chars.Add(cc);
            }

            return new string(chars.ToArray());
        }
    }
}
