using DataGate.Com.DB;
using DataGate.Com;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Data.Common;
using DataGate.Com.Logs;

namespace DataGate.App.DataService
{
    /// <summary>
    /// 数据集中服务
    /// 由于DBHelper中事务的存在，这个服务不能为单例
    /// </summary>
    public class DataGateService : IDisposable
    {
        IMetaService _ms;

        /// <summary>
        /// 在事务中的DBHelper
        /// </summary>
        public DBHelper DB
        {
            get; private set;
        }

        /// <summary>
        /// 用户会话信息
        /// </summary>
        public UserSession Session { get; set; }

        public DataGateService(IMetaService ms)
        {
            _ms = ms;
        }

        /// <summary>
        /// 获取Key对应定义的元数据,返回给客户端 
        /// </summary>
        /// <param name="key">客户端提供的Key</param>
        /// <returns></returns>
        public IEnumerable<FieldMeta> Metadata(string key)
        {
            DataGateKey gkey = GetDataGate(key);

            //应付Model定义中有>号或=号隔开的多个名称
            return gkey.TableJoins[0].Table.Fields;
        }

        private DataGateKey GetDataGate(string key)
        {
            var gkey = _ms.GetDataKey(key);
            //如果是多数据库，则需要在*Keys.json中配置数据库连接名称ConnName

            //注意这里在单个生命周期内_db只能有一个
            if (DB == null)
            {
                DB = DBFactory.CreateDBHelper(gkey.ConnName ?? "Default");
                DB.Log = (sql, ps) =>
                {
                    LogAction?.Invoke(gkey, sql, ps);
                };
            }
            gkey.DataService = this; //将DB对象传递给可能的DataGate前后切面数据处理程序
            return gkey;
        }

        /// <summary>
        /// 提供记录日志的委托
        /// </summary>
        public Action<DataGateKey, string, IDataParameter[]> LogAction { get; set; }

        /// <summary>
        /// 获取指定表指定条件的数据列表
        /// </summary>
        /// <param name="key"></param>
        /// <param name="paramObj">字典对象或实体对象</param>
        /// <returns></returns>
        public async Task<object> QueryAsync(string key, object paramObj)
        {
            DataGateKey gkey = GetDataGate(key);

            if (!(paramObj is IDictionary<string, object> param))
            {
                param = CommOp.ToStrObjDict(paramObj);
            }
            object result = gkey.Data;
            if (result != null)
            {
                return result;
            }

            gkey.DataGate.OnQuery(gkey, param);

            switch (gkey.OpType)
            {
                case DataOpType.GetArray:
                    result = await GetArrayAsync(gkey, param);
                    break;
                case DataOpType.GetObject:
                    result = await GetObjectAsync(gkey, param);
                    break;
                case DataOpType.GetPage:
                    result = await GetPageAsync(gkey, param);
                    break;
                default:
                    throw new ArgumentException("key=GetArray, GetObject or GetPage");
            }
            gkey.DataGate.OnResult(gkey, result);
            return result;
        }

        async Task<object> QueryForExportAsync(DataGateKey gkey, Dictionary<string, object> param)
        {
            object result = gkey.Data;
            if (result != null)
            {
                return result;
            }
            switch (gkey.OpType)
            {
                case DataOpType.GetPage:
                case DataOpType.GetArray:
                    result = await GetArrayAsync(gkey, param);
                    break;
                case DataOpType.GetObject:
                    throw new NotSupportedException("不支持单个对象导出");
                default:
                    throw new ArgumentException("key=GetArray or GetPage");
            }
            return result;
        }

        /// <summary>
        /// 通用导出成Excel v0.2.0+
        /// </summary>
        /// <param name="key"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<Stream> GetExcelStreamAsync(string key, Dictionary<string, object> param)
        {
            DataGateKey gkey = GetDataGate(key);
            var result = await QueryForExportAsync(gkey, param);
            var defaultGate = Consts.Get<DefaultExportGate>();
            DataTable dt = defaultGate.OnExport(gkey, result);
            gkey.DataGate.OnExport(gkey, dt);
            return ExcelHelper.ExportByEPPlus(dt);
        }

        /// <summary>
        /// 执行非查询语句 v0.2.0+
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="param">参数</param>
        /// <returns>受影响的行数</returns>
        public async Task<int> NonQueryAsync(string key, Dictionary<string, object> param)
        {
            DataGateKey gkey = GetDataGate(key);
            object result = gkey.Data;
            if (result != null)
            {
                return CommOp.ToInt(result);
            }
            gkey.DataGate.OnQuery(gkey, param);
            int i = 0;
            if (gkey.Sql.IsEmpty())
            {
                throw new NoNullAllowedException("在执行NonQuery命令时，Sql是必须的");
            }
            var ps = DB.GetParameter(param);
            i = await DB.ExecNonQueryAsync(gkey.Sql, ps.ToArray());
            gkey.DataGate.OnResult(gkey, i);
            return i;
        }

