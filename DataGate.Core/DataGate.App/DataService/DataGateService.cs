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

namespace DataGate.App.DataService
{
    /// <summary>
    /// 数据集中服务
    /// 由于DBHelper中事务的存在，这个服务不能为单例
    /// </summary>
    public class DataGateService : IDisposable
    {
        DBHelper _db;
        IMetaService _ms;
        public DataGateService(DBHelper db, IMetaService ms)
        {
            _db = db;
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
            if (!gkey.ConnName.IsEmpty())
            {
                _db.ConnStr = Consts.Config.GetConnectionString(gkey.ConnName);
            }
            return gkey;
        }

        /// <summary>
        /// 获取指定表指定条件的数据列表
        /// </summary>
        /// <param name="key"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<object> QueryAsync(string key, Dictionary<string, object> param)
        {
            DataGateKey gkey = GetDataGate(key);
            object result = null;
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
            }
            return result;
        }

        /// <summary>
        /// 根据查询参数对象调用Query
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj">对象参数</param>
        /// <returns></returns>
        public async Task<object> QueryAsync(string key, object obj)
        {
            Dictionary<string, object> param = CommOp.ToDictionary(obj);
            DataGateKey gkey = GetDataGate(key);
            object result = null;
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
            }
            return result;
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
            IEnumerable<string> ids = new string[0];

            _db.BeginTrans();
            //先删子表
            foreach (var detail in request.Details)
            {
                var dkey = GetDataGate(detail.Key);
                if (!detail.Removed.IsEmpty())
                {
                    await DeleteManyAsync(dkey, detail.Removed);
                }
            }

            if (!request.Added.IsEmpty())
            {
                ids = await InsertManyAsync(gkey, request.Added);
            }
            if (!request.Changed.IsEmpty())
            {
                await UpdateManyAsync(gkey, request.Changed);
            }
            if (!request.Removed.IsEmpty())
            {
                await DeleteManyAsync(gkey, request.Removed);
            }

