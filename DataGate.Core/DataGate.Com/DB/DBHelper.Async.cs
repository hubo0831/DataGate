using DataGate.Com;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGate.Com.DB
{
    partial class DBHelper
    {
        /// <summary>
        /// 用指定连接串执行非查询语句异步版本
        /// </summary>
        /// <param name="sql">非查询语句</param>
        /// <param name="sp">可选参数数组</param>
        /// <returns>影响的行数</returns>
        public virtual async Task<int> ExecNonQueryAsync(string sql, params IDataParameter[] sp)
        {
            var conn = StartConn();
            DbCommand sc = DBComm.CreateCommand(conn);
            sc.CommandText = sql;
            PrepareCommand(sc, sp);
            if (!sql.Contains(' ')) sc.CommandType = CommandType.StoredProcedure;
            int r = await sc.ExecuteNonQueryAsync();
            sc.Parameters.Clear();
            EndConn(conn);
            return r;
        }

        /// <summary>
        /// 根据指定Sql返回一个DataReader
        /// </summary>
        /// <param name="sql">要执行的sql语句</param>
        /// <param name="sp">参数数组</param>
        /// <returns>IDataReader</returns>
        public virtual async Task<IDataReader> ExecReaderAsync(string sql, params IDataParameter[] sp)
        {
            var conn = StartConn();
            DbCommand sc = DBComm.CreateCommand(conn);
            sc.CommandText = sql;

            // we use a try/catch here because if the method throws an exception we want to 
            // close the connection throw code, because no datareader will exist, hence the 
            // commandBehaviour.CloseConnection will not work
            try
            {
                PrepareCommand(sc, sp);
                IDataReader rdr = await sc.ExecuteReaderAsync(
                    InTrans ? CommandBehavior.Default :
                    CommandBehavior.CloseConnection);
                sc.Parameters.Clear();
                return rdr;
            }
            catch
            {
                EndConn(conn);
                throw;
            }
        }

        /// <summary>
        /// 得到单一对象
        /// </summary>
        /// <param name="sql">要执行的sql语句</param>
        /// <param name="sp">参数数组</param>
        /// <returns>返回的单个值</returns>
        public virtual async Task<object> ExecGetObjectAsync(String sql, params IDataParameter[] sp)
        {
            object o = 0;
            var conn = StartConn();
            DbCommand sc = DBComm.CreateCommand(conn);
            sc.CommandText = sql;
            PrepareCommand(sc, sp);
            o = await sc.ExecuteScalarAsync();
            sc.Parameters.Clear();
            EndConn(conn);
            return o;
        }

        /// <summary>
        /// 执行存储过程修改数据
        /// </summary>
        /// <param name="procname">存储过程名称</param>
        /// <param name="sp">可选参数数组</param>
        /// <returns>影响的行数</returns>
        public virtual async Task<int> RunProcedureAsync(string procname, params IDataParameter[] sp)
        {
            var conn = StartConn();
            DbCommand sc = DBComm.CreateCommand(conn);
            sc.CommandText = procname;
            sc.CommandType = CommandType.StoredProcedure;
            PrepareCommand(sc, sp);
            int r = await sc.ExecuteNonQueryAsync();
            sc.Parameters.Clear();
            EndConn(conn);
            return r;
        }

        /// <summary>
        /// 执行SQL查询语句，返回DataTable
        /// </summary>
        /// <param name="sql">要执行的Sql语句</param>
        /// <param name="sp">参数数组</param>
        /// <returns>DataTable</returns>
        public virtual async Task<DataTable> ExecDataTableAsync(string sql, params IDataParameter[] sp)
        {
            using (var dataReader = await ExecReaderAsync(sql, sp))
            {
                DataTable dt = new DataTable();
                dt.Load(dataReader);
                return dt;
            }
        }
    }
}