        /// <summary>
        /// 执行非查询语句，用于内部调用
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public async Task<int> NonQueryAsync(string key, object param)
        {
            return await NonQueryAsync(key, CommOp.ToStrObjDict(param));
        }

        private async Task RecurDelete(DataGateKey gkey, DataSubmitRequest request)
        {
            //先删子表
            foreach (var detail in request.Details)
            {
                var dkey = GetDataGate(detail.Key);
                await RecurDelete(dkey, detail);
            }

            if (!request.Removed.IsEmpty())
            {
                await DeleteManyAsync(gkey, request.Removed);
            }
        }

        private async Task<IEnumerable<string>> RecurUpdate(DataGateKey gkey, DataSubmitRequest request)
        {
            IEnumerable<string> ids = new string[0];
            if (!request.Added.IsEmpty())
            {
                ids = await InsertManyAsync(gkey, request.Added);
            }
            if (!request.Changed.IsEmpty())
            {
                await UpdateManyAsync(gkey, request.Changed);
            }

            //后插子表
            foreach (var detail in request.Details)
            {
                var dkey = GetDataGate(detail.Key);
                await RecurUpdate(dkey, detail);
            }
            return ids;
        }

        /// <summary>
        /// 批量执行增删改操作,是此系统的核心方法
        /// </summary>
        /// <param name="key">客户端提交的Key值，代表执行的操作</param>
        /// <param name="request">请求参数</param>
        /// <returns>如有新增操作，则返回新增的对象ID列表</returns>
        public async Task<IEnumerable<string>> SubmitAsync(string key, DataSubmitRequest request)
        {
            DataGateKey gkey = GetDataGate(key);

            gkey.DataGate.OnSubmit(gkey, request);

            //有测试数据时直接返回测试数据
            if (gkey.Data != null)
            {
                return gkey.Data.Select(jk => jk.ToString());
            }

            bool isStartCall = !DB.InTrans;

            if (isStartCall) DB.BeginTrans();

            try
            {
                //先递归删除， 先删子表
                await RecurDelete(gkey, request);

                //再递归增改， 先增改主表
                return await RecurUpdate(gkey, request);
            }
            catch (Exception)
            {
                DB.RollbackTrans();
                throw;
            }
            finally
            {
                if (isStartCall && DB.InTrans) DB.EndTrans();
            }
        }

        /// <summary>
        /// 批量执行增删改操作, 使用一个匿名对象转成DataSubmitRequest
        /// 用于服务端内部调用
        /// </summary>
        /// <param name="key"></param>
        /// <param name="requestObj">匿名对象</param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> SubmitAsync(string key, object requestObj)
        {
            DataSubmitRequest request = JObject.FromObject(requestObj).ToObject<DataSubmitRequest>();
            return await SubmitAsync(key, request);
        }

        private async Task<IEnumerable<string>> InsertManyAsync(DataGateKey gkey, JArray added)
        {
            List<string> ids = new List<string>();
            foreach (var jToken in added)
            {
                ids.Add(await InsertOneAsync(gkey, jToken));
            }
            return ids;
        }

        private async Task UpdateManyAsync(DataGateKey gkey, JArray changed)
        {
            foreach (var jToken in changed)
            {
                await UpdateAsync(gkey, jToken);
            }
        }

        private async Task DeleteManyAsync(DataGateKey gkey, JArray removed)
        {
            foreach (var jToken in removed)
            {
                await DeleteOneAsync(gkey, jToken);
            }
        }

        //将filter条件中的对象属性名换成数据库字段名
        private string FormatFilter(string filter, params TableMeta[] tableMetas)
        {
            if (filter.IsEmpty()) return null;
            foreach (var tableMeta in tableMetas)
            {
                Regex reg = new Regex($"([^\\w@]+|^){tableMeta.Name}(\\W+|$)", RegexOptions.IgnoreCase);

                if (reg.IsMatch(filter))
                {
                    string suffixDbName = tableMeta.FixDbName;
                    //表的逻辑名称和物理名称不对应的情况下，在字段名前加别名前缀，否则加物理名称前缀
                    if (IsVirtualTable(tableMeta))
                    {
                        suffixDbName = tableMeta.Name;
                    }
                    filter = reg.Replace(filter, $"$1{suffixDbName}$2");
                }

                foreach (var field in tableMeta.Fields)
                {
                    string prefixReg = DB.DBComm.FieldPrefix.IsEmpty() ? "" : $"\\{DB.DBComm.FieldPrefix}";
                    //@号表示排除参数，取查询子句后面的属性名称, 加入_db.DBComm.FieldPrefix
                    //以防止多表时，两表字段名相同时的重复替换
                    reg = new Regex($"([^\\w@{prefixReg}]+|^){field.Name}(\\W+|$)", RegexOptions.IgnoreCase);

                    if (reg.IsMatch(filter))
                    {
                        filter = reg.Replace(filter, $"$1{field.FixDbName}$2");
                    }
                }
            }
            return filter;
        }

