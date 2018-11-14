using DataGate.Com;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataGate.Com.DB
{
    /// <summary>
    /// 此处主要放置ORM相关的逻辑
    /// </summary>
    partial class DBHelper
    {
        #region  T

        /// <summary>
        /// 根据查询参对象返回T的列表异步版本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strAfterWhere"></param>
        /// <param name="paramObj"></param>
        /// <returns></returns>
        public virtual async Task<List<T>> GetListAsync<T>(string strAfterWhere, IDictionary<string, object> paramObj)
            where T : new()
        {
            var ps = GetParameter(paramObj);
            return await GetListAsync<T>(strAfterWhere, ps.ToArray());
        }

        /// <summary>
        /// 通过指定条件返回一个T的列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strAfterWhere"></param>
        /// <param name="sp"></param>
        /// <returns></returns>
        public virtual async Task<List<T>> GetListAsync<T>(string strAfterWhere = null, params IDataParameter[] sp)
            where T : new()
        {
            string strFields = String.Join(",", typeof(T).GetProperties()
                .Select(p => AddFix(p.Name)));
            if (strAfterWhere != null)
            {
                strAfterWhere = "where " + strAfterWhere;
            }
            string sql = $"select {strFields} from {GetDbObjName(typeof(T).Name)} {strAfterWhere}";
            return await GetSqlListAsync<T>(sql, sp);
        }

        /// <summary>
        /// 通过执行sql语句返回一个泛型T的列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="sql">查询语句</param>
        /// <param name="sp">参数列表</param>
        /// <returns>T的泛型列表</returns>
        public virtual async Task<List<T>> GetSqlListAsync<T>(string sql, params IDataParameter[] sp)
            where T : new()
        {
            using (IDataReader reader = await ExecReaderAsync(sql, sp))
            {
                DataTable schemaTable = reader.GetSchemaTable();
                PropertyInfo[] infos = typeof(T).GetProperties();
                List<T> listT = new List<T>();
                var readerCols = schemaTable.Rows.Cast<DataRow>().Select(dr => dr["ColumnName"].ToString());
                while (reader.Read())
                {
                    T t = new T();
                    foreach (PropertyInfo info in infos)
                    {
                        var fieldName = GetDbObjName(info.Name);
                        if (readerCols.Contains(fieldName))
                        {
                            SetValue(t, info, reader[fieldName]);
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
        /// <param name="id">ID</param>
        /// <returns>返回实体类</returns>
        public async Task<T> GetModelByIdAsync<T>(string id) where T : IId<string>, new()
        {
            if (string.IsNullOrEmpty(id))
            {
                return default(T);
            }

            T model = new T();
            Type type = model.GetType();
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT * FROM ").Append(AddFix(type.Name)).Append(" Where ID=@ID");
            List<IDataParameter> list = new List<IDataParameter>();
            DataTable dt = await this.ExecDataTableAsync(sb.ToString(), CreateParameter("ID", id));
            if (dt.Rows.Count > 0)
            {
                return ReaderToModel<T>(dt.Rows[0]);
            }
            return model;
        }

        /// <summary>
        /// 根据查询条件获取对象,返回实体，实体为数据表
        /// </summary>
        /// <param name="where">条件</param>
        /// <param name="param">参数化</param>
        /// <returns>返回实体类</returns>
        public async Task<T> GetModelByWhereAsync<T>(string where, params IDataParameter[] param)
where T : new()
        {
            Type type = typeof(T);
            StringBuilder strSql = new StringBuilder();
            strSql.Append("SELECT * FROM " + AddFix(type.Name) + " WHERE ");
            strSql.Append(where);
            DataTable dt = await this.ExecDataTableAsync(strSql.ToString(), param);
            if (dt.Rows.Count > 0)
            {
                return ReaderToModel<T>(dt.Rows[0]);
            }
            return default(T);
        }

        /// <summary>
        /// 根据查询条件获取对象,返回实体，实体可为业务Model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<T> GetModelAsync<T>(string sql, params IDataParameter[] param)
  where T : new()
        {
            Type type = typeof(T);
            DataTable dt = await this.ExecDataTableAsync(sql, param);
            if (dt.Rows.Count > 0)
            {
                return ReaderToModel<T>(dt.Rows[0]);
            }
            return default(T);
        }

        /// <summary>
        /// 插入新对象到表异步版本
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="t">对象</param>
        /// <returns>插入的条数</returns>
        public async Task<int> InsertModelAsync<T>(T t)
        {
            string sql = PrepareInsertSqlString(t);
            var sp = GetParameter(t);
            return await ExecNonQueryAsync(sql, sp.ToArray());
        }

        /// <summary>
        /// 插入新对象到表异步版本
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="t">对象</param>
        /// <returns>插入的条数</returns>
        public async Task<int> InsertModelAsync<T>(IDictionary<string, object> t)
        {
            string sql = PrepareInsertSqlString(t);
            var sp = GetParameter(t);
            return await ExecNonQueryAsync(sql, sp.ToArray());
        }

        /// <summary>
        /// 更新对象异步版本
        /// </summary>
        /// <typeparam name="T">要更新的对象类型</typeparam>
        /// <param name="t">对象</param>
        /// <returns>更新条数</returns>
        public async Task<int> UpdateModelAsync<T>(T t) where T : IId<string>
        {
            string sql = PrepareUpdateSqlString(t);
            var sp = GetParameter(t);
            return await ExecNonQueryAsync(sql, sp.ToArray());
        }

        /// <summary>
        /// 根据ID删除指定对象异步版本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<int> DeleteModelAsync<T>(string id) where T : IId<string>
        {
            string sql = PrepareDeleteSqlString<T>(id);
            var sp = CreateParameter("@ID", id);
            return await ExecNonQueryAsync(sql, sp);
        }

        /// <summary>
        /// 根据条件批量删除指定对象的异步版本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public async Task<int> DeleteModelAsync<T>(string where, IDictionary<string, object> ps)
        {
            String sql = $"delete from {AddFix(typeof(T).Name)} where " + where;
            var sp = GetParameter(ps).ToArray();
            return await ExecNonQueryAsync(sql, sp);
        }


        /// <summary>
        /// 根据查询条件批量更新实体异步版本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strAfterWhere">查询条件</param>
        /// <param name="t">要更橷的对象</param>
        /// <param name="ht">查询参数</param>
        /// <returns></returns>
        public async Task<int> UpdateModelAsync<T>(string strAfterWhere, T t, IDictionary<string, object> ht) where T : IId<string>
        {
            string sql = PrepareUpdateSqlString<T>(strAfterWhere, t);
            var sp = GetParameter(t).Union(GetParameter(ht));
            return await ExecNonQueryAsync(sql, sp.ToArray());
        }

        #endregion

    }
}
