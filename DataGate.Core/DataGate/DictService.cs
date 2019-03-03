using DataGate.App.DataService;
using DataGate.Com.DB;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using DataGate.Com;
using System.Collections;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.IO;

namespace DataGate
{
    /// <summary>
    /// 数据字典服务，提供数据字典、数据配置文件的生成
    /// </summary>
    public class DictService
    {
        IDictFor _dictFor;
        DBHelper _db;
        INameConverter _nameConverter;
        DictSettings _settings;
        List<ColumnMeta> _sourceMetaList;
        public DictService(IDictFor dictFor, INameConverter nameConverter, DictSettings settings)
        {
            _dictFor = dictFor;
            _db = DBFactory.CreateDBHelper("Default");
            _db.DbNameConverter = null; //不转换字典sql中输出的字段名
            _nameConverter = nameConverter;
            _settings = settings;
        }

        public void Run()
        {
            GetSourceMetaList();
            if (_settings.CreateModels)
            {
                CreateTableModels();
            }
            if (_settings.CreateKeys)
            {
                CreateTableKeys();
            }
            if (_settings.CreateDict)
            {
                CreateDict();
            }
        }

        private void GetSourceMetaList()
        {
            _sourceMetaList = _db.GetSqlListAsync<ColumnMeta>(_dictFor.DictSql).Result;
            IEnumerable<ColumnMeta> tb1 = new List<ColumnMeta>();
            IEnumerable<ColumnMeta> tb2 = new List<ColumnMeta>();
            if (!_settings.Tables.IsEmpty())
            {
                tb1 = _sourceMetaList.Where(col =>
                _settings.Tables.Contains(col.TableName, StringComparer.OrdinalIgnoreCase));
            }
            if (!_settings.TableReg.IsEmpty())
            {
                Regex tbReg = new Regex(_settings.TableReg, RegexOptions.IgnoreCase);
                tb2 = _sourceMetaList.Where(col => tbReg.IsMatch(col.TableName))
                .ToList();
            }
            _sourceMetaList = tb1.Union(tb2).ToList();
        }

        void CreateTableModels()
        {
            var tables = _sourceMetaList
                .GroupBy(col => col.TableName);
            //new TableMeta, 为简化序列化后的json结果，用匿名对象
            var result = tables.Select(tb =>
             new
             {
                 Name = _nameConverter.ToPropName(tb.Key),
                 Remark = tb.FirstOrDefault()?.Remark,
                 Fields = tb.Select(f => new
                 {
                     Name = _nameConverter.ToPropName(f.ColumnName),
                     f.Title,
                     DataType = MapDataTypes(f.TypeName, CommOp.ToInt(f.MaxLength), CommOp.ToInt(f.Precision)),
                     f.PrimaryKey,
                     Order = tb.ToList().IndexOf(f) + 1,
                     f.MaxLength,
                     f.Required,
                     Value = f.Value.IsEmpty() ? null : f.Value,
                 }).ToList()
             });
            string output = JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            });
            Console.WriteLine(output);

            if (!_settings.ModelFile.IsEmpty())
            {
                File.WriteAllText(_settings.ModelFile, output);
            }
        }

        void CreateTableKeys()
        {
            var tables = _sourceMetaList
                .GroupBy(col => col.TableName);
            //new DataGateKey, 为简化序列化后的json结果，用匿名对象
            var result = tables.Select(tb => new
            {
                Key = "Get" + FirstUpper(_nameConverter.ToPropName(tb.Key)),
                tb.First().Remark,
                Name = tb.First().Remark,
                Model = _nameConverter.ToPropName(tb.Key),
                OpType = DataOpType.GetArray.ToString(),
                ConnName = "Default",
            });
            string output = JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            });
            Console.WriteLine(output);
            if (!_settings.KeyFile.IsEmpty())
            {
                File.WriteAllText(_settings.KeyFile, output);
            }
        }

        private string FirstUpper(string key)
        {
            return key.First().ToUpper() + key.Substring(1);
        }

        void CreateDict()
        {
            Console.WriteLine("尚未实现");
        }

        string MapDataTypes(string columnType, int length, int precision)
        {
            switch (columnType.ToLower())
            {
                case "varchar2":
                case "nvarchar2":
                default:
                    if (length > 200)
                        return "Text";
                    else
                        return "String";
                case "date":
                    return "Date";
                case "number":
                case "integer":
                case "numeric":
                case "int":
                case "double":
                case "float":
                case "longint":
                case "long":
                    if (length == 1)
                        return "Boolean";
                    else
                        return "Number";

            }
        }
    }
}
