using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataGate.Com;
using System.Data;

namespace DataGate.Com.DB
{
    /// <summary>
    /// 封装对象的增删改查操作
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <typeparam name="TId">主键类型</typeparam>
    public class DBCrud<T, TId> where T : IId<TId>, new()
    {
        protected DBHelper Helper { get; }

        public DBCrud(DBHelper helper)
        {
            Helper = helper;
        }

        public async Task<IList<T>> GetListAsync(string strAfterWhere = null, IDictionary<string, object> param = null)
        {
            return await Helper.GetListAsync<T>(strAfterWhere, param);
        }

        public async Task<T> GetModelByWhereAsync(string where, params IDataParameter[] ps)
        {
            return await Helper.GetModelByWhereAsync<T>(where, ps);
        }

        public async Task<T> GetModelByIdAsync(TId id)
        {
            return await Helper.GetModelByIdAsync<T, TId>(id);
        }

        /// <summary>
        /// 插入新记录
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public async Task<int> InsertAsync(T t)
        {
            return await Helper.InsertModelAsync(t);
        }

        public async Task<int> DeleteAsync(T t)
        {
            return await Helper.DeleteModelAsync<T, TId>(t.Id);
        }

        public async Task<int> DeleteManyAsync(string where, IDictionary<string, object> t)
        {
            return await Helper.DeleteModelAsync<T>(where, t);
        }

        public async Task<int> UpdateAsync(T t)
        {
            return await Helper.UpdateModelAsync<T, TId>(t);
        }

        public async Task<int> UpdateManyAsync(string where, object update, object p)
        {
            return await Helper.UpdateManyAsync<T>(where, update, p);
        }
    }

    /// <summary>
    /// 专对字符串主键的对象的增删改查操作
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DBCrud<T> : DBCrud<T, string> where T : IId<string>, new()
    {
        public DBCrud(DBHelper helper) : base(helper) { }
    }

}