        private async Task<int> DeleteOneAsync(DataGateKey gkey, JToken jToken)
        {
            var tableMeta = gkey.MainTable;
            IDictionary<string, object> psin = jToken.ToDictionary();
            var fields = tableMeta.PrimaryKeys.Select(pk => pk.Name)
                .Intersect(psin.Select(kv => kv.Key),
                 StringComparer.OrdinalIgnoreCase).ToList();
            gkey.DataGate.OnRemove(gkey, psin);
            var ps = fields.Select(f =>
             {
                 var ff = tableMeta.GetField(f);
                 var psKey = psin.Keys.First(key => key.Equals(f, StringComparison.OrdinalIgnoreCase));
                 return DB.CreateParameter(ff.DbName, psin[psKey]);
             }).ToList();

            string filter = CreateKeyFilter(tableMeta);
            string sql = $"delete from {tableMeta.FixDbName} where {filter}";

            int r = await DB.ExecNonQueryAsync(sql, ps.ToArray());
            if (r > 1)
            {
                DB.RollbackTrans();
                throw new InvalidOperationException("错误操作，根据ID删除的记录数过多");
            }
            gkey.DataGate.OnRemoved(gkey, psin);
            return r;
        }

        //在新增记录时，对于单一主键的记录，检查主键字段是否自增或是guid,是否值为空,
        //如果是自增，则去掉传过来的值，由库自动生成，如果是guid并且为空，则生成一个guid
        private void CheckPrimaryKey(TableMeta tableMeta, IDictionary<string, object> psin, out string id, out string getMaxIdSql)
        {
            id = null;
            getMaxIdSql = null;
            var pKeyField = tableMeta.PrimaryKey;
            if (pKeyField == null) return;

            if (pKeyField.DataType == "Number")
            {
                getMaxIdSql = $"select max({pKeyField.FixDbName}) from {tableMeta.FixDbName}";
            }

            var pkey = psin.Keys.FirstOrDefault(k => k.Equals(pKeyField.Name, StringComparison.OrdinalIgnoreCase));

            //没有传主键字段过来
            if (pkey == null)
            {
                pkey = pKeyField.Name;
                psin.Add(pkey, null);
            }

            //当主键为Number型时，认为是自增字段，为空时从参数中去掉，让数据库自动生成
            if (pKeyField.DataType == "Number")
            {
                psin.Remove(pkey);
            }
            //非number型，并且为空，则认为是32位的guid字符串，为它自动生成
            else if (CommOp.IsEmpty(psin[pkey]))
            {
                id = CommOp.NewId();
                psin[pkey] = id;
            }
            else
            {
                id = CommOp.ToStr(psin[pkey]);
            }
        }

        /// <summary>
        /// 插入单条记录
        /// </summary>
        /// <param name="gkey"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        private async Task<string> InsertOneAsync(DataGateKey gkey, JToken jToken)
        {
            var tableMeta = gkey.MainTable;
            IDictionary<string, object> psin = jToken.ToDictionary();
            CheckPrimaryKey(tableMeta, psin, out string id, out string getMaxIdSql);
            gkey.DataGate.OnAdd(gkey, psin);
            List<string> fields = tableMeta.Fields.Where(f => !f.IsArray && f.ForeignField.IsEmpty())
                .Select(f => f.Name).Intersect(psin.Select(kv => kv.Key),
                 StringComparer.OrdinalIgnoreCase).ToList();
            var ps = fields.Select(f =>
            {
                //集合字段不进入Insert语句
                var ff = tableMeta.Fields.FirstOrDefault(fd => fd.Name.Equals(f, StringComparison.OrdinalIgnoreCase));
                if (ff != null && ff.IsArray) return null;
                //外键字段pass掉
                if (ff != null && !ff.ForeignField.IsEmpty()) return null;
                var psKey = psin.Keys.First(key => key.Equals(f, StringComparison.OrdinalIgnoreCase));
                return new
                {
                    f = ff.FixDbName,
                    p = DB.CreateParameter(ff.DbName, psin[psKey]),
                };
            }).Where(p => p.f != null);
            string strFields = String.Join(",", ps.Select(p => p.f));
            string strValues = String.Join(",", ps.Select(p => '@' + p.p.ParameterName));
            string sql = $"insert into {tableMeta.FixDbName} ({strFields}) values({strValues})";

            await DB.ExecNonQueryAsync(sql, ps.Select(p => p.p).ToArray());

            if (!getMaxIdSql.IsEmpty())
            {
                id = CommOp.ToStr(await DB.ExecGetObjectAsync(getMaxIdSql));
                psin[id] = id;
            }

            gkey.DataGate.OnAdded(gkey, psin);

            return id;
        }

