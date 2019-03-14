using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataGate.App.DataService
{
    public static class DataGateExtensions
    {
        /// <summary>
        /// 根据名称查找字段，不分大小写
        /// </summary>
        /// <param name="gkey"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FieldMeta GetField(this DataGateKey gkey, string name)
        {
            return gkey.MainTable.Fields.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 根据名称查找字段，不分大小写
        /// </summary>
        /// <param name="gkey"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FieldMeta GetField(this TableMeta tableMeta, string name)
        {
            return tableMeta.Fields.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
