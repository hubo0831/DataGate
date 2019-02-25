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

        /// <summary>
        /// 获取实体列表
        /// </summary>
        /// <param name="strAfterWhere">Where后的查询条件</param>
        /// <param name="param">查询参数，通常是匿名对象或字典对象</param>
        /// <returns></returns>
        public async Task<IList<T>> GetListAsync(string strAfterWhere = null, object param = null)
        {
            return await Helper.GetListAsync<T>(strAfterWhere, param);
        }

        /// <summary>
        /// 根据条件获取单个实体。如果查询出多个实体则返回空
        /// </summary>
        /// <param name="where"></param>
        /// <param name="param">查询参数，通常是匿名对象或字典对象</param>
        /// <returns>单个实体</returns>
        public async Task<T> GetModelByWhereAsync(string where, object param = null)
        {
            return await Helper.GetModelByWhereAsync<T>(where, param);
        }

        /// <summary>
        /// 根据ID获取单个对象，实体必须有Id属性
        /// </summary>
        /// <param name="id">ID值</param>
        /// <returns>单个实体</returns>
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

        /// <summary>
        /// 根据条件删除多个实体
        /// </summary>
        /// <param name="where"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public async Task<int> DeleteManyAsync(string where, object t)
        {
            return await Helper.DeleteManyAsync<T>(where, t);
        }

        /// <summary>
        /// 替换单个实体
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public async Task<int> UpdateAsync(T t)
        {
            return await Helper.UpdateModelAsync<T, TId>(t);
        }

        /// <summary>
        /// 同时更新多个实体
        /// </summary>
        /// <param name="where"></param>
        /// <param name="update"></param>
        /// <param name="p"></param>
        /// <returns></returns>
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
