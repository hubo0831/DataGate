using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGate.Com
{
    /// <summary>  
    /// JObject扩展  
    /// 抄自：http://blog.csdn.net/shiyaru1314/article/details/53840983
    /// </summary>  
    public static class JObjectExtensions
    {
        /// <summary>  
        /// 将JObject转化成字典  
        /// </summary>  
        /// <param name="json"></param>  
        /// <returns></returns>  
        public static IDictionary<string, object> ToDictionary(this JToken json)
        {
            var propertyValuePairs = json.ToObject<Dictionary<string, object>>();
            ProcessJObjectProperties(propertyValuePairs);
            ProcessJArrayProperties(propertyValuePairs);
            return propertyValuePairs;
        }

        private static void ProcessJObjectProperties(IDictionary<string, object> propertyValuePairs)
        {
            var objectPropertyNames = (from property in propertyValuePairs
                                       let propertyName = property.Key
                                       let value = property.Value
                                       where value is JObject
                                       select propertyName).ToList();

            objectPropertyNames.ForEach(propertyName => propertyValuePairs[propertyName] = ToDictionary((JObject)propertyValuePairs[propertyName]));
        }

        private static void ProcessJArrayProperties(IDictionary<string, object> propertyValuePairs)
        {
            var arrayPropertyNames = (from property in propertyValuePairs
                                      let propertyName = property.Key
                                      let value = property.Value
                                      where value is JArray
                                      select propertyName).ToList();

            arrayPropertyNames.ForEach(propertyName => propertyValuePairs[propertyName] = ToArray((JArray)propertyValuePairs[propertyName]));
        }

        /// <summary>  
        ///   
        /// </summary>  
        /// <param name="array"></param>  
        /// <returns></returns>  
        public static object[] ToArray(this JArray array)
        {
            return array.ToObject<object[]>().Select(ProcessArrayEntry).ToArray();
        }

        /// <summary>
        /// 将Json数组转换成DataTable
        /// </summary>
        /// <param name="array">Json数组</param>
        /// <param name="metadata">元数据定义</param>
        /// <returns></returns>
        public static DataTable ToDataTable(this JArray array, JArray metadata)
        {
            DataTable table = new DataTable();
            //构造表头
            foreach (JToken jkon in metadata)
            {
                string name = jkon["name"].Value<string>();
                DataColumn dc = new DataColumn(name);
                dc.Caption = (jkon["title"] ?? jkon["name"]).Value<string>();
                table.Columns.Add(dc);
            }
            //向表中添加数据
            for (int i = 0; i < array.Count; i++)
            {
                DataRow row = table.NewRow();
                JObject obj = array[i] as JObject;
                foreach (JToken jkon in obj.AsEnumerable<JToken>())
                {

                    string name = ((JProperty)(jkon)).Name;
                    if (!table.Columns.Contains(name))
                    {
                        continue;
                    }
                    var value = ((JProperty)(jkon)).Value;
                    var arr = value as JArray;
                    if (arr != null)
                    {
                        row[name] = String.Join(",", arr.ToArray().Select(a => a.ToString()));
                    }
                    else
                    {
                        row[name] = value.ToString();
                    }
                }
                table.Rows.Add(row);
            }
            return table;
        }

        private static object ProcessArrayEntry(object value)
        {
            if (value is JObject)
            {
                return ToDictionary((JObject)value);
            }
            if (value is JArray)
            {
                return ToArray((JArray)value);
            }
            return value;
        }

    }
}