        /// <summary>
        /// 插入新记录，主要用于非API的服务端内部调用
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public async Task<string> InsertOneAsync(string key, object ps)
        {
            var gkey = GetDataGate(key);
            var jToken = JObject.FromObject(ps);
            var r = await InsertOneAsync(gkey, jToken);
            return r;
        }

        /// <summary>
        /// 根据条件更新数据库,主要用于非API的服务端内部调用
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public async Task<int> UpdateAsync(string key, object ps)
        {
            var gkey = GetDataGate(key);
            var jToken = JObject.FromObject(ps);
            var r = await UpdateAsync(gkey, jToken);
            return r;
        }

        //在v.0.2.0以后，有可能更新多条，可以带Filter条件更新多条
        private async Task<int> UpdateAsync(DataGateKey gkey, JToken jToken)
        {
            var tableMeta = gkey.MainTable;
            IDictionary<string, object> psin = jToken.ToDictionary();
            gkey.DataGate.OnChange(gkey, psin);
            List<string> fields = tableMeta.Fields.Where(f => !f.IsArray && f.ForeignField.IsEmpty())
                .Select(f => f.Name).Intersect(psin.Select(kv => kv.Key),
                 StringComparer.OrdinalIgnoreCase).ToList();

            var ps = fields.Select(f =>
            {
                var ff = tableMeta.GetField(f);
                var psKey = psin.Keys.First(key => key.Equals(f, StringComparison.OrdinalIgnoreCase));
                return DB.CreateParameter(ff.DbName, psin[psKey]);
            }).ToArray();

            string strFields = String.Join(",", fields.Select(f =>
            {
                f = f.Trim();
                //主键或集合字段不进入更新语句
                var ff = tableMeta.GetField(f);
                if (ff != null && (ff.IsArray || ff.PrimaryKey)) return null;
                //外来表字段pass掉
                if (ff != null && !ff.ForeignField.IsEmpty()) return null;
                var p = ps.First(p1 => p1.ParameterName.Equals(ff.DbName, StringComparison.OrdinalIgnoreCase));
                return ff.FixDbName + "=@" + p;
            }).Where(f => f != null));

            //根据是否带筛选条件决定是根据主键更新单条还是根据条件有可能更新多条
            string filter = FormatFilter(gkey.Filter, tableMeta) ?? CreateKeyFilter(tableMeta);

            string sql = $"update {tableMeta.FixDbName} set {strFields} where {filter}";
            int r = await DB.ExecNonQueryAsync(sql, ps);
            if (r > 1)
            {
                DB.RollbackTrans();
                throw new InvalidOperationException("错误操作，根据主键更新的记录数过多");
            }
            gkey.DataGate.OnChanged(gkey, psin);
            return r;
        }

        //更新单条记录的条件固定为主键值相等
        private string CreateKeyFilter(TableMeta tableMeta)
        {
            var r = String.Join(" AND ", tableMeta.Fields.Where(t => t.PrimaryKey)
                .Select(t => $"{t.FixDbName}=@{t.DbName}"));
            if (r.IsEmpty())
            {
                throw new Exception($"PrimaryKey(s) not defined in {tableMeta.Name}");
            }
            return r;
        }

        async Task<IDictionary<string, object>> GetObjectAsync(DataGateKey gkey, IDictionary<string, object> parameters)
        {
            var result = await GetArrayAsync(gkey, parameters);
            if (result is DataTable)
            {
                DataTable dt = result as DataTable;
                if (dt.Rows.Count == 0) return null;
                var dr = dt.Rows[0];
                Dictionary<string, object> dict = new Dictionary<string, object>();
                foreach (DataColumn dc in dt.Columns)
                {
                    dict.Add(dc.ColumnName, dr[dc]);
                }
                return dict;
            }
            else
            {
                JArray jarr = result as JArray;
                if (jarr.Count == 0) return null;
                return jarr[0].ToDictionary();
            }
        }

        //将DataTable结果表中来自数据库的名称转成对象属性名称
        private void ReNameColumns(TableMeta tableMeta, DataTable dt)
        {
            //tableMeta==null表示是直接执行的sql，Model为空
            if (tableMeta == null)
            {
                foreach (DataColumn dc in dt.Columns)
                {
                    dc.ColumnName = DB.DbNameConverter.ToPropName(dc.ColumnName);
                }
            }
            else
            {
                var fieldsDict = tableMeta.Fields
                    .ToDictionary(f => f.DbName, f => f.Name);

                foreach (DataColumn dc in dt.Columns)
                {
                    if (fieldsDict.ContainsKey(dc.ColumnName))
                        dc.ColumnName = fieldsDict[dc.ColumnName];
                }
            }
        }

