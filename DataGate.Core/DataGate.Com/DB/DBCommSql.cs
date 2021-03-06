using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.Data.Common;
using System.Configuration;
using DataGate.Com;
using System.Threading.Tasks;
using System.Linq;

namespace DataGate.Com.DB
{
    /// <summary>
    /// SqlServer数据库的访问接口实现
    /// </summary>
    public class DBCommSql : IDBComm, ISupportBuckCopy
    {
        /// <summary>
        /// 通用数据访问类
        /// </summary>
        public DBHelper Helper { get; set; }
        /// <summary>
        /// 生成SqlConnection
        /// </summary>
        /// <returns></returns>
        public DbConnection CreateConnection()
        {
            return new SqlConnection(Helper.ConnStr);
        }

        /// <summary>
        /// 生成SqlDataAdapter
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public DbDataAdapter CreateDataAdapter(DbCommand command)
        {
            return new SqlDataAdapter((SqlCommand)command);
        }

        /// <summary>
        /// 生成SqlParameter
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IDataParameter CreateParameter(string parameterName, object value)
        {
            if (value is byte[])
            {
                return CreateImageParameter(parameterName, (byte[])value);
            }
            IDataParameter param = new SqlParameter(parameterName, CommOp.TestNull(value));
            return param;
        }

        /// <summary>
        /// 生成二进制参数SqlParameter
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IDataParameter CreateImageParameter(string parameterName, byte[] value)
        {
            IDataParameter param = new SqlParameter(parameterName, SqlDbType.Image);
            param.Value = CommOp.TestNull(value);
            return param;
        }
        /// <summary>
        /// 获取数据库的大小
        /// </summary>
        /// <returns></returns>
        public virtual double GetDBSize()
        {
            DataSet ds = Helper.ExecDataSet("sp_spaceused");
            string r = (string)ds.Tables[0].Rows[0]["database_size"];
            return CommOp.ToDouble(r.Split(' ')[0]);
        }
        /// <summary>
        /// 收缩数据库
        /// </summary>
        public virtual void ShrinkDB()
        {
            string sql = String.Format(@"Backup Log {0} with no_log
dump transaction {0} with no_log
USE {0} 
DBCC SHRINKFILE (2)", Helper.GetDBName());
            Helper.ExecNonQuery(sql);
        }

        /// <summary>
        /// 数据分页查询 异步版本
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="sp"></param>
        /// <returns></returns>
        public async Task<IDataReader> ExecPageReaderAsync(IPager p, params IDataParameter[] sp)
        {
            if (p is DBPagerInfo)
            {
                DBPagerInfo pager = p as DBPagerInfo;
                var tp = GetSortFieldFromOrderBy(pager.OrderBy, pager.KeyId);
                string orderby = tp[0];
                string sql = "SELECT COUNT(1) FROM (" + pager.Query + ")c";
                pager.RecordCount = CommOp.ToInt(await Helper.ExecGetObjectAsync(sql, sp));
                sql = $@"WITH PAGED AS ( 
SELECT ROW_NUMBER() OVER(ORDER BY {pager.OrderBy}) AS rowNum, 
* FROM ({pager.Query})a)
SELECT TT.*  FROM PAGED P INNER JOIN ({pager.Query})TT 
ON P.{pager.KeyId} = TT.{pager.KeyId}  WHERE ROWNUM BETWEEN {pager.StartIndex + 1} AND {pager.StartIndex + pager.PageSize}
ORDER BY {pager.OrderBy}";
                //pager.OrderBy, pager.Query, pager.KeyId, pager.StartIndex + 1, pager.StartIndex + pager.PageSize);
                return await Helper.ExecReaderAsync(sql, sp);
            }

            else if (p is MasterDetailPagerInfo)
            {
                return await ExecMasterDetailPageReaderAsync((MasterDetailPagerInfo)p, sp);
            }
            else
            {
                throw new ArgumentException($"请传入{nameof(DBPagerInfo)}或{nameof(MasterDetailPagerInfo)}");
            }
        }

        async Task<IDataReader> ExecMasterDetailPageReaderAsync(MasterDetailPagerInfo pager, params IDataParameter[] sp)
        {
            var tp = GetSortFieldFromOrderBy(pager.OrderBy, pager.KeyId);
            string sortFields = tp[1];
            string innerSorts = tp[2];
            string orderby = tp[0];
            string innerId = pager.KeyId.Split('.').Last();
            string filter = pager.Filter.IsEmpty() ? "" : " where " + pager.Filter;
            string sql = $"select COUNT(distinct({pager.KeyId})) from "
               + $"{pager.TablesAndJoins}{filter}";
            if (!innerSorts.IsEmpty())
            {
                innerSorts = "," + innerSorts;
            }
            if (!sortFields.IsEmpty())
            {
                sortFields = "," + sortFields;
            }
            pager.RecordCount = CommOp.ToInt(await Helper.ExecGetObjectAsync(sql, sp));
            sql = $@"select {pager.Fields} from {pager.TablesAndJoins} where
               {pager.KeyId} in
                (select {innerId} from
                (select {innerId}{innerSorts},row_number() over (order by {orderby}) r from(
               select distinct({pager.KeyId}){sortFields} from {pager.TablesAndJoins}{filter}
              )g__
               )h__ where r between {pager.StartIndex + 1} and {pager.StartIndex + pager.PageSize})
               order by {orderby}";
            return await Helper.ExecReaderAsync(sql, sp);
        }