            //后插子表
            foreach (var detail in request.Details)
            {
                var dkey = GetDataGate(detail.Key);
                if (!detail.Added.IsEmpty())
                {
                    await InsertManyAsync(dkey, detail.Added);
                }
                if (!detail.Changed.IsEmpty())
                {
                    await UpdateManyAsync(dkey, detail.Changed);
                }
            }
            _db.EndTrans();
            return ids;
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
                await UpdateOneAsync(gkey, jToken);
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
        private static string FormatFilter(string filter, params TableMeta[] tableMetas)
        {
            if (filter.IsEmpty()) return null;
            foreach (var tableMeta in tableMetas)
            {
                Regex reg = new Regex($"([^\\w@]+|^){tableMeta.Name}(\\W+|$)", RegexOptions.IgnoreCase);

                if (reg.IsMatch(filter))
                {
                    filter = reg.Replace(filter, $"$1{tableMeta.FixDbName}$2");
                }

                foreach (var field in tableMeta.Fields)
                {
                    //@号表示排除参数，取查询子句后面的属性名称
                    reg = new Regex($"([^\\w@]+|^){field.Name}(\\W+|$)", RegexOptions.IgnoreCase);

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
            var tableMeta = GetMainTable(gkey);
            IDictionary<string, object> psin = jToken.ToDictionary();
            var fields = tableMeta.PrimaryKeys.Select(pk => pk.Name)
                .Intersect(psin.Select(kv => kv.Key),
                 StringComparer.OrdinalIgnoreCase).ToList();
            gkey.DataGate?.OnRemove(psin);
            var ps = fields.Select(f =>
             {
                 var psKey = psin.Keys.First(key => key.Equals(f, StringComparison.OrdinalIgnoreCase));
                 return _db.CreateParameter(psKey, psin[psKey]);
             }).ToList();

            string filter = CreateKeyFilter(tableMeta);
            string sql = $"delete from {tableMeta.FixDbName} where {filter}";

            int r = await _db.TransNonQueryAsync(sql, ps.ToArray());
            if (r > 1)
            {
                _db.RollbackTrans();
                throw new InvalidOperationException("错误操作，根据ID删除的记录数过多");
            }
            return r;
        }

        private TableMeta GetMainTable(DataGateKey gkey)
        {
            return gkey.TableJoins[0].Table;
        }


        private async Task<string> InsertOneAsync(DataGateKey gkey, JToken jToken)
        {
            var tableMeta = GetMainTable(gkey);
            IDictionary<string, object> psin = jToken.ToDictionary();
            List<string> fields = tableMeta.Fields.Where(f => !f.IsArray && f.ForeignField.IsEmpty())
                .Select(f => f.Name).Intersect(psin.Select(kv => kv.Key),
                 StringComparer.OrdinalIgnoreCase).ToList();

            gkey.DataGate?.OnAdd(fields, psin);

            string id = null;

            //假定ID字段是GUID
            if (fields.Contains(Consts.DefaultKeyName))
            {
                id = CommOp.ToStr(psin[Consts.DefaultKeyName]);
                if (psin.ContainsKey(Consts.DefaultKeyName) && id.IsEmpty())
                {
                    id = CommOp.NewId();
                    psin[Consts.DefaultKeyName] = id;
                }
            }

            var ps = fields.Select(f =>
            {
                //集合字段不进入Insert语句
                var ff = tableMeta.Fields.FirstOrDefault(fd => fd.Name.Equals(f, StringComparison.OrdinalIgnoreCase));
                if (ff != null && ff.IsArray) return null;
                //外键字段pass掉
                if (ff != null && !ff.ForeignField.IsEmpty()) return null;
                var psKey = psin.Keys.First(key => key.Equals(f, StringComparison.OrdinalIgnoreCase));
                return _db.CreateParameter(psKey, psin[psKey]);
            }).Where(p => p != null).ToArray();
            string strFields = String.Join(",", fields.Select(p => _db.AddFix(p.Trim())));
            string strValues = String.Join(",", ps.Select(p => '@' + p.ParameterName));
            string sql = $"insert into {tableMeta.FixDbName} fields({strFields}) values({strValues})";
            await _db.TransNonQueryAsync(sql, ps);
            return id;
        }

        /// <summary>
        /// 根据主键更新数据库
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public async Task<int> UpdateOneAsync(string key, object ps)
        {
            var jToken = JObject.FromObject(ps);
            _db.BeginTrans();
            var r = await UpdateOneAsync(_ms.GetDataKey(key), jToken);
            _db.EndTrans();
            return r;
        }

        private async Task<int> UpdateOneAsync(DataGateKey gkey, JToken jToken)
        {
            var tableMeta = GetMainTable(gkey);
            IDictionary<string, object> psin = jToken.ToDictionary();
            List<string> fields = tableMeta.Fields.Where(f => !f.IsArray && f.ForeignField.IsEmpty())
                .Select(f => f.Name).Intersect(psin.Select(kv => kv.Key),
                 StringComparer.OrdinalIgnoreCase).ToList();

            gkey.DataGate?.OnChange(fields, psin);
            var ps = fields.Select(f =>
            {
                var psKey = psin.Keys.First(key => key.Equals(f, StringComparison.OrdinalIgnoreCase));
                return _db.CreateParameter(psKey, psin[psKey]);
            }).ToArray();

            string strFields = String.Join(",", fields.Select(f =>
            {
                f = f.Trim();
                //主键或集合字段不进入更新语句
                var ff = tableMeta.Fields.FirstOrDefault(fd => fd.Name.Equals(f, StringComparison.OrdinalIgnoreCase));
                if (ff != null && (ff.IsArray || ff.PrimaryKey)) return null;
                //外来表字段pass掉
                if (ff != null && !ff.ForeignField.IsEmpty()) return null;
                var p = ps.First(p1 => p1.ParameterName.Equals(f, StringComparison.OrdinalIgnoreCase));
                return _db.AddFix(f) + "=@" + p;
            }).Where(f => f != null));

            string filter = CreateKeyFilter(tableMeta);

            string sql = $"update {tableMeta.FixDbName} set {strFields} where {filter}";
            int r = await _db.TransNonQueryAsync(sql, ps);
            if (r > 1)
            {
                _db.RollbackTrans();
                throw new InvalidOperationException("错误操作，根据ID更新的记录数过多");
            }
            return r;
        }

        //更新单条记录的条件固定为主键值相等
        private string CreateKeyFilter(TableMeta tableMeta)
        {
            var r = String.Join(" AND ", tableMeta.Fields.Where(t => t.PrimaryKey)
                .Select(t => $"{t.FixDbName}=@{t.Name}"));
            if (r.IsEmpty())
            {
                throw new Exception($"PrimaryKey(s) not defined in {tableMeta.Name}");
            }
            return r;
        }

        private async Task<object> GetObjectAsync(DataGateKey gkey, Dictionary<string, object> parameters)
        {
            var tableMeta = GetMainTable(gkey);
            var ps = _db.GetParameter(parameters);
            string sql = BuildSql(gkey);
            var dt = await _db.ExecDataTableAsync(sql, ps.ToArray());
            if (dt.Rows.Count == 0) return null;
            var dr = dt.Rows[0];
            Dictionary<string, object> dict = new Dictionary<string, object>();
            ReNameColumns(tableMeta, dt);
            foreach (DataColumn dc in dt.Columns)
            {
                dict.Add(dc.ColumnName, dr[dc]);
            }
            return dict;
        }

        //将DataTable结果表中来自数据库的名称转成对象属性名称
        private void ReNameColumns(TableMeta tableMeta, DataTable dt)
        {
            var fieldsDict = tableMeta.Fields
                .ToDictionary(f => f.DbName, f => f.Name);

            foreach (DataColumn dc in dt.Columns)
            {
                if (fieldsDict.ContainsKey(dc.ColumnName))
                    dc.ColumnName = fieldsDict[dc.ColumnName];
            }
        }

        //从参数表中获取值并移除参数中中该项，通常用于分页查询中获取分页参数
        string GetValueRemoveKey(Dictionary<string, object> parameters, string key)
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
        private async Task<PageResult> GetPageAsync(DataGateKey gkey, Dictionary<string, object> parameters)
        {
            if (gkey.TableJoins.Length > 1)
            {
                return await GetMasterDetailPageAsync(gkey, parameters);
            }
            var tableMeta = GetMainTable(gkey);
            CreateFilterStr(gkey, tableMeta, parameters);
            var ps = _db.GetParameter(parameters);
            IPager pager = BuildPager(gkey, parameters);

            using (var dr = await _db.DBComm.ExecPageReaderAsync(pager, ps.ToArray()))
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
        private async Task<PageResult> GetMasterDetailPageAsync(DataGateKey gkey, Dictionary<string, object> parameters)
        {
            var tableMeta = gkey.TableJoins[0].Table;
            CreateFilterStr(gkey, tableMeta, parameters);
            var ps = _db.GetParameter(parameters);
            IPager pager = BuildMasterDetailPager(gkey, parameters);

            using (var dr = await _db.DBComm.ExecPageReaderAsync(pager, ps.ToArray()))
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

        /// <summary>
        /// 在结果集中对主表数据去重，并获取子表明细数据放到主表指定字段值中
        /// </summary>
        /// <param name="tableMeta"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        private JArray CreateMasterArray(TableMeta tableMeta, DataTable dt)
        {
            var pkey = tableMeta.PrimaryKey;
            JArray newDt = new JArray();

            var ids = dt.Rows.Cast<DataRow>().Select(dr => dr[pkey.DbName]).Distinct();
            ids.Each(id =>
            {
                var drs = dt.Rows.Cast<DataRow>().Where(dr => dr[pkey.DbName].Equals(id));
                var firstDr = drs.First();
                var newRow = new JObject();
                tableMeta.Fields.Each(col =>
                {
                    if (col.IsArray)
                    {
                        newRow[col.Name] = GetChildItems(col, drs);
                        return;
                    }
                    else if (col.UIType == Consts.OperatorUIType)
                    {
                        return;
                    }
                    if (col.ForeignField.IsEmpty() && !dt.Columns.Contains(col.DbName))
                    {
                        return;
                    }
                    if (!col.ForeignField.IsEmpty() && !dt.Columns.Contains(col.Name))
                    {
                        return;
                    }
                    //在定义了外表字段时，表查询中是用别名表示外表字段名，所以这里应该分开处理
                    if (!col.ForeignField.IsEmpty())
                    {
                        if (dt.Columns.Contains(col.Name))
                            newRow[col.Name] = new JValue(firstDr[col.Name]);
                    }
                    else
                    {
                        newRow[col.Name] = new JValue(firstDr[col.DbName]);
                    }
                });
                newDt.Add(newRow);
            });
            return newDt;
        }

        /// <summary>
        /// 获取表连接时子表的明细数据
        /// </summary>
        /// <param name="col"></param>
        /// <param name="drs"></param>
        /// <returns></returns>
        private JArray GetChildItems(FieldMeta col, IEnumerable<DataRow> drs)
        {
            var childModel = _ms.GetTableMeta(col.ArrayItemType);
            JArray childDt = new JArray();
            foreach (var dr in drs)
            {
                var newDr = new JObject();
                bool hasValue = false;
                foreach (FieldMeta fm in childModel.Fields)
                {
                    if (dr[fm.DbName] != DBNull.Value)
                    {
                        hasValue = true;
                    }
                    newDr[fm.Name] = new JValue(dr[fm.DbName]);
                }
                //忽略全部是空的行，这在左连接时经常发生
                if (hasValue)
                {
                    childDt.Add(newDr);
                }
            }
            return childDt;
        }

        /// <summary>
        /// 根据传过来的地址栏filter参数来获取查询条件
        /// 与服务端配置的filter合并@$///\\\\
        /// </summary>
        /// <param name="tableMeta"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        private void CreateFilterStr(DataGateKey gkey, TableMeta tableMeta, Dictionary<string, object> ps)
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

            if (!filterStr.IsEmpty())
            {
                gkey.Filter = gkey.Filter.IsEmpty() ? filterStr : $"({gkey.Filter}) and {filterStr}";
            }
        }

        /// <summary>
        /// 获取不分页的数据数组
        /// </summary>
        /// <param name="gkey">查询Key名称</param>
        /// <param name="parameters">查询参数</param>
        /// <returns>DataTable</returns>
        private async Task<object> GetArrayAsync(DataGateKey gkey, Dictionary<string, object> parameters)
        {
            if (gkey.TableJoins.Length > 1)
            {
                return await GetMasterDetaiArrayAsync(gkey, parameters);
            }
            var tableMeta = GetMainTable(gkey);
            CreateFilterStr(gkey, tableMeta, parameters);
            var ps = _db.GetParameter(parameters);
            string sql = BuildSql(gkey);
            var dt = await _db.ExecDataTableAsync(sql, ps.ToArray());
            ReNameColumns(tableMeta, dt);
            return dt;
        }

        private async Task<JArray> GetMasterDetaiArrayAsync(DataGateKey gkey, Dictionary<string, object> parameters)
        {
            var tableMeta = gkey.TableJoins[0].Table;
            CreateFilterStr(gkey, tableMeta, parameters);
            var ps = _db.GetParameter(parameters);
            string sql = BuildMasterDetailSql(gkey, parameters);

            DataTable dt = await _db.ExecDataTableAsync(sql, ps.ToArray());
            var data = CreateMasterArray(tableMeta, dt);
            return data;
        }

        //构造不分页的主从表查询语句
        private string BuildMasterDetailSql(DataGateKey gkey, Dictionary<string, object> parameters)
        {
            var tableMetas = gkey.TableJoins.Select(m =>
            {
                return m.Table;
            });
            string orderBy = FormatOrderBy(gkey.OrderBy);
            if (!orderBy.IsEmpty())
            {
                orderBy = " order by " + orderBy;
            }
            string filter = FormatFilter(gkey.Filter, tableMetas.ToArray());
            if (!filter.IsEmpty()) filter = " where " + filter;
            return $"select {gkey.QueryFieldsTerm} from {gkey.JoinSubTerm}{filter}{orderBy}";
        }

        //单表的分页
        private IPager BuildPager(DataGateKey gkey, Dictionary<string, object> parameters)
        {
            var tableMeta = GetMainTable(gkey);
            string filter = FormatFilter(gkey.Filter, tableMeta);
            if (!filter.IsEmpty())
            {
                filter = " where " + filter;
            }
            string sql = $"select {gkey.QueryFieldsTerm} from {tableMeta.FixDbName}{filter}";

            int pageSize = CommOp.ToInt(GetValueRemoveKey(parameters, "pageSize"));
            if (pageSize <= 0) pageSize = Consts.DefaultPageSize;
            DBPagerInfo pager = new DBPagerInfo
            {
                Query = sql,
                PageIndex = Math.Max(1, CommOp.ToInt(GetValueRemoveKey(parameters, "pageIndex"))) - 1,
                PageSize = pageSize,
                OrderBy = gkey.OrderBy,
            };
            return pager;
        }

        //构造分页主从表查询分页对象
        private IPager BuildMasterDetailPager(DataGateKey gkey, Dictionary<string, object> parameters)
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

        //单表不分页sql
        private string BuildSql(DataGateKey gkey)
        {
            var tableMeta = GetMainTable(gkey);

            string filter = FormatFilter(gkey.Filter, tableMeta);
            if (!filter.IsEmpty())
            {
                filter = " where " + filter;
            }
            string orderby = FormatOrderBy(gkey.OrderBy);
            if (!orderby.IsEmpty())
            {
                orderby = " order by " + orderby;
            }
            string sql = $"select {gkey.QueryFieldsTerm} from {tableMeta.FixDbName}{filter}{orderby}";
            return sql;
        }

        //单表不分页查询，将orderby子句中的属性名替换成数据库字段限定名
        private string FormatOrderBy(string orderBy)
        {
            if (orderBy.IsEmpty())
            {
                return null;
            }
            orderBy = Regex.Replace(orderBy, "\\s+", " ");
            var orders = orderBy.Split(',').Select(order => order.Trim());
            var fieldArr = orders.Select(o =>
            {
                string prop = o.Split(' ')[0].Trim();
                string field = String.Join(".", prop.Split('.').Select(a => _db.AddFix(a)));
                return new { prop, field };
            });
            fieldArr.OrderBy(pf => -pf.field.Length)
                .Each(pf => orderBy = orderBy.Replace(pf.prop, pf.field));
            return orderBy;
        }

        public void Dispose()
        {
            if (_db != null)
            {
                _db.Dispose();
                _db = null;
            }
        }
    }
}
//命名规则：
//GetXXX() --数据本来就有，只是简单的提取
// CreateXXX() -- 需要有创建的过程
// BuildXXX() --数据需要几个子项分别组成