        //从参数表中获取值并移除参数中中该项，通常用于分页查询中获取分页参数
        string GetValueRemoveKey(IDictionary<string, object> parameters, string key)
        {
            string dictKey = parameters.Keys.FirstOrDefault(k => k.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (dictKey != null)
            {
                var value = CommOp.ToStr(parameters[dictKey]);
                parameters.Remove(dictKey);
                return value;
            }
            return null;
        }

        /// <summary>
        /// 获取分页的数据
        /// </summary>
        /// <param name="gkey">查询Key名称</param>
        /// <param name="parameters">查询参数</param>
        /// <returns>{ total: 总数, data:DataTable}</returns>
        private async Task<PageResult> GetPageAsync(DataGateKey gkey, object obj)
        {
            if (!(obj is IDictionary<string, object> parameters))
            {
                parameters = CommOp.ToStrObjDict(obj);
            }
            if (gkey.TableJoins.Count > 1)
            {
                return await GetMasterDetailPageAsync(gkey, parameters);
            }
            var tableMeta = gkey.MainTable;
            CreateFilterStr(gkey, tableMeta, parameters);
            CreateOrderStr(gkey, tableMeta, parameters);
            var ps = DB.GetParameter(parameters);
            IPager pager = BuildPager(gkey, parameters);

            using (var dr = await DB.DBComm.ExecPageReaderAsync(pager, ps.ToArray()))
            {
                DataTable dt = new DataTable();
                dt.Load(dr);
                ReNameColumns(tableMeta, dt);
                return new PageResult
                {
                    Total = pager.RecordCount,
                    Data = dt
                };
            }
        }

        /// <summary>
        /// 连接查询获取主从表数据集
        /// </summary>
        /// <param name="gkey"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private async Task<PageResult> GetMasterDetailPageAsync(DataGateKey gkey, IDictionary<string, object> parameters)
        {
            var tableMeta = gkey.TableJoins[0].Table;
            CreateFilterStr(gkey, tableMeta, parameters);
            CreateOrderStr(gkey, tableMeta, parameters);
            var ps = DB.GetParameter(parameters);
            IPager pager = BuildMasterDetailPager(gkey, parameters);

            using (var dr = await DB.DBComm.ExecPageReaderAsync(pager, ps.ToArray()))
            {
                DataTable dt = new DataTable();
                dt.Load(dr);
                var data = CreateMasterArray(tableMeta, dt);
                return new PageResult
                {
                    Total = pager.RecordCount,
                    Data = data
                };
            }
        }

        int _deep = 0;
        /// <summary>
        /// 在结果集中对主表数据去重，并获取子表明细数据放到主表指定字段值中
        /// </summary>
        /// <param name="tableMeta"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        private JArray CreateMasterArray(TableMeta tableMeta, DataTable dt)
        {
            var pkeys = tableMeta.PrimaryKeys;
            JArray newDt = new JArray();

            var getPKeyValues = new Func<DataRow, string>(dr =>
            {
                var keyValues = pkeys.Select(pkey =>
                {
                    return dr[pkey.DbName].ToString();
                });
                return String.Join("^", keyValues);
            });

            //查询主表中不重复的主键字段值
            var keyRowsGroup = dt.Rows.Cast<DataRow>().GroupBy(dr => getPKeyValues(dr)).ToList();
            //遍历DataTable结果集， 找到相同的主键DataRow，并组装子表所在子集合
            keyRowsGroup.ForEach(drs =>
            {
                var newRow = new JObject();

                //遍历主表的每个字段，找出各类字段定义
                tableMeta.Fields.Each(fm =>
                {
                    if (fm.IsArray)
                    {
                        _deep = 0;
                        newRow[fm.Name] = GetChildItems(fm, drs);
                        return;
                    }
                    else if (fm.UIType == Consts.OperatorUIType)
                    {
                        return;
                    }
                    if (fm.ForeignField.IsEmpty() && !dt.Columns.Contains(fm.DbName))
                    {
                        return;
                    }
                    if (!fm.ForeignField.IsEmpty() && !dt.Columns.Contains(fm.Name))
                    {
                        return;
                    }

                    var firstDr = drs.First();
                    //在定义了外表字段时，表查询中是用别名表示外表字段名，所以这里应该分开处理
                    if (!fm.ForeignField.IsEmpty())
                    {
                        if (dt.Columns.Contains(fm.Name))
                            newRow[fm.Name] = new JValue(firstDr[fm.Name]);
                    }
                    else
                    {
                        newRow[fm.Name] = new JValue(firstDr[fm.DbName]);
                    }
                });
                newDt.Add(newRow);
            });
            return newDt;
        }

        /// <summary>
        /// 获取表连接时子表的明细数据，递归执行
        /// </summary>
        /// <param name="col"></param>
        /// <param name="drList"></param>
        /// <returns></returns>
        private JArray GetChildItems(FieldMeta col, IEnumerable<DataRow> drList)
        {
            JArray childDt = new JArray();
            if (++_deep > 8) return childDt; //防止递归层次过多
            if (drList.IsEmpty()) return childDt;

            var dt = drList.First().Table;
            var childTable = _ms.GetTableMeta(col.ArrayItemType);
            var pkeys = childTable.PrimaryKeys;

            //找到ForeignKey定义中的别名
            string alias = null;
            var aliases = col.ForeignKey.Split('.');
            if (aliases.Length > 1) alias = aliases[0];

            //当从表的主键字段在数据集中找不到时，说明没有联接此从表
            if (pkeys.Any(pkey =>
                {
                    var pkeyAlias = $"{alias ?? childTable.Name}_{pkey.Name}";
                    return !dt.Columns.Contains(pkeyAlias);
                }))
            {
                return childDt;
            }

            var getPKeyValues = new Func<DataRow, string>(dr =>
            {
                var keyValues = pkeys.Select(pkey =>
                {
                    var pkeyAlias = $"{alias ?? childTable.Name}_{pkey.Name}";
                    return dr[pkeyAlias].ToString();
                });
                return String.Join("^", keyValues);
            });

            //查询主子表中不重复的主键字段值
            var keyRowRroups = drList
                   .GroupBy(dr => getPKeyValues(dr))
             .Where(kv => kv.Key.IsNotEmpty()).ToList();

            keyRowRroups.Each(drs =>
            {
                var newRow = new JObject();

                //遍历主表的每个字段，找出各类字段定义
                childTable.Fields.Each(fm =>
                {
                    var bm = $"{alias ?? childTable.Name}_{fm.Name}";

                    if (fm.IsArray)
                    {
                        newRow[fm.Name] = GetChildItems(fm, drs);
                        return;
                    }
                    else if (fm.UIType == Consts.OperatorUIType)
                    {
                        return;
                    }
                    if (fm.ForeignField.IsEmpty() && !dt.Columns.Contains(bm))
                    {
                        return;
                    }

                    var firstDr = drs.First();
                    newRow[fm.Name] = new JValue(firstDr[bm]);
                });
                childDt.Add(newRow);
            });
            _deep--;
            return childDt;
        }

        /// <summary>
        /// 根据传过来的地址栏filter参数来获取查询条件
        /// 与服务端配置的filter合并
        /// </summary>
        /// <param name="tableMeta"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        private void CreateFilterStr(DataGateKey gkey, TableMeta tableMeta, IDictionary<string, object> ps)
        {
            if (!ps.ContainsKey(Consts.FilterKey)) return;
            var filterStr = CommOp.ToStr(ps[Consts.FilterKey]);
            var requests = JsonConvert.DeserializeObject<FilterRequest[]>(filterStr);
            ps.Remove(Consts.FilterKey);

            filterStr = string.Join(" and ", requests.Select(r =>
            {
                var field = tableMeta.Fields.FirstOrDefault(f => f.Name.Equals(r.Name, StringComparison.OrdinalIgnoreCase));
                if (field == null) return null;
                string left = field.ForeignField.IsEmpty() ? (gkey.TableJoins[0].Alias ?? tableMeta.Name) + "." + field.Name : field.ForeignField;

                //当有sql语句并且有模型定义时
                if (!gkey.Sql.IsEmpty() && gkey.TableJoins.Count > 0)
                {
                    left = field.Name;
                }

                string pName = r.Name + "_f"; //加后缀以免和未知的key冲突
                ps[pName] = r.Value;
                switch (r.Operator)
                {
                    case "e":
                        return $"{left}=@{pName}";
                    //判断日期相等，日期相等比较特殊，很难精确相等，
                    //因此转成只判断是否在当天
                    case "de":
                        var date = CommOp.ToDateTime(r.Value).Date;
                        var date1 = date.AddDays(1).AddTicks(-1);
                        ps[pName] = date;
                        ps[pName + 1] = date1;
                        return $"{left} between @{pName} and @{pName}1";
                    case "ne":
                        return $"{left}!=@{pName}";
                    case "in":
                        return $"{left} in @({pName})";
                    case "nin":
                        return $"{left} not in @({pName})";
                    case "i":
                        ps[pName] = "%" + r.Value + '%';
                        return $"{left} like @{pName}";
                    case "ni":
                        ps[pName] = "%" + r.Value + '%';
                        return $"{left} not like @{pName}";
                    case "lte":
                        return $"{left} <= @{pName}";
                    case "gte":
                        return $"{left} >= @{pName}";
                    case "bt":
                        if (r.Value1.IsDefault())
                        {
                            return $"{left} >= @{pName}";
                        }
                        if (field.DataType == "Date" || field.DataType == "DateTime")
                        {
                            r.Value1 = CommOp.ToDateTime(r.Value1).Date.AddDays(1).AddTicks(-1);
                        }
                        ps[pName + 1] = r.Value1;
                        return $"{left} between @{pName} and @{pName}1";
                    case "n":
                        return $"{left} is null";
                    case "nn":
                        return $"{left} is not null";
                    default:
                        return null;
                        //throw new ArgumentException("非法的查询请求:" + r.Operator);

                }
            }).Where(r => r != null));

            //与原有的gkey.Filter合并得到一个and条件
            if (!filterStr.IsEmpty())
            {
                gkey.Filter = gkey.Filter.IsEmpty() ? filterStr : $"({gkey.Filter}) and {filterStr}";
            }

        }

        //生成排序子句,排序子句的传入规则是  field1 a/d field2 a/d ...
        private void CreateOrderStr(DataGateKey gkey, TableMeta tableMeta, IDictionary<string, object> ps)
        {
            if (!ps.ContainsKey(Consts.SortKey)) return;
            var sortStr = CommOp.ToStr(ps[Consts.SortKey]);
            ps.Remove(Consts.SortKey);
            var sortArr = sortStr.Split(' ');

            List<string> sorts = new List<string>();
            for (var i = 0; i < sortArr.Length - 1; i += 2)
            {
                string f = sortArr[i];
                string field = GetSortField(f, gkey);
                if (field.IsEmpty()) continue;
                string ad = sortArr[i + 1];
                if (ad.StartsWith("d"))
                {
                    sorts.Add(field + " desc");
                }
                else
                {
                    sorts.Add(field.ToStr());
                }
            }
            string orderby = String.Join(",", sorts);
            if (!orderby.IsEmpty())
            {
                gkey.OrderBy = orderby;
            }
        }

        /// <summary>
        /// 获取order by子句的字段序号
        /// </summary>
        /// <param name="f"></param>
        /// <param name="gkey"></param>
        /// <returns></returns>
        private string GetSortField(string f, DataGateKey gkey)
        {
            string[] qfs = gkey.QueryFieldsTerm.Split(',').Select(fs => fs.Trim()).ToArray();
            var ff = gkey.GetField(f)?.FixDbName;
            for (var i = 0; i < qfs.Length; i++)
            {
                if (qfs[i] == ff) return qfs[i];
                if (qfs[i].EndsWith("." + ff)) return qfs[i];
            }
            return GetForeignKeyField(f, gkey);
        }

        /// <summary>
        /// 在联表查询的对外表字段的排序中，查找title相同或order为相反数的字段的foreignkey作为排序字段
        /// </summary>
        /// <param name="f"></param>
        /// <param name="gkey"></param>
        /// <returns></returns>
        private string GetForeignKeyField(string f, DataGateKey gkey)
        {
            if (gkey.TableJoins.IsEmpty()) return null;
            var table = gkey.TableJoins[0].Table;
            var field = table.Fields.FirstOrDefault(f1 => f1.Name == f);
            if (field?.ForeignField != null)
            {
                field = table.Fields.FirstOrDefault(f1 => f1 != field && (f1.Title == field.Title
                || (Math.Abs(f1.Order ?? 0) == Math.Abs(field.Order ?? 0))));
                return field?.ForeignKey;
            }
            return null;
        }

        /// <summary>
        /// 获取不分页的数据数组
        /// </summary>
        /// <param name="gkey">查询Key名称</param>
        /// <param name="parameters">查询参数</param>
        /// <returns>DataTable</returns>
        private async Task<object> GetArrayAsync(DataGateKey gkey, IDictionary<string, object> parameters)
        {
            if (gkey.TableJoins?.Count > 1)
            {
                return await GetMasterDetailArrayAsync(gkey, parameters);
            }
            var tableMeta = gkey.MainTable;
            CreateFilterStr(gkey, tableMeta, parameters);
            CreateOrderStr(gkey, tableMeta, parameters);
            var ps = DB.GetParameter(parameters);
            string sql = BuildSql(gkey);
            var dt = await DB.ExecDataTableAsync(sql, ps.ToArray());
            ReNameColumns(tableMeta, dt);
            return dt;
        }

        private async Task<JArray> GetMasterDetailArrayAsync(DataGateKey gkey, IDictionary<string, object> parameters)
        {
            var tableMeta = gkey.TableJoins[0].Table;
            CreateFilterStr(gkey, tableMeta, parameters);
            CreateOrderStr(gkey, tableMeta, parameters);
            var ps = DB.GetParameter(parameters);
            string sql = BuildMasterDetailSql(gkey);

            DataTable dt = await DB.ExecDataTableAsync(sql, ps.ToArray());
            var data = CreateMasterArray(tableMeta, dt);
            return data;
        }

        //构造不分页的主从表查询语句
        private string BuildMasterDetailSql(DataGateKey gkey)
        {
            var tableMetas = gkey.TableJoins.Select(m => m.Table);
            string orderBy = FormatOrderBy(gkey);
            if (!orderBy.IsEmpty())
            {
                orderBy = " order by " + orderBy;
            }
            string filter = FormatFilter(gkey.Filter, tableMetas.ToArray());
            if (!filter.IsEmpty()) filter = " where " + filter;
            string sql = $"{filter}{orderBy}";
            if (gkey.Sql.IsEmpty())
            {
                return $"select {gkey.QueryFieldsTerm} from {gkey.JoinSubTerm}{sql}";
            }
            else if (sql.IsEmpty())
            {
                return gkey.Sql;
            }
            else
            {
                return $"select * from ({gkey.Sql})c {sql}";
            }
        }

        //单表的分页
        private IPager BuildPager(DataGateKey gkey, IDictionary<string, object> parameters)
        {
            var tableMeta = gkey.MainTable;
            string filter = FormatFilter(gkey.Filter, tableMeta);
            if (!filter.IsEmpty())
            {
                filter = " where " + filter;
            }

            string sql = gkey.Sql;
            if (sql.IsEmpty())
            {
                sql = $"select {gkey.QueryFieldsTerm} from {tableMeta.FixDbName}{filter}";
            }
            int pageSize = CommOp.ToInt(GetValueRemoveKey(parameters, "pageSize"));
            if (pageSize <= 0) pageSize = Consts.DefaultPageSize;
            DBPagerInfo pager = new DBPagerInfo
            {
                Query = sql,
                KeyId = $"{gkey.TableJoins[0].Alias ?? tableMeta.FixDbName}.{tableMeta.PrimaryKey.FixDbName}",
                PageIndex = Math.Max(1, CommOp.ToInt(GetValueRemoveKey(parameters, "pageIndex"))) - 1,
                PageSize = pageSize,
                OrderBy = gkey.OrderBy,
            };
            return pager;
        }

        //构造分页主从表查询分页对象
        private IPager BuildMasterDetailPager(DataGateKey gkey, IDictionary<string, object> parameters)
        {
            var mainModel = gkey.TableJoins[0].Table;

            var tableMetas = gkey.TableJoins.Select(m =>
            {
                return m.Table;
            });
            int pageSize = CommOp.ToInt(GetValueRemoveKey(parameters, "pageSize"));
            if (pageSize <= 0) pageSize = Consts.DefaultPageSize;
            return new MasterDetailPagerInfo
            {
                TablesAndJoins = gkey.JoinSubTerm,
                Fields = gkey.QueryFieldsTerm,
                OrderBy = gkey.OrderBy,
                Filter = FormatFilter(gkey.Filter, tableMetas.ToArray()),
                KeyId = $"{gkey.TableJoins[0].Alias ?? mainModel.FixDbName}.{mainModel.PrimaryKey.FixDbName}",
                PageIndex = Math.Max(1, CommOp.ToInt(GetValueRemoveKey(parameters, "pageIndex"))) - 1,
                PageSize = pageSize,
            };
        }

        /// <summary>
        /// 判断是否是虚表（name和DbName不对应）
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        private bool IsVirtualTable(TableMeta meta)
        {
            return DB.GetDbObjName(meta.Name) != meta.DbName; 
        }

        //单表不分页sql
        private string BuildSql(DataGateKey gkey)
        {
            var tableMeta = gkey.MainTable;
            
            if (tableMeta == null)
            {
                return gkey.Sql;
            }
            string filter = FormatFilter(gkey.Filter, tableMeta);
            if (!filter.IsEmpty())
            {
                filter = " where " + filter;
            }
            string orderby = FormatOrderBy(gkey);
            if (!orderby.IsEmpty())
            {
                orderby = " order by " + orderby;
            }
            string sql = $"{filter }{ orderby}";
            if (gkey.Sql.IsEmpty())
            {
                return $"select {gkey.QueryFieldsTerm} from {tableMeta.FixDbName}{sql}";
            }
            else if (sql.IsEmpty())
            {
                return gkey.Sql;
            }
            return $"SELECT * FROM ({gkey.Sql}) {tableMeta.DbName} {sql}";
        }

        //单表不分页查询，将orderby子句中的属性名替换成数据库字段限定名
        private string FormatOrderBy(DataGateKey gkey)
        {
            var orderBy = gkey.OrderBy;
            if (orderBy.IsEmpty())
            {
                return null;
            }
            orderBy = Regex.Replace(orderBy, "\\s+", " ");
            var orders = orderBy.Split(',').Select(order => order.Trim());
            var fieldArr = orders.Select(o =>
            {
                string prop = o.Split(' ')[0].Trim();
                string field = String.Join(".", prop.Split('.').Select(a => DB.AddFix(a)));
                return new { prop, field };
            });
            fieldArr.OrderBy(pf => -pf.field.Length)
                .Each(pf => orderBy = orderBy.Replace(pf.prop, pf.field));
            return orderBy;
        }

        public void Dispose()
        {
            if (DB != null)
            {
                DB.Dispose();
                DB = null;
            }
        }
    }
}
//命名规则：
//GetXXX() --数据本来就有，只是简单的提取
// CreateXXX() -- 需要有创建的过程
// BuildXXX() --数据需要几个子项分别组成