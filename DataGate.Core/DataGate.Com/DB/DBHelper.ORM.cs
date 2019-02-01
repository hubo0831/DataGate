using DataGate.Com;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataGate.Com.DB
{
    /// <summary>
    /// 此处主要放置ORM相关的逻辑
    /// </summary>
    partial class DBHelper
    {
        #region  T

        /// <summary>
        /// 通过指定条件返回一个T的列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strAfterWhere"></param>
        /// <param name="sp"></param>
        /// <returns></returns>
        public virtual List<T> GetList<T>(string strAfterWhere = null, params IDataParameter[] sp)
            where T : new()
        {
            if (strAfterWhere != null)
            {
                strAfterWhere = "where " + strAfterWhere;
            }
            string sql = $"select * from {GetDbObjName(typeof(T).Name)} {strAfterWhere}";
            return GetSqlList<T>(sql, sp);
        }

        /// <summary>
        /// 通过执行sql语句返回一个泛型T的列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="sql">查询语句</param>
        /// <param name="sp">参数列表</param>
        /// <returns>T的泛型列表</returns>
        public virtual List<T> GetSqlList<T>(string sql, params IDataParameter[] sp)
            where T : new()
        {
            using (IDataReader reader = ExecReader(sql, sp))
            {
                DataTable schemaTable = reader.GetSchemaTable();
                PropertyInfo[] infos = typeof(T).GetProperties();
                var readerCols = schemaTable.Rows.Cast<DataRow>().Select(dr => dr["ColumnName"].ToString().ToLower());
                List<T> listT = new List<T>();
                while (reader.Read())
                {
                    T t = new T();
                    foreach (PropertyInfo info in infos)
                    {
                        if (readerCols.Contains(info.Name.ToLower()))
                        {
                            SetValue(t, info, reader[info.Name]);
                        }
                    }
                    listT.Add(t);
                }

                return listT;
            }
        }

        /// <summary>
        /// 根据唯一ID获取对象,返回实体，实体为数据表
        /// </summary>
        /// <param name="id">ID值</param>
        /// <returns>返回实体类</returns>
        public T GetModelById<T>(string id) where T : IId<string>, new()
        {
            if (string.IsNullOrEmpty(id))
            {
                return default(T);
            }

            T model = new T();
            Type type = model.GetType();
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT * FROM ").Append(AddFix(type.Name)).Append(" WHERE ID=@ID");
            List<IDataParameter> list = new List<IDataParameter>();
            DataTable dt = this.ExecDataTable(sb.ToString(), CreateParameter("ID", id));
            if (dt.Rows.Count > 0)
            {
                return RowToModel<T>(dt.Rows[0]);
            }
            return model;
        }

        /// <summary>
        /// 根据查询条件获取对象,返回实体，实体为数据表
        /// </summary>
        /// <param name="where">条件</param>
        /// <param name="param">参数化</param>
        /// <returns>返回实体类</returns>
        public T GetModelByWhere<T>(string where, params IDataParameter[] param) where T : new()
        {
            Type type = typeof(T);
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT * FROM " + AddFix(type.Name) + " WHERE 1=1");
            strSql.Append(where);
            DataTable dt = this.ExecDataTable(strSql.ToString(), param);
            if (dt.Rows.Count > 0)
            {
                return RowToModel<T>(dt.Rows[0]);
            }
            return default(T);
        }

        /// <summary>
        /// 根据查询条件获取对象,返回实体，实体可为业务Model
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="sql">查询语句</param>
        /// <param name="param">查询参数</param>
        /// <returns>对象</returns>
        public T GetModel<T>(string sql, params IDataParameter[] param) where T : new()
        {
            Type type = typeof(T);
            DataTable dt = this.ExecDataTable(sql, param);
            if (dt.Rows.Count > 0)
            {
                return RowToModel<T>(dt.Rows[0]);
            }
            return default(T);
        }

        /// <summary>
        /// 插入新对象到表
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="t">对象</param>
        /// <returns>插入的条数</returns>
        public int InsertModel<T>(T t)
        {
            string sql = PrepareInsertSqlString(t);
            var sp = GetParameter(t);
            return ExecNonQuery(sql, sp.ToArray());
        }

        /// <summary>
        /// 更新字符串ID的对象
        /// </summary>
        /// <typeparam name="T">要更新的对象类型</typeparam>
        /// <param name="t">对象</param>
        /// <returns>更新条数</returns>
        public int UpdateModel<T>(T t) where T : IId<string>
        {
            return UpdateModel<T, string>(t);
        }

        /// <summary>
        /// 更新对象
        /// </summary>
        /// <typeparam name="T">要更新的对象类型</typeparam>
        /// <param name="t">对象</param>
        /// <returns>更新条数</returns>
        public int UpdateModel<T, TId>(T t) where T : IId<TId>
        {
            string sql = PrepareUpdateSqlString<T, TId>(t);
            var sp = GetParameter(t);
            return ExecNonQuery(sql, sp.ToArray());
        }

        /// <summary>
        /// 根据ID删除指定对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public int DeleteModel<T>(string id) where T : IId<string>
        {
            string sql = PrepareDeleteSqlString<T>(id);
            var sp = CreateParameter("@ID", id);
            return ExecNonQuery(sql, sp);
        }

        #endregion

        #region 对象参数转换SqlParam
        /// <summary>
        /// 字典对象参数转换
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="suffix">是否要在参数名后加后缀</param>
        /// <returns></returns>
        public IEnumerable<IDataParameter> GetParameter(IDictionary<string, object> dict, string suffix = null)
        {
            if (dict == null)
            {
                yield break;
            }
            foreach (string key in dict.Keys)
            {
                yield return CreateParameter(key + suffix, dict[key]);
            }
        }

        /// <summary>
        /// 实体类对象参数转换
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="suffix">为和查询参数相区别对应的后缀</param>
        /// <returns></returns>
        IEnumerable<IDataParameter> GetParameter(object entity, string suffix = null)
        {
            if (entity is IDictionary<string, object> dict)
            {
                foreach (var a in GetParameter(dict, suffix))
                {
                    yield return a;
                }
                yield break;
            }
            if (entity == null) yield break;
            Type type = entity.GetType();
            PropertyInfo[] props = type.GetProperties();
            foreach (PropertyInfo prop in props)
            {
                yield return CreateParameter(prop.Name + suffix, prop.GetValue(entity, null));
            }
        }

        #endregion

        #region 拼接 查询
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        string PrepareQuerySqlString<T>(T entity)
        {
            Type type = entity.GetType();
            PropertyInfo[] props = type.GetProperties();
            StringBuilder sb = new StringBuilder();
            sb.Append(" select * from  ");
            sb.Append(type.Name);
            sb.Append("where ");
            StringBuilder sp = new StringBuilder();

            foreach (PropertyInfo prop in props)
            {
                if (prop.GetValue(entity, null) != null)
                {
                    sp.Append("," + prop.Name + $"={DBComm.ParamPrefix}{prop.Name}");
                }
            }

            sb.Append(sp.ToString().Substring(1, sp.ToString().Length - 1) + ")");
            return sb.ToString();
        }

        #endregion
        #region 拼接 新增 SQL语句
        /// <summary>
        /// 泛型方法，反射生成InsertSql语句
        /// </summary>
        /// <param name="entity">实体类</param>
        /// <returns>int</returns>
        string PrepareInsertSqlString<T>(T entity)
        {
            Type type = typeof(T);
            PropertyInfo[] props = type.GetProperties();
            string fields = String.Join(",", props.Select(p => AddFix(p.Name)));
            string values = String.Join(",", props.Select(p => "@" + p.Name));
            StringBuilder sb = new StringBuilder();
            return $"insert into {AddFix(type.Name)}\r\n" +
                $"({fields})\r\n" +
                $"values({values})";
        }

        #endregion

        #region 拼接 修改 SQL语句
        /// <summary>
        /// 哈希表生成UpdateSql语句
        /// </summary>
        /// <param name="strAfterWhere">查询条件</param>
        /// <param name="t">更新的对象</param>
        /// <returns></returns>
        string PrepareUpdateSqlString<T>(string strAfterWhere, T t)
            where T : IId<string>
        {
            return PrepareUpdateSqlString<T, string>(t);
        }

        /// <summary>
        /// 泛型方法，反射生成字符串ID的UpdateSql语句
        /// </summary>
        /// <param name="t">实体类</param>
        /// <returns>int</returns>
        string PrepareUpdateSqlString<T>(T t) where T : IId<string>
        {
            return PrepareUpdateSqlString<T, string>(t);
        }

        /// <summary>
        /// 泛型方法，反射生成UpdateSql语句
        /// </summary>
        /// <param name="t">实体类</param>
        /// <returns>int</returns>
        string PrepareUpdateSqlString<T, TId>(T t) where T : IId<TId>
        {
            Type type = typeof(T);
            PropertyInfo[] props = type.GetProperties();
            StringBuilder sb = new StringBuilder();
            sb.Append($"UPDATE {AddFix(type.Name)} SET ");
            List<string> subs = new List<string>();
            foreach (PropertyInfo prop in props)
            {
                string dbName = GetDbObjName(prop.Name);
                if (dbName == GetDbObjName("Id"))
                {
                    continue;
                }
                subs.Add($"{AddFix(prop.Name)}=@{prop.Name}");
            }
            sb.Append(String.Join(",", subs));
            sb.Append(" WHERE ID=@ID");
            return sb.ToString();
        }

        #endregion

        #region 拼接 删除 SQL语句
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        string PrepareDeleteSqlString<T>(string id)
        {
            return PrepareDeleteSqlString<T, string>(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        string PrepareDeleteSqlString<T, TId>(TId id)
        {
            String sql = $"delete from {AddFix(typeof(T).Name)} where ID=@ID";
            return sql;
        }

        /// <summary>
        /// 拼接删除SQL语句
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="pkName">字段主键</param>
        /// <returns></returns>
        string PrepareDeleteSqlString(string tableName, string pkName)
        {

            StringBuilder sb = new StringBuilder("Delete From " + AddFix(tableName) + " Where " + AddFix(pkName) + " = @" + pkName + "");
            return sb.ToString();
        }
        /// <summary>
        /// 拼接删除SQL语句
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="ht">多参数</param>
        /// <returns></returns>
        string PrepareDeleteSqlString(string tableName, Hashtable ht)
        {

            StringBuilder sb = new StringBuilder("Delete From " + tableName + " Where 1=1");
            foreach (string key in ht.Keys)
            {
                sb.Append(" AND " + AddFix(key) + " =@" + key + "");
            }
            return sb.ToString();
        }
        #endregion

        /// <summary>
        /// 将对象属性名转成字段名的转换器
        /// </summary>
        public INameConverter DbNameConverter { get; set; }

        Dictionary<string, string> _propFieldDict = new Dictionary<string, string>();

        /// <summary>
        /// 手动建立对象的属性名和数据库中名称之间的对应关系
        /// </summary>
        /// <param name="objName"></param>
        /// <param name="dbName"></param>
        public void SetDbObjName(string objName, string dbName)
        {
            _propFieldDict[objName] = dbName;
        }

        /// <summary>
        /// 根据DbNameConverter,将对象属性名获取数据库中的名称
        /// 如果没有定义根据DbNameConverter则原样返回
        /// </summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        public string GetDbObjName(string propName)
        {
            if (_propFieldDict.ContainsKey(propName))
            {
                return _propFieldDict[propName];
            }
            if (DbNameConverter != null)
            {
                _propFieldDict[propName] = DbNameConverter.ToDBName(propName);
                return _propFieldDict[propName];
            }
            return propName;
        }

        /// <summary>
        /// 将属性名转换，并形成带限定符的字段名
        /// </summary>
        /// <param name="propName">属性名</param>
        /// <returns></returns>
        public string AddFix(string propName)
        {
            if (DbNameConverter != null)
            {
                propName = GetDbObjName(propName);
            }
            if (!DBComm.FieldPrefix.IsEmpty() && propName.StartsWith(DBComm.FieldPrefix))
            {
                propName = propName.Substring(1, propName.Length - 2);
            }
            return DBComm.FieldPrefix + propName + DBComm.FieldSuffix;
        }

        /// <summary>
        /// 将DataRow转换为 实体类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <returns></returns>
        T RowToModel<T>(DataRow dr) where T : new()
        {
            T model = new T();
            foreach (PropertyInfo pi in model.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance))
            {
                string fieldName = GetDbObjName(pi.Name);
                if (dr.Table.Columns.Contains(fieldName))
                {
                    if (!CommOp.IsEmpty(dr[fieldName]))
                    {
                        pi.SetValue(model, CommOp.HackType(dr[fieldName], pi.PropertyType), null);
                    }
                }
            }
            return model;
        }

    }
}