        private string[] GetSortFieldFromOrderBy(string orderBy, string keyId)
        {
            keyId = Helper.AddFix(keyId);
            orderBy = orderBy.ToStr();
            if (orderBy.IsEmpty())
            {
                throw new ArgumentException($"{nameof(PagerInfo)}没有指定OrderBy属性");
            }
            string[] orders = orderBy.Split(',');
            var fieldArr = orders.Select(o =>
            {
                string prop = o.Trim().Split(' ')[0];
                string field = String.Join(".", prop.Split('.').Select(a => Helper.AddFix(a)));
                string innerField = Helper.AddFix(prop.Split('.').Last());
                if (innerField == keyId)
                {
                    innerField = null;
                }
                if (field == keyId)
                {
                    innerField = null;
                }
                return new { prop, field, innerField };
            });
            fieldArr.OrderBy(pf => -pf.field.Length).Each(pf => orderBy = orderBy.Replace(pf.prop, pf.field));
            string fields = String.Join(",", fieldArr.Where(pf => pf.innerField != null).Select(pf => pf.field));
            string innerFields = String.Join(",", fieldArr.Where(pf => pf.innerField != null).Select(pf => pf.innerField));
            return new string[] { orderBy, fields, innerFields };
        }

        /// <summary>
        /// 数据分页查询
        /// </summary>
        /// <param name="pager"></param>
        /// <param name="sp"></param>
        /// <returns></returns>
        public IDataReader ExecPageReader(DBPagerInfo pager, params IDataParameter[] sp)
        {
            string sql = "SELECT COUNT(*) FROM (" + pager.Query + ")c";

            pager.RecordCount = (int)Helper.ExecGetObject(sql, sp);

            sql = String.Format(@"WITH PAGED AS ( 
SELECT ROW_NUMBER() OVER(ORDER BY {0}) AS rowNum, 
* FROM ({1})a)
SELECT TT.*  FROM PAGED P INNER JOIN ({1})TT 
ON P.{2} = TT.{2}  WHERE ROWNUM BETWEEN {3} AND {4}
ORDER BY {0}",
            pager.OrderBy, pager.Query, pager.KeyId, pager.StartIndex + 1, pager.StartIndex + pager.PageSize);
            return Helper.ExecReader(sql, sp);
        }


        public string TypeMappingName(Type type, int maxLength)
        {
            switch (type.Name.ToLower())
            {
                case "int32": return "int";
                case "int64": return "bigint";
                case "bool": return "bit";
                case "byte[]": return "image";
                case "float": return "real";
                case "double": return "float";
                case "decimal": return "numeric(18,4)";
            }
            return "nvarchar(" + maxLength + ")";
        }

        public bool TableExists(string tableName)
        {
            object o = Helper.ExecGetObject("select COUNT(*) from sysobjects where name=@tableName ", Helper.CreateParameter("tableName", tableName));
            return CommOp.ToInt(o) == 1;
        }
        public async Task<bool> TableExistsAsync(string tableName)
        {
            object o = await Helper.ExecGetObjectAsync("select COUNT(*) from sysobjects where name=@tableName ", Helper.CreateParameter("tableName", tableName));
            return CommOp.ToInt(o) == 1;
        }


        public void CreateTable(DataTable dt, string tableName)
        {
            if (tableName.IsEmpty())
            {
                tableName = dt.TableName;
            }

            string strSql = string.Format("create table {0}(", tableName);
            foreach (DataColumn c in dt.Columns)
            {
                strSql += string.Format("[{0}] {1},", c.ColumnName, TypeMappingName(c.DataType, c.MaxLength < 0 ? 255 : c.MaxLength));
            }
            strSql = strSql.Trim(',') + ")";

            Helper.ExecNonQuery(strSql);
        }
        public async Task CreateTableAsync(DataTable dt, string tableName)
        {
            if (tableName.IsEmpty())
            {
                tableName = dt.TableName;
            }

            string strSql = string.Format("create table {0}(", tableName);
            foreach (DataColumn c in dt.Columns)
            {
                strSql += string.Format("[{0}] {1},", c.ColumnName, TypeMappingName(c.DataType, c.MaxLength < 0 ? 255 : c.MaxLength));
            }
            strSql = strSql.Trim(',') + ")";

            await Helper.ExecNonQueryAsync(strSql);
        }


        public int BuckCopy(DataTable dt, string tableName, int notifyAfter, Action<int> onRowsCopied)
        {
            int rowsCopied = 0;
            try
            {

                using (SqlBulkCopy bcp = new SqlBulkCopy(Helper.ConnStr))
                {
                    bcp.SqlRowsCopied += (s, e) =>
                    {
                        rowsCopied = (int)e.RowsCopied;
                        onRowsCopied(rowsCopied);
                    };
                    // bcp.BatchSize = Math.Max(dt.Rows.Count / 20, 1);//每次传输的行数   
                    bcp.BatchSize = 1000;//每次传输的行数   
                    bcp.NotifyAfter = notifyAfter;//进度提示的行数   
                    bcp.DestinationTableName = "[" + tableName + "]";//目标表   
                    bcp.WriteToServer(dt);
                }
                return rowsCopied;
            }
            catch (Exception ex)
            {
                throw new TableImportException(ex, rowsCopied + 1, 0);
            }
        }

        /// <summary>
        /// 表示参数开头的符号
        /// </summary>
        public string ParamPrefix
        {
            get { return "@"; }
        }

        /// <summary>
        /// 表示字段开头的符号
        /// </summary>
        public string FieldPrefix
        {
            get { return "["; }
        }

        /// <summary>
        /// 表示字段结束的符号
        /// </summary>
        public string FieldSuffix
        {
            get { return "]"; }
        }

        public DbCommandBuilder CreateCommandBuilder(DbDataAdapter sda)
        {
            return new SqlCommandBuilder((SqlDataAdapter)sda);
        }

        public DbCommand CreateCommand(DbConnection connection)
        {
            return connection.CreateCommand();
        }
    }
}

