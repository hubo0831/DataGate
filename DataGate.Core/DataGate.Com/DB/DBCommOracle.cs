using DataGate.Com;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataGate.Com.DB
{
    /// <summary>
    /// Oracle的DBComm接口实现，基于Oracle提供的MDA
    /// MDA的特点是不需要额外安装客户端
    /// </summary>
    public class DBCommOracle : IDBComm
    {
        public DBHelper Helper
        {
            get;
            set;
        }

        public DbConnection CreateConnection()
        {
            return new Oracle.ManagedDataAccess.Client.OracleConnection(Helper.ConnStr);
        }

        public DbCommand CreateCommand(DbConnection connection)
        {
            var command = connection.CreateCommand() as OracleCommand;
            command.BindByName = true;
            return command;
        }

        public DbDataAdapter CreateDataAdapter(DbCommand command)
        {
            var adapter = new Oracle.ManagedDataAccess.Client.OracleDataAdapter((Oracle.ManagedDataAccess.Client.OracleCommand)command);
            return adapter;
        }

        public IDataParameter CreateParameter(string parameterName, object value)
        {
            if (value is byte[])
            {
                return CreateImageParameter(parameterName, (byte[])value);
            }
            
            var p = new Oracle.ManagedDataAccess.Client.OracleParameter(parameterName, CommOp.TestNull(value));
            return p;
        }

        public IDataParameter CreateImageParameter(string parameterName, byte[] value)
        {
            //var p = new OracleParameter(parameterName, DbType.Object);
            //p.Value = CommOp.TestNull(value);
            //return p;
            //张群龙改：
            var pp = new Oracle.ManagedDataAccess.Client.OracleParameter(parameterName, OracleDbType.Blob, value, ParameterDirection.InputOutput);
            return pp;
        }

        public async Task<IDataReader> ExecPageReaderAsync(IPager p, params IDataParameter[] sp)
        {
            if (p is DBPagerInfo)
            {
                DBPagerInfo pager = p as DBPagerInfo;
                var tp = GetSortFieldFromOrderBy(pager.OrderBy,pager.KeyId);
                string orderby = tp[0];
                string sql = "SELECT COUNT(1) FROM (" + pager.Query + ")c";
                pager.RecordCount = CommOp.ToInt(await Helper.ExecGetObjectAsync(sql, sp));
                sql = String.Format(@"SELECT * FROM(SELECT A.*, rownum r FROM({0} ORDER BY {1})A WHERE rownum <= {2})B WHERE r>{3}",
                    pager.Query, orderby, pager.StartIndex + pager.PageSize, pager.StartIndex);
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

        //        --查询条件也有子表
        //select u.*, r.*, ur.* from app_user u left join app_user_role ur on u.id = ur.user_id
        //left join app_role r on r.id = ur.role_id where u.id in
        //(select id from
        //(select id, failed_try, rownum r from (
        //select distinct(u.id), u.failed_try from app_user u left join app_user_role ur on u.id = ur.user_id
        //left join app_role r on r.id = ur.role_id
        // where u.name like '测试用户%' and r.name like '%管理员%'
        //  order by u.failed_try)g 
        //)h where r between 1 and 3)
        // order by u.failed_try;

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
            sql = $"select {pager.Fields} from {pager.TablesAndJoins} where{Environment.NewLine}" +
                $" {pager.KeyId} in{Environment.NewLine}"
                + $"(select {innerId} from"
                + $"(select {innerId}{innerSorts},rownum r from({Environment.NewLine}"
               + $"select distinct({pager.KeyId}){sortFields} from {pager.TablesAndJoins}{filter}{Environment.NewLine}"
                + $"order by {orderby})g__{Environment.NewLine}"
                + $")h__ where r between {pager.StartIndex + 1} and {pager.StartIndex + pager.PageSize}){Environment.NewLine}"
                + $"order by {orderby}";
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

        //https://blog.csdn.net/zhu_nana/article/details/52037464?utm_source=copy 
        //多表查询的分页：
        ////t1 为主表，t2为子表 
        //分页原则 先将主表分页，再查子表内容
        //SELECT t1.*, t2.*
        //FROM TABLENAME1 t1, TABLENAME2 t2 WHERE t1.ID = t2.PID
        //AND t2.PID IN(SELECT ID FROM(SELECT ID, rownum FROM TABLENAME1 WHERE rownum <=pageNum* pageSize) WHERE rownum > (pageNum-1)*pageSize)ORDER BY t1.VC_DATE DESC

        /*sqlserver的分页对比参考
        public IDataReader ExecPageReader(DBPagerInfo pager, params IDataParameter[] sp)
        {
            string sql = "SELECT COUNT(*) FROM (" + pager.Query + ")c";

            pager.RecordCount = (int)Helper.ExecGetObject(sql, sp);
            int startRow = pager.PageSize * (pager.PageIndex - 1) + 1;

            sql = String.Format(@"WITH PAGED AS ( 
SELECT ROW_NUMBER() OVER(ORDER BY {0}) AS rowNum, 
* FROM ({1})a)
SELECT TT.*  FROM PAGED P INNER JOIN ({1})TT 
ON P.{2} = TT.{2}  WHERE ROWNUM BETWEEN {3} AND {4}
ORDER BY {0}",
            pager.OrderBy, pager.Query, pager.KeyId, startRow, startRow + pager.PageSize - 1);
            return Helper.ExecReader(sql, sp);
        }
        */

        public double GetDBSize()
        {
            throw new NotImplementedException();
        }

        public void ShrinkDB()
        {
            throw new NotImplementedException();
        }

        public void CreateTable(DataTable dt, string tableName)
        {
            if (tableName.IsEmpty())
            {
                tableName = dt.TableName;
            }

            string strSql = String.Format("create table {0}(", tableName);
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

            string strSql = String.Format("create table {0}(", tableName);
            foreach (DataColumn c in dt.Columns)
            {
                strSql += string.Format("[{0}] {1},", c.ColumnName, TypeMappingName(c.DataType, c.MaxLength < 0 ? 255 : c.MaxLength));
            }
            strSql = strSql.Trim(',') + ")";

            await Helper.ExecNonQueryAsync(strSql);
        }

        string TypeMappingName(Type type, int maxLength)
        {
            switch (type.Name.ToLower())
            {
                case "int32": return "number";
                case "int64": return "number";
                case "bool": return "number";
                case "byte[]": return "blob";
                case "float": return "number";
                case "double": return "number";
                case "decimal": return "numeric(18,4)";
            }
            return "nvarchar(" + maxLength + ")";
        }

        public bool TableExists(string tableName)
        {
            object exist = Helper.ExecGetObject("SELECT count(*) FROM user_tables WHERE table_name=@tableName", Helper.CreateParameter("tableName", tableName.ToUpper()));
            return CommOp.ToInt(exist) == 1;
        }

        public async Task<bool> TableExistsAsync(string tableName)
        {
            object exist = await Helper.ExecGetObjectAsync("SELECT count(*) FROM user_tables WHERE table_name=@tableName", Helper.CreateParameter("tableName", tableName.ToUpper()));
            return CommOp.ToInt(exist) == 1;
        }

        public DbCommandBuilder CreateCommandBuilder(DbDataAdapter sda)
        {
            return new Oracle.ManagedDataAccess.Client.OracleCommandBuilder((Oracle.ManagedDataAccess.Client.OracleDataAdapter)sda);
        }

        public string ParamPrefix
        {
            get { return ":"; }
        }

        public string FieldPrefix
        {
            get { return ""; }//"\""; }
        }

        public string FieldSuffix
        {
            get { return ""; }// "\""; }
        }
    }
}
