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

namespace DataGate.App.DataService
{
    /// <summary>
    /// 关于数据模型配置的提供程序
    /// </summary>
    public class MetaService : ISingleton, IMetaService
    {
        DBHelper _db;
        bool _modelInited = false;
        readonly object synObj = new object();
        Dictionary<string, DataGateKey> _dataKeysDict = new Dictionary<string, DataGateKey>();
        Dictionary<string, TableMeta> _tableMetas = new Dictionary<string, TableMeta>();
        Dictionary<string, IDataGate> _dataGateDict = new Dictionary<string, IDataGate>();
        FileSystemWatcher fw;
#if DEBUG
        //.net core在调试时不会将项目目录作为当前目录
        string _appDataDir = Path.Combine(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory
            ).Parent.Parent.Parent.FullName, "App_Data");
#else
        string _appDataDir =Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "App_Data");
#endif

        public MetaService(DBHelper db)
        {
            _db = db;
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
                    _modelInited = true;
                }
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
                    key.TableJoins = GetJoinInfos(key).ToArray();
                    var mainModel = key.TableJoins[0].Table;
                    //string tableJoins = mainModel.FixDbName;
                    string tableJoins = key.TableJoins[0].Alias == null ? mainModel.FixDbName :
                       mainModel.FixDbName + " " + key.TableJoins[0].Alias;
                    for (var i = 1; i < key.TableJoins.Length; i++)
                    {
                        string joins = BuildTableJoins(key.TableJoins, i);
                        tableJoins += joins;
                    };
                    key.JoinSubTerm = tableJoins;
                    //暂不考虑修改数据时生成表查询字段和语句
                    if (key.OpType != DataOpType.Save)
                    {
                        key.QueryFieldsTerm = BuildQueryFields(key);
                    }
                });

                allKeys.AddRange(keys);
            }
            _dataKeysDict = allKeys.ToDictionary(m => m.Key.ToLower());
        }

        void GetTableMetas()
        {
            _tableMetas.Clear();
            foreach (var modelFile in Directory.GetFiles(_appDataDir, "*Models.json"))
            {
                var json = File.ReadAllText(modelFile);
                var modelTypeDict = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(json);

                modelTypeDict.Select(kv => CreateTableMeta(kv.Key, kv.Value))
                    .Each(tm => _tableMetas.Add(tm.Name, tm));
            }
        }

        private TableMeta CreateTableMeta(string key, JToken jt) => new TableMeta
        {
            Name = key,
            DbName = _db.GetDbObjName(key),
            FixDbName = _db.AddFix(key),
            Fields = ParseMetadata(jt).ToArray().Each(fm =>
            {
                fm.DbName = _db.GetDbObjName(fm.Name);
                fm.FixDbName = _db.AddFix(fm.Name);
            })
        };

        /// <summary>
        /// 注册一个数据处理前后的监视程序
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataGate"></param>
        public void RegisterDataGate(string key, IDataGate dataGate)
        {
            if (!_dataGateDict.ContainsKey(key.ToLower()))
            {
                _dataGateDict.Add(key.ToLower(), dataGate);
            }
            else
            {
                _dataGateDict[key.ToLower()] = dataGate;
            }
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
        /// 获取数据访问的Key(大小写不敏感)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public DataGateKey GetDataKey(string key)
        {
            if (!_modelInited) InitTableMetas();
            key = key.ToLower();
            var gkey = _dataKeysDict[key];
            if (gkey.DataGate == null && _dataGateDict.ContainsKey(key))
            {
                gkey.DataGate = _dataGateDict[key];
            }
            return new DataGateKey
            {
                DataGate = gkey.DataGate,
                Filter = gkey.Filter,
                Key = gkey.Key,
                Model = gkey.Model,
                OpType = gkey.OpType,
                OrderBy = gkey.OrderBy,
                Sql = gkey.Sql,
                TableJoins = gkey.TableJoins,
                JoinSubTerm = gkey.JoinSubTerm,
                QueryFieldsTerm = gkey.QueryFieldsTerm
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

        IEnumerable<JoinInfo> GetJoinInfos(DataGateKey key)
        {
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
                if (!_tableMetas.ContainsKey(name))
                {
                    if (key.QueryFields.IsEmpty())
                    {
                        throw new Exception($"没有以下二者之一：在*Models.json中定义的模型，" +
                            $"或者{nameof(DataGateKey.QueryFields)}中定义的字段列表");
                    }
                    var tm = CreateTableMeta(key.Model, new JValue(key.QueryFields));
                    _tableMetas.Add(tm.Name, tm);
                }
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

        /// <summary>
        /// 构造第i条join子句
        /// </summary>
        /// <param name="models"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        private string BuildTableJoins(JoinInfo[] models, int idx)
        {
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
                            $" {_db.AddFix(joinField.ForeignKey)}={models[i].Alias ?? modelLeft.FixDbName}.{joinField.FixDbName}";
                    }
                }
                else
                {
                    return $"{join} {rightNames} on" +
                        $" {_db.AddFix(joinField.ForeignKey)}={models[idx].Alias ?? modelRight.FixDbName}.{joinField.FixDbName}";
                }
            }
            throw new ArgumentException($"找不到模型{modelRightName}中的{nameof(FieldMeta.ForeignKey)}定义。");
        }

        //构造select后面的字段列表
        private string BuildQueryFields(DataGateKey gkey)
        {
            if (!gkey.QueryFields.IsEmpty())
            {
                return FormatQueryFields(gkey);
            }
            var mainModel = gkey.TableJoins[0];
            var mainTable = mainModel.Table;
            List<string> allFields = new List<string>();
            if (gkey.TableJoins.Length > 1)
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
                            return $"{_db.AddFix(f.ForeignField)} {f.Name}";
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
                    if (gkey.TableJoins.Length <= 1) return null;

                    //排除掉没有参与联查的表的字段
                    if (gkey.TableJoins.All(tj =>
                    !f.ForeignField.StartsWith((tj.Alias ?? tj.Name) + ".", StringComparison.OrdinalIgnoreCase)))
                    {
                        return null;
                    }

                    return $"{_db.AddFix(f.ForeignField)} {f.Name}";

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
                return String.Join(".", f.Split('.').Select(s=> _db.AddFix(s.Trim())));
            }));
        }
    }
}
