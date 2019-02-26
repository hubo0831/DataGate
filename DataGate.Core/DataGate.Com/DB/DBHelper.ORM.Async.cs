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
        /// 通过指定条件返回一个T的列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strAfterWhere"></param>
        /// <param name="sp"></param>
        /// <returns></returns>
        public virtual async Task<List<T>> GetListAsync<T>(string strAfterWhere = null, object param = null)
            where T : new()
        {
            string strFields = String.Join(",", typeof(T).GetProperties()
                .Select(p => AddFix(p.Name)));
            if (strAfterWhere != null)
            {
                strAfterWhere = "where " + strAfterWhere;
            }
            string sql = $"select {strFields} from {GetDbObjName(typeof(T).Name)} {strAfterWhere}";
            return await GetSqlListAsync<T>(sql, GetParameter(param).ToArray());
        }

        /// <summary>
        /// 通过执行sql语句返回一个泛型T的列表
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="sql">查询语句</param>
        /// <param name="sp">参数列表</param>
        /// <returns>T的泛型列表</returns>
        async Task<List<T>> GetSqlListAsync<T>(string sql, params IDataParameter[] sp)
            where T : new()
        {
            using (IDataReader reader = await ExecReaderAsync(sql, sp))
            {
                DataTable schemaTable = reader.GetSchemaTable();
                PropertyInfo[] infos = typeof(T).GetProperties();
                List<T> listT = new List<T>();
                var readerCols = schemaTable.Rows.Cast<DataRow>().Select(dr => dr["ColumnName"].ToString().ToLower());
                while (reader.Read())
                {
                    T t = new T();
                    foreach (PropertyInfo info in infos)
                    {
                        var fieldName = GetDbObjName(info.Name).ToLower();
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
        /// 根据唯一ID获取对象,返回实体，如果有多个则报错
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>返回实体类</returns>
        public async Task<T> GetModelByIdAsync<T, TId>(TId id) where T : IId<TId>, new()
        {
            if (CommOp.IsDefault(id))
            {
                return default(T);
            }
            var pid = CreateParameter("ID", id);
            Type type = typeof(T);
            string sql = $"SELECT COUNT(1) FROM {AddFix(type.Name)} where ID=@ID";
            int cnt = CommOp.ToInt(await this.ExecGetObjectAsync(sql, pid));
            if (cnt == 0) return default(T);
            if (cnt > 1) throw new Exception("根据唯一的ID查到不止一条记录");

            StringBuilder sb = new StringBuilder();
            sql = "SELECT * FROM {AddFix(type.Name)} where ID=@ID";
            List<IDataParameter> list = new List<IDataParameter>();
            DataTable dt = await this.ExecDataTableAsync(sb.ToString(), pid);
            if (dt.Rows.Count == 1)
            {
                return RowToModel<T>(dt.Rows[0]);
            }
            return default(T);
        }

        /// <summary>
        /// 根据唯一的字符串ID获取对象,返回实体，实体为数据表
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>返回实体类</returns>
        public async Task<T> GetModelByIdAsync<T>(string id) where T : IId<string>, new()
        {
            return await GetModelByIdAsync<T, string>(id);
        }

        /// <summary>
        /// 根据查询条件获取对象,返回实体，如果有多个实体，则返回空
        /// </summary>
        /// <param name="where">条件</param>
        /// <param name="param">参数化</param>
        /// <returns>返回实体类</returns>
        public async Task<T> GetModelByWhereAsync<T>(string where, object param = null)
where T : new()
        {
            var ps = GetParameter(param).ToArray();
            Type type = typeof(T);
            string sql = $"SELECT COUNT(1) FROM {AddFix(type.Name)} WHERE {where}";
            int cnt = CommOp.ToInt(await this.ExecGetObjectAsync(sql, ps));
            if (cnt != 1) return default(T);

            sql = $"SELECT * FROM {AddFix(type.Name)} WHERE {where}";
            DataTable dt = await this.ExecDataTableAsync(sql, ps);
            if (dt.Rows.Count == 1)
            {
                return RowToModel<T>(dt.Rows[0]);
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
        /// 更新字符串ID对象的异步版本
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
        /// 更新对象异步版本
        /// </summary>
        /// <typeparam name="T">要更新的对象类型</typeparam>
        /// <param name="t">对象</param>
        /// <returns>更新条数</returns>
        public async Task<int> UpdateModelAsync<T, TId>(T t) where T : IId<TId>
        {
            string sql = PrepareUpdateSqlString<T, TId>(t);
            var sp = GetParameter(t);
            return await ExecNonQueryAsync(sql, sp.ToArray());
        }

        /// <summary>
        /// 根据字符串类型ID删除指定对象异步版本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<int> DeleteModelAsync<T>(string id) where T : IId<string>
        {
            return await DeleteModelAsync<T, string>(id);
        }

        /// <summary>
        /// 根据ID删除指定对象异步版本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<int> DeleteModelAsync<T, TId>(TId id) where T : IId<TId>
        {
            string sql = PrepareDeleteSqlString<T, TId>(id);
            var sp = CreateParameter("@ID", id);
            return await ExecNonQueryAsync(sql, sp);
        }


        /// <summary>
        /// 根据条件批量删除指定对象的异步版本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strAfterWhere"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public async Task<int> DeleteManyAsync<T>(string strAfterWhere, object param = null)
        {
            String sql = $"delete from {AddFix(typeof(T).Name)} where {strAfterWhere}";
            var sp = GetParameter(param).ToArray();
            return await ExecNonQueryAsync(sql, sp);
        }

        /// <summary>
        /// 根据查询条件批量更新实体异步版本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strAfterWhere">查询条件</param>
        /// <param name="dataToUpdate">要更新的对象</param>
        /// <param name="parameters">查询对象参数</param>
        /// <returns></returns>
        public async Task<int> UpdateManyAsync<T>(string strAfterWhere, object dataToUpdate, object parameters)
        {
            string sql = PrepareUpdateManyString<T>(strAfterWhere, dataToUpdate);
            var sp = GetParameter(dataToUpdate, "_u").Union(GetParameter(parameters));
            return await ExecNonQueryAsync(sql, sp.ToArray());
        }

        /// <summary>
        /// 哈希表生成UpdateSql语句
        /// </summary>
        /// <param name="strAfterWhere">查询条件</param>
        /// <param name="dataToUpdate">更新的对象</param>
        /// <returns></returns>
        string PrepareUpdateManyString<T>(string strAfterWhere, object dataToUpdate)
        {
            if (dataToUpdate is IDictionary<string, object> dict)
            {
                return PrepareUpdateManyString<T>(strAfterWhere, dict);
            }
            List<string> sbs = new List<string>();
            string sets = String.Join(",", dataToUpdate.GetType().GetProperties()
                .Select(key => $"{AddFix(key.Name)}=@{key.Name}_u"));
            return $"update {GetDbObjName(typeof(T).Name)} set {sets} where {strAfterWhere}";
        }

        string PrepareUpdateManyString<T>(string strAfterWhere, IDictionary<string, object> ht)
        {
            List<string> sbs = new List<string>();
            string sets = String.Join(",", ht
                .Select(key => $"{AddFix(key.Key)}=@{key.Key}_u"));
            return $"update {GetDbObjName(typeof(T).Name)} set {sets} where {strAfterWhere}";
        }
        #endregion

    }
}
