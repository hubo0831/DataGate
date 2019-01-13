using DataGate.App.DataService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DataGate.Com;
using Newtonsoft.Json.Linq;

namespace DataGate.App.DataService
{
    /// <summary>
    /// 将字典项翻译成文字
    /// </summary>
    public class DefaultExportGate : ISingleton
    {
        public Dictionary<string, Dictionary<string, string>> Dict { get; set; }

        public DefaultExportGate()
        {
            DataGateService service = Consts.Get<DataGateService>();
            DataTable dt = (DataTable)service.QueryAsync("GetSysDict", new { }).Result;

            Dict = dt.Rows.Cast<DataRow>().Select(dr => new
            {
                parentCode = dr.ToStr("parentCode"),
                code = dr.ToStr("code"),
                name = dr.ToStr("name")
            }).GroupBy(g => g.parentCode)
            .ToDictionary(g => g.Key, g => g.ToDictionary(c => c.code, c => c.name));
        }

        /// <summary>
        /// 将输出转换成DataTable，并将字典的列值代替字典代码
        /// </summary>
        /// <param name="gkey"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public DataTable OnExport(DataGateKey gkey, object result)
        {
            var fields = gkey.TableJoins[0].Table.Fields
             .Where(f => (f.Order ?? 0) >= 0);
            DataTable dt = result as DataTable;
            if (dt != null)
            {
                for (int i = dt.Columns.Count - 1; i >= 0; i--)
                {
                    var dc = dt.Columns[i];
                    var meta = fields.FirstOrDefault(f => f.Name.Equals(dc.ColumnName, StringComparison.OrdinalIgnoreCase));
                    if (meta != null && meta.Order < 0)
                    {
                        dt.Columns.Remove(dc);
                    }
                }
            }
            else if (result is JArray jarr)
            {
                dt = jarr.ToDataTable(JArray.FromObject(fields));
            }

            //处理字典的列
            var dictFields = fields.Where(f => f.UIType == "DictList").ToList();
            //   if (dictFields.Count == 0) return;

            foreach (var field in dictFields)
            {
                if (!dt.Columns.Contains(field.Name)) continue;
                var group = field.Options.FirstOrDefault()?.ToString();
                if (group.IsEmpty() || !Dict.ContainsKey(group)) continue;
                var groupDict = Dict[group];
                foreach (DataRow dr in dt.Rows)
                {
                    string key = dr.ToStr(field.Name);
                    if (key.IsEmpty()) continue;
                    dr[field.Name] = String.Join(", ", key.Split(',')
                         .Select(k => groupDict.ContainsKey(k) ? groupDict[k] : k));
                }
            }

            //格式化日期的列
            var dateFields = fields.Where(f => f.UIType == "Date" || f.DataType == "Date"
            || f.DataType == "DateTIme" || f.UIType == "DateTime").ToList();

            foreach (var field in dateFields)
            {
                if (!dt.Columns.Contains(field.Name)) continue;

                foreach (DataRow dr in dt.Rows)
                {
                    var key = dr.ToValue<DateTime>(field.Name);
                    if (key.IsDefault()) continue;
                    dr[field.Name] =
                        key.ToString((field.UIType == "Date" || field.DataType == "Date")
                        ? "yyyy-MM-dd" : "yyyy-MM-dd HH:mm:ss");
                }
            }

            return dt;
        }
    }
}
