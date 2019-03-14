using DataGate.Com.DB;
using DataGate.Com;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace DataGate.App.DataService
{
    /// <summary>
    /// 关于数据模型配置的提供程序
    /// </summary>
    public class MetaService : ISingleton, IMetaService
    {
        bool _modelInited = false;
        readonly object synObj = new object();
        Dictionary<string, DataGateKey> _dataKeysDict = new Dictionary<string, DataGateKey>();
        Dictionary<string, TableMeta> _tableMetas = new Dictionary<string, TableMeta>();

        //为保证原始添加顺序，不用Dictionary<>
        static List<DataGateEntry> _dataGateEntrys = new List<DataGateEntry>();
        FileSystemWatcher fw;
        string _appDataDir;

        private void LocateAppDataDir()
        {
            _appDataDir = Consts.Config.GetSection("AppConfig:AppDataDir").Value;
            if (_appDataDir.IsEmpty())
            {
#if DEBUG
                //.net core在调试时不会将项目目录作为当前目录
                _appDataDir = Path.Combine(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory
                   ).Parent.Parent.Parent.FullName, "App_Data");
#else
             _appDataDir =Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "App_Data");
#endif
            }
            _appDataDir = new DirectoryInfo(_appDataDir).FullName;
        }

        public MetaService()
        {
            LocateAppDataDir();
            InitTableMetas();
            fw = new FileSystemWatcher(_appDataDir, "*.json");
            fw.EnableRaisingEvents = true;
            fw.Changed += (s, e) =>
            {
                _modelInited = false;
            };
        }
        private void InitTableMetas()
        {
            if (_modelInited) return;
            lock (synObj)
            {
                if (!_modelInited)
                {
                    GetTableMetas();
                    GetDataKeys();
                    CreateDataGateList();
                    _modelInited = true;
                }
            }
        }

        /// <summary>
        /// 根据已注册的IDataGate的字典构造实际执行的IDataGate列表
        /// </summary>
        private void CreateDataGateList()
        {
            foreach (var gkey in _dataKeysDict.Values)
            {
                List<IDataGate> dataGates = new List<IDataGate>();
                foreach (var kvEntry in _dataGateEntrys)
                {
                    if (kvEntry.IsMatch(gkey))
                    {
                        dataGates.Add(kvEntry.DataGate);
                    }
                }
                gkey.DataGate = new ListDataGate(dataGates);
            }
        }

        void GetDataKeys()
        {
            List<DataGateKey> allKeys = new List<DataGateKey>();
            foreach (var keyFile in Directory.GetFiles(_appDataDir, "*Keys.json"))
            {
                string json = File.ReadAllText(keyFile);
                List<DataGateKey> keys = JsonConvert.DeserializeObject<List<DataGateKey>>(json);
                keys.ForEach(key =>
                {
                    key.Source = new FileInfo(keyFile).Name;
                    BuildKey(key);
                });

                allKeys.AddRange(keys);
            }
            _dataKeysDict = allKeys.ToDictionary(m => m.Key.ToLower());
        }

        /// <summary>
        /// 构建DataGateKye, 生成其中的某些默认值
        /// </summary>
        /// <param name="key"></param>
        public void BuildKey(DataGateKey key)
        {
            if (key.Model.IsEmpty()) return;
            key.TableJoins = GetJoinInfos(key).ToList();
            var mainModel = key.TableJoins[0].Table;
            //string tableJoins = mainModel.FixDbName;
            string tableJoins = key.TableJoins[0].Alias == null ? mainModel.FixDbName :
               mainModel.FixDbName + " " + key.TableJoins[0].Alias;
            for (var i = 1; i < key.TableJoins.Count; i++)
            {
                string joins = BuildTableJoins(key, i);
                tableJoins += joins;
            };
            key.JoinSubTerm = tableJoins;
            //暂不考虑修改数据时生成表查询字段和语句
            if (key.OpType != DataOpType.Save)
            {
                key.QueryFieldsTerm = BuildQueryFields(key);
            }
        }

        void GetTableMetas()
        {
            _tableMetas.Clear();
            var allTableMetas = new List<TableMeta>();
            foreach (var modelFile in Directory.GetFiles(_appDataDir, "*Models.json"))
            {
                var json = File.ReadAllText(modelFile);
                var tableJson = JToken.Parse(json);

                JArray tableJArr = tableJson as JArray;
                IEnumerable<TableMeta> tableMetas = null;
                if (tableJArr == null) //json文件是字典对象
                {
                    tableMetas = tableJson.ToDictionary().
                    Select(kv =>
                    {
                        var tb = CreateTableMeta(kv.Key, JToken.FromObject(kv.Value));
                        tb.Source = new FileInfo(modelFile).Name;
                        return tb;
                    });
                }
                else //json文件是数组对象
                {
                    tableMetas = tableJArr.Select(jtoken =>
                    {
                        var tb = CreateTableMeta((JObject)jtoken);
                        tb.Source = new FileInfo(modelFile).Name;
                        return tb;
                    });
                }
                allTableMetas.AddRange(tableMetas);
            }
            _tableMetas = allTableMetas.ToDictionary(m => m.Name);

            CheckPlusFields();

        }

        int _callDeepth = 0;
        /// <summary>
        /// 将name 带+号的字段为包含的其他实体定义的字段，进行转换成自身的字段
        /// </summary>
        private void CheckPlusFields()
        {
            foreach (var tm in _tableMetas.Values)
            {
                _callDeepth = 0;
                CheckPlusField(tm);
            }
        }

        private void CheckPlusField(TableMeta tm)
        {
            _callDeepth++;
            if (_callDeepth > 10)
            {
                throw new Exception($"递归层次太多，是否产生了循环引用在模型：'{tm.Name}'?");
            }

            int cnt = tm.Fields.Count;

            //从后往前循环加，是为了让排后面的属性优先级高于前面的同名属性
            for (int i = cnt - 1; i >= 0; i--)
            {
                if (tm.Fields[i].Name.StartsWith("+"))
                {
                    var name = tm.Fields[i].Name.Substring(1);
                    var child = _tableMetas[name];
                    CheckPlusField(child);

                    tm.Fields.RemoveAt(i);
                    int j = i;
                    foreach (var field in child.Fields.ToArray())
                    {
                        //如果已有相同属性则跳过
                        if (tm.Fields.All(f => !f.Name.Equals(field.Name, StringComparison.OrdinalIgnoreCase)))
                            tm.Fields.Insert(j++, field);
                    }
                }
            }
            _callDeepth--;
        }

        private TableMeta CreateTableMeta(JObject metaObj)
        {
            string key = (string)metaObj[nameof(TableMeta.Name)];
            JToken jt = metaObj[nameof(TableMeta.Fields)];
            return CreateTableMeta(key, jt);
        }

        private TableMeta CreateTableMeta(string key, JToken jt) => new TableMeta
        {
            Name = key,
            Fields = ParseMetadata(jt)
            .OrderBy(m => m.Order)
            .ToList()
        };

        /// <summary>
        /// 注册一个数据处理前后的监视程序
        /// </summary>
        /// <param name="key">一个完全匹配的key(只包含字母、数字和下划线）或一个匹配的正则表达式</param>
        /// <param name="dataGate">实现IDataGate接口的处理程序</param>
        public static void RegisterDataGate(string key, IDataGate dataGate)
        {
            key = key.ToLower();
            _dataGateEntrys.Add(new DataGateEntry(key, dataGate));
        }

        /// <summary>
        /// 注册一个数据处理前后的监视程序
        /// </summary>
        /// <param name="key">一个完全匹配的key(只包含字母、数字和下划线）或一个匹配的正则表达式</param>
        /// <param name="dataGate">实现IDataGate接口的处理程序</param>
        public static void RegisterDataGate(Func<DataGateKey, bool> filter, IDataGate dataGate)
        {
            _dataGateEntrys.Add(new DataGateEntry(filter, dataGate));
        }


        //支持逗号分隔的字符串/字符串数组/对象数组/混合数组
        private IEnumerable<FieldMeta> ParseMetadata(JToken jToken)
        {
            if (jToken is JArray) //是字符串数组/对象数组/混合数组
            {
                return jToken.Select(jt =>
                {
                    if (jt is JObject) return ((JObject)jt).ToObject<FieldMeta>();
                    if (jt is JValue) return new FieldMeta
                    {
                        Name = (string)jt
                    };
                    throw new InvalidDataException("json文件分析有误");
                });
            }
            else if (jToken is JValue) //是逗号分隔的字符串
            {
                return ((string)jToken).Split(',').Select(a => a.Trim())
                    .Select(a => new FieldMeta
                    {
                        Name = a
                    });
            }
            else
            {
                throw new InvalidDataException("json文件分析有误");
            }
        }

        /// <summary>
        /// 获取数据访问的Key(大小写不敏感)对应的数据操作对象
        /// 该对象是从字典库复制过来，所以可以在运行中修改其属性值
        /// 而不用担心其他会话会受影响
        /// </summary>
        /// <param name="key">数据访问的Key(大小写不敏感)</param>
        /// <returns>数据操作对象</returns>
        public DataGateKey GetDataKey(string key)
        {
            if (!_modelInited) InitTableMetas();
            key = key.ToLower();
            var gkey = _dataKeysDict[key];
            return new DataGateKey
            {
                DataGate = gkey.DataGate,
                Name = gkey.Name,
                Filter = gkey.Filter,
                Key = gkey.Key,
                Model = gkey.Model,
                OpType = gkey.OpType,
                OrderBy = gkey.OrderBy,
                Sql = gkey.Sql,
                TableJoins = gkey.TableJoins?.ToList(),
                JoinSubTerm = gkey.JoinSubTerm,
                QueryFieldsTerm = gkey.QueryFieldsTerm,
                ConnName = gkey.ConnName,
                Data = gkey.Data,
                Attr = gkey.Attr,
                QueryFields = gkey.QueryFields,
                Source = gkey.Source
            };
        }

        /// <summary>
        /// 根据名称获取表定义(大小写敏感)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TableMeta GetTableMeta(string name)
        {
            if (!_modelInited) InitTableMetas();
            return _tableMetas[name];
        }

        /// <summary>
        /// 分析DataKey中Model属性的字符串，分离出主从各表信息用于
        /// 下一步生成表连接子句
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IEnumerable<JoinInfo> GetJoinInfos(DataGateKey key)
        {
            if (key.Model.IsEmpty())
            {
                return null;
            }
            key.Model = Regex.Replace(key.Model, "\\s+", " ");
            string[] models = key.Model.Replace(">", ">%").Replace("=", "=%").Split('%');
            return models.Select(n =>
            {
                n = n.Trim();
                string joinFlag = null;
                string alias = null;
                string name = null;
                if (n.EndsWith(">") || n.EndsWith("="))
                {
                    joinFlag = n.Last().ToString();
                    n = n.Substring(0, n.Length - 1);
                }
                var arr = n.Split(' ');
                name = arr[0];
                if (arr.Length > 1)
                {
                    alias = arr[1];
                }


                TableMeta tm = null;

                //如果Models.json中没有定义表模型，则找Keys.json中
                //的QueryFields属性生成一个表模型并放到模型字典中
                if (!_tableMetas.ContainsKey(name))
                {
                    if (key.QueryFields.IsEmpty())
                    {
                        throw new Exception($"在解析[Key:{key.Key}]的过程中，没有找到[Model:{name}]的定义：在*Models.json中定义的模型，" +
                            $"或者{nameof(DataGateKey.QueryFields)}中定义的字段列表");
                    }
                    tm = CreateTableMeta(key.Model, new JValue(key.QueryFields));
                    _tableMetas.Add(tm.Name, tm);
                }
                else
                {
                    tm = _tableMetas[name];
                }

                TranslateModelNames(GetTempDB(key), tm);

                var joinInfo = new JoinInfo
                {
                    Name = name,
                    Table = _tableMetas[name],
                    Alias = alias,
                    JoinFlag = joinFlag
                };
                return joinInfo;
            });
        }

        void TranslateModelNames(DBHelper db, TableMeta tm)
        {
            tm.DbName = tm.DbName ?? db.GetDbObjName(tm.Name);
            tm.FixDbName = db.AddFix(tm.DbName);
            foreach (var fm in tm.Fields)
            {
                if (fm.DbName.IsEmpty())
                    fm.DbName = db.GetDbObjName(fm.Name);
                fm.FixDbName = db.AddFix(fm.DbName);
            }
        }

        /// <summary>
        /// 构造第i条join子句
        /// </summary>
        /// <param name="models"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        private string BuildTableJoins(DataGateKey gkey, int idx)
        {
            var models = gkey.TableJoins;
            var db = GetTempDB(gkey);
            string modelRightName = models[idx].Name;
            for (var i = 0; i < idx; i++)
            {
                var modelLeft = models[i].Table;
                var modelRight = models[idx].Table;
                var join = models[idx - 1].JoinFlag == ">" ? " left join"
                      : (models[idx - 1].JoinFlag == "=" ? " inner join" : "");
                var joinField = modelRight.Fields.FirstOrDefault(f => (f.ForeignKey ?? "").StartsWith((models[i].Alias ?? modelLeft.Name) + "."));
                string rightNames = models[idx].Alias == null ? modelRight.FixDbName :
                    modelRight.FixDbName + " " + models[idx].Alias;
                if (joinField == null)
                {
                    joinField = modelLeft.Fields.FirstOrDefault(f => (f.ForeignKey ?? "").StartsWith((models[idx].Alias ?? modelRight.Name) + "."));
                    if (joinField != null)
                    {
                        return $"{join} {rightNames} on" +
                            $" {AddDotFix(db, joinField.ForeignKey)}={models[i].Alias ?? modelLeft.FixDbName}.{joinField.FixDbName}";
                    }
                }
                else
                {
                    return $"{join} {rightNames} on" +
                        $" {AddDotFix(db, joinField.ForeignKey)}={models[idx].Alias ?? modelRight.FixDbName}.{joinField.FixDbName}";
                }
            }
            throw new ArgumentException($"找不到模型{modelRightName}中的{nameof(FieldMeta.ForeignKey)}定义。");
        }

        private string AddDotFix(DBHelper db, string dotName)
        {
            return String.Join(".", dotName.Split('.').Select(n => db.AddFix(n)));
        }

        //构造select后面的字段列表
        private string BuildQueryFields(DataGateKey gkey)
        {
            //当明确指定了QueryFields列时，直接处理该属性值
            if (!gkey.QueryFields.IsEmpty())
            {
                return FormatQueryFields(gkey);
            }
            //当定义了Sql和Model时，从Model中生成查询列表，不加表别名或表名，主要用于前台
            //单击列标题排序时判断由哪个字段排序
            else if (!(gkey.Sql.IsEmpty() || gkey.Model.IsEmpty()))
            {
                return FormatFieldsFromSql(gkey);
            }
            var db = GetTempDB(gkey);
            //否则从TableMeata元数据定义中生成查询字段列表

            var mainModel = gkey.TableJoins[0];
            var mainTable = mainModel.Table;
            List<string> allFields = new List<string>();
            if (gkey.TableJoins.Count > 1)
            {
                var otherTableFields = mainTable.Fields.Where(f => !f.ArrayItemType.IsEmpty());
                foreach (var other in otherTableFields)
                {
                    var tableMeta = _tableMetas[other.ArrayItemType];
                    var otherFields = String.Join(",", tableMeta.Fields.Select(f =>
                    {
                        if (f.IsArray)
                        {
                            return null;
                        }
                        //过滤掉自定义操作列
                        else if (f.UIType == Consts.OperatorUIType)
                        {
                            return null;
                        }
                        else if (!f.ForeignField.IsEmpty())
                        {
                            return $"{db.AddFix(f.ForeignField)} {f.Name}";
                        }
                        else
                        {
                            //带[]号的datatype中表示要查子表字段，用别名区分子表字段
                            return $"{tableMeta.FixDbName}.{f.FixDbName}";
                        }
                    }).Where(f => !f.IsEmpty()));
                    allFields.Add(otherFields);
                }
            }
#if DEBUG
            if (mainTable.Name == "WProject")
            {

            }
#endif
            var mainFields = string.Join(",", mainTable.Fields.Select(f =>
            {
                if (f.IsArray)
                {
                    return null;
                }
                //过滤掉自定义操作列
                else if (f.UIType == Consts.OperatorUIType)
                {
                    return null;
                }
                else if (!f.ForeignField.IsEmpty())
                {
                    //单表查询不考虑联查字段
                    if (gkey.TableJoins.Count <= 1) return null;

                    //排除掉没有参与联查的表的字段
                    if (gkey.TableJoins.All(tj =>
                    !f.ForeignField.StartsWith((tj.Alias ?? tj.Name) + ".", StringComparison.OrdinalIgnoreCase)))
                    {
                        return null;
                    }

                    return $"{AddDotFix(db, f.ForeignField)} {f.Name}";

                }
                else
                {
                    return $"{mainModel.Alias ?? mainTable.FixDbName}.{f.FixDbName}";
                }
            }).Where(f => !f.IsEmpty()));
            allFields.Insert(0, mainFields);
            return String.Join(",", allFields);
        }

        private string FormatQueryFields(DataGateKey gkey)
        {
            return String.Join(",", gkey.QueryFields.Split(',').Select(f =>
            {
                return String.Join(".", f.Split('.').Select(s => GetTempDB(gkey).AddFix(s.Trim())));
            }));
        }

        private string FormatFieldsFromSql(DataGateKey gkey)
        {
            return String.Join(",", gkey.TableJoins[0].Table.Fields.Select(f => f.FixDbName));
        }

        Dictionary<string, DBHelper> _tempDict = new Dictionary<string, DBHelper>();

        private DBHelper GetTempDB(DataGateKey key)
        {
            string connName = key.ConnName ?? "Default";
            if (!_tempDict.ContainsKey(connName))
            {
                _tempDict[connName] = DBFactory.CreateDBHelper(connName);
            }
            return _tempDict[connName];
        }
    }

    /// <summary>
    /// 用于注册DataGate的内部类
    /// </summary>
    class DataGateEntry
    {
        public DataGateEntry(string key, IDataGate dataGate)
        {
            Key = key;
            DataGate = dataGate;
        }

        public DataGateEntry(Func<DataGateKey, bool> filter, IDataGate dataGate)
        {
            Filter = filter;
            DataGate = dataGate;
        }

        public string Key { get; set; }

        public IDataGate DataGate { get; set; }

        public Func<DataGateKey, bool> Filter { get; set; }

        public bool IsMatch(DataGateKey gkey)
        {
            if (Filter != null)
            {
                return Filter(gkey);
            }
            string pattern = Key;
            if (pattern.IsVariableName())
            {
                pattern = '^' + pattern + "$";
            }
            if (Regex.IsMatch(gkey.Key, pattern, RegexOptions.IgnoreCase))
            {
                return true;
            }
            return false;
        }
    }
}
