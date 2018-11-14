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
    public class DBCrud<T> where T : IId<string>, new()
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

        public async Task<T> GetModelByIdAsync(string id)
        {
            return await Helper.GetModelByIdAsync<T>(id);
        }

        /// <summary>
        /// 插入新记录
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public async Task<int> InsertAsync(IDictionary<string, object> t)
        {
            return await Helper.InsertModelAsync(t);
        }

        public async Task<int> DeleteAsync(T t)
        {
            return await Helper.DeleteModelAsync<T>(t.Id);
        }

        public async Task<int> DeleteManyAsync(string where, IDictionary<string, object> t)
        {
            return await Helper.DeleteModelAsync<T>(where, t);
        }

        public async Task<int> UpdateAsync(T t)
        {
            return await Helper.UpdateModelAsync(t);
        }

        public async Task<int> UpdateManyAsync(string where, T t, IDictionary<string, object> p)
        {
            return await Helper.UpdateModelAsync(where, t, p);
        }



    }
}
