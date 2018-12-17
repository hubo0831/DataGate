using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Configuration;
using System.Text.RegularExpressions;
using DataGate.Com;
using System.Reflection;
using System.Linq;
using DataGate.Com.DB;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace DataGate.Com.DB
{

    /// <summary>
    /// 通用数据访问类
    /// </summary>
    public partial class DBHelper : IDisposable
    {
        Dictionary<string, string> versions;
        IDBComm dBComm;
        DbTransaction _trans;
        DbConnection _transConn;

        /// <summary>
        /// 用于记录SQL日志的委托
        /// </summary>
        public Action<string, IDataParameter[]> Log { get; set; } = (sql, sp) =>
        {
            sp = sp ?? new IDataParameter[0];
            string sps = String.Join(",", sp.Select(p => $"{p.ParameterName}={p.Value}"));
            Debug.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} \n{sql}\n{sps}");
        };

        /// <summary>
        /// 数据库链接字符串
        /// </summary>
        public String ConnStr { get; set; }

        /// <summary>
        /// 超时时间
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// 数据库对象的公共接口
        /// </summary>
        public virtual IDBComm DBComm
        {
            get
            {
                return dBComm;
            }
            set
            {
                dBComm = value;
                dBComm.Helper = this;
            }
        }

        /// <summary>
        /// <summary>
        /// 创建一个默认的数据库帮助类
        /// </summary>
        public DBHelper()
        {
        }

        /// <summary>
        /// 根据连接字符串和数据接口类创建一个DBHelper
        /// </summary>
        /// <param name="connStr"></param>
        /// <param name="dbComm"></param>
        public DBHelper(string connStr, IDBComm dbComm)
        {
            this.ConnStr = connStr;
            this.DBComm = dbComm;
        }

        /// <summary>
        /// 获取数据库系统版本
        /// </summary>
        /// <returns>系统版本号</returns>
        public virtual String GetServerVersion()
        {
            if (versions == null)
                TestConnetion();
            return versions["ServerVersion"];
        }

        /// <summary>
        /// 获取数据库名
        /// </summary>
        /// <returns>数据库名称</returns>
        public virtual String GetDBName()
        {
            if (versions == null)
                TestConnetion();
            return versions["Database"];
        }

        /// <summary>
        /// 测试Connection
        /// </summary>
        /// <returns>测试连接成功与否</returns>
        public virtual bool TestConnetion()
        {
            try
            {
                versions = new Dictionary<string, string>();
                _transConn = DBComm.CreateConnection();
                _transConn.Open();
                versions["ServerVersion"] = _transConn.ServerVersion;
                versions["Database"] = _transConn.Database;
                versions["DataSource"] = _transConn.DataSource;
            }
            catch
            {
                return false;
            }
            finally
            {
                _transConn.Close();
            }
            return true;
        }

        /// <summary>
        /// 开始一个事务
        /// </summary>
        public virtual DbTransaction BeginTrans()
        {
            _transConn = DBComm.CreateConnection();
            _transConn.Open();
            _trans = _transConn.BeginTransaction();
            return _trans;
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public virtual void RollbackTrans()
        {
            _trans.Rollback();
            _transConn.Close();
        }

        /// <summary>
        /// 结束事务
        /// </summary>
        /// <returns>结束事务出错时返回的错误信息</returns>
        public virtual String EndTrans()
        {
            String s = "";
            try
            {
                _trans.Commit();
            }
            catch (DbException ex)
            {
                _trans.Rollback();
                s = ex.Message;
            }

            _transConn.Close();
            return s;
        }

        /// <summary>
        /// 用指定连接串执行非查询语句
        /// </summary>
        /// <param name="sql">非查询语句</param>
        /// <param name="sp">可选参数数组</param>
        /// <returns>影响的行数</returns>
        public virtual int ExecNonQuery(string sql, params IDataParameter[] sp)
        {
            using (DbConnection conn = DBComm.CreateConnection())
            {
                DbCommand sc = DBComm.CreateCommand(conn);
                sc.CommandText = sql;
                PrepareCommand(sc, sp);
                int r = sc.ExecuteNonQuery();
                conn.Close();
                sc.Parameters.Clear();
                return r;
            }
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="procname">存储过程名称</param>
        /// <param name="sp">可选参数数组</param>
        /// <returns>影响的行数</returns>
        public virtual int RunProcedure(string procname, params IDataParameter[] sp)
        {
            using (DbConnection conn = DBComm.CreateConnection())
            {
                DbCommand sc = DBComm.CreateCommand(conn);
                sc.CommandText = procname;
                sc.CommandType = CommandType.StoredProcedure;
                PrepareCommand(sc, sp);
                int r = sc.ExecuteNonQuery();
                sc.Parameters.Clear();
                return r;
            }
        }

        /// <summary>
        /// 执行存储过程(返回DataSet)
        /// </summary>
        /// <param name="procname">存储过程名称</param>
        /// <param name="sp">可选参数数组</param>
        /// <returns>DataSet</returns>
        public virtual DataSet RunProcedureDs(string procname, params IDataParameter[] sp)
        {
            using (DbConnection conn = DBComm.CreateConnection())
            {
                DbCommand sc = DBComm.CreateCommand(conn);
                sc.CommandText = procname;
                sc.CommandType = CommandType.StoredProcedure;
                PrepareCommand(sc, sp);

                DbDataAdapter da = DBComm.CreateDataAdapter(sc);
                DataSet ds = new DataSet();
                da.Fill(ds);
                sc.Parameters.Clear();
                return ds;
            }
        }

        /// <summary>
        /// 执行事务中的非查询语句
        /// </summary>
        /// <param name="sql">非查询语句</param>
        /// <param name="sp">可选参数数组</param>
        /// <returns>影响的行数</returns>
        public virtual int TransNonQuery(string sql, params IDataParameter[] sp)
        {
            DbCommand sc = DBComm.CreateCommand(_transConn);
            sc.CommandText = sql;
            PrepareCommand(sc, sp);
            sc.Transaction = _trans;
            int i = sc.ExecuteNonQuery();
            sc.Parameters.Clear();
            return i;
        }

        /// <summary>
        /// 在事务中获取单个对象
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <param name="sp">可选参数数组</param>
        /// <returns>返回的单个值</returns>
        public virtual object TransGetObject(string sql, params IDataParameter[] sp)
        {
            object o = null;
            DbCommand sc = DBComm.CreateCommand(_transConn);
            sc.CommandText = sql;
            PrepareCommand(sc, sp);
            sc.Transaction = _trans;
            o = sc.ExecuteScalar();
            sc.Parameters.Clear();
            return o;
        }


        /// <summary>
        /// 得到单一对象
        /// </summary>
        /// <param name="sql">要执行的sql语句</param>
        /// <param name="sp">参数数组</param>
        /// <returns>返回的单个值</returns>
        public virtual object ExecGetObject(String sql, params IDataParameter[] sp)
        {
            object o = 0;
            using (DbConnection conn = DBComm.CreateConnection())
            {
                DbCommand sc = DBComm.CreateCommand(conn);
                sc.CommandText = sql;
                PrepareCommand(sc, sp);
                o = sc.ExecuteScalar();
                sc.Parameters.Clear();
                return o;
            }
        }

        private void PrepareCommand(DbCommand sc, IDataParameter[] sp)
        {
            if (sp != null)
            {
                //去掉参数前面的@符号或:符号
                foreach (IDataParameter parm in sp)
                {
                    if (parm.ParameterName.StartsWith("@"))
                    {
                        parm.ParameterName = parm.ParameterName.Substring(1);
                    }
                    else if (parm.ParameterName.StartsWith(DBComm.ParamPrefix))
                    {
                        parm.ParameterName = parm.ParameterName.Substring(1);
                    }
                }

                //使用正则表达式替换sql语句中参数前的@号到特定数据库的前导符号，如oracle的:
                foreach (IDataParameter parm in sp.OrderByDescending(p => p.ParameterName.Length))
                {
                    Regex reg = new Regex($"([^\\w]+)@{parm.ParameterName}([^\\w]+|$)", RegexOptions.IgnoreCase);
                    if (reg.IsMatch(sc.CommandText))
                    {
                        var arr = parm.Value as IEnumerable;
                        var str = parm.Value as String;
                        //如果参数值是字符串并以[开头,则转成数组
                        if (str != null && str.StartsWith("["))
                        {
                            arr = JsonConvert.DeserializeObject<JArray>(str).ToArray();
                            //arr = jarr.Select(a => a.ToString());
                            str = null;
                        }
                        //如果参数是数组，则替换成带,号的表达式形式
                        if (arr != null && str == null)
                        {
                            string inParams = String.Join(",", arr.Each(a => "'" + a + "'"));
                            sc.CommandText = reg.Replace(sc.CommandText, $"$1{inParams}$2");
                        }
                        else
                        {
                            sc.CommandText = reg.Replace(sc.CommandText, $"$1{DBComm.ParamPrefix + parm.ParameterName}$2");
                            sc.Parameters.Add(parm);
                        }
                    }
                }

                Log?.Invoke(sc.CommandText, sp);
            }
            sc.CommandTimeout = Timeout;
            if (sc.Connection.State != ConnectionState.Open)
                sc.Connection.Open();
        }

        /// <summary>
        /// 根据指定Sql返回一个DataReader
        /// </summary>
        /// <param name="sql">要执行的sql语句</param>
        /// <param name="sp">参数数组</param>
        /// <returns>IDataReader</returns>
        public virtual IDataReader ExecReader(string sql, params IDataParameter[] sp)
        {
            DbConnection conn = DBComm.CreateConnection();
            DbCommand sc = DBComm.CreateCommand(conn);
            sc.CommandText = sql;

            // we use a try/catch here because if the method throws an exception we want to 
            // close the connection throw code, because no datareader will exist, hence the 
            // commandBehaviour.CloseConnection will not work
            try
            {
                PrepareCommand(sc, sp);
                IDataReader rdr = sc.ExecuteReader(CommandBehavior.CloseConnection);
                sc.Parameters.Clear();
                return rdr;
            }
            catch
            {
                conn.Close();
                throw;
            }
        }

        /// <summary>
        /// 执行SQL查询语句，返回DataTable
        /// </summary>
        /// <param name="sql">要执行的Sql语句</param>
        /// <param name="sp">参数数组</param>
        /// <returns>DataTable</returns>
        public virtual DataTable ExecDataTable(string sql, params IDataParameter[] sp)
        {
            using (DbConnection conn = DBComm.CreateConnection())
            {
                DbCommand sc = DBComm.CreateCommand(conn);
                sc.CommandText = sql;
                PrepareCommand(sc, sp);
                DbDataAdapter da = DBComm.CreateDataAdapter(sc);
                DataTable dt = new DataTable();
                da.Fill(dt);

                sc.Parameters.Clear();
                return dt;
            }
        }

        /// <summary>
        /// 执行SQL查询语句，返回DataSet
        /// </summary>
        /// <param name="sql">要执行的Sql语句</param>
        /// <param name="sp">参数数组</param>
        /// <returns>DataSet</returns>
        public virtual DataSet ExecDataSet(string sql, params IDataParameter[] sp)
        {
            using (DbConnection conn = DBComm.CreateConnection())
            {
                DbCommand sc = DBComm.CreateCommand(conn);
                sc.CommandText = sql;
                PrepareCommand(sc, sp);
                DbDataAdapter da = DBComm.CreateDataAdapter(sc);
                DataSet ds = new DataSet();
                da.Fill(ds);
                sc.Parameters.Clear();
                return ds;
            }
        }

        /// <summary>
        /// 生成参数
        /// </summary>
        /// <param name="parameterName">参数名称</param>
        /// <param name="value">参数值</param>
        /// <returns>参数对象</returns>
        public virtual IDataParameter CreateParameter(string parameterName, object value)
        {
            return DBComm.CreateParameter(parameterName, CommOp.TestNull(value));
        }

        /// <summary>
        /// 获取数据库大小
        /// </summary>
        /// <returns>数据库大小</returns>
        public virtual double GetDBSize()
        {
            return DBComm.GetDBSize();
        }

        /// <summary>
        /// 压缩数据库
        /// </summary>
        public virtual void ShrinkDB()
        {
            DBComm.ShrinkDB();
        }

        /// <summary>
        /// 导入DataTable到数据库
        /// </summary>
        /// <param name="dt">内存中的数据表</param>
        /// <param name="tableName">表名,如果为空，则以传入的DataTable的TableName作为表名</param>
        /// <param name="buckCopy">是否使用批量导入, 对应的DBComm必须实现ISupportBuckCopy的接口</param>
        /// <param name="notifyAfter">发生提示时导入的行数</param>
        /// <param name="onRowsCopied">发生提示时执行的委托</param>
        /// <returns>成功导入的行数</returns>
        public virtual int Import(DataTable dt, string tableName = null, bool buckCopy = true, int notifyAfter = 10, Action<int> onRowsCopied = null)
        {
            if (dt == null)
            {
                throw new ArgumentNullException("dt");
            }
            if (onRowsCopied == null) onRowsCopied = r => { };

            if (tableName.IsEmpty())
            {
                tableName = dt.TableName;
            }
            if (notifyAfter <= 0)
            {
                throw new ArgumentException("notifyAfter<=0");
            }

            int rowsCopied = 0;
            int rowCount = dt.Rows.Count;

            //如果目标表不存在则创建
            if (!DBComm.TableExists(tableName))
            {
                DBComm.CreateTable(dt, tableName);
            }

            //用bcp导入数据
            if (buckCopy && DBComm is ISupportBuckCopy)
            {
                return ((ISupportBuckCopy)DBComm).BuckCopy(dt, tableName, notifyAfter, onRowsCopied);
            }
            else //用Sql Insert 导入数据
            {
                BeginTrans();
                string sqlFields = "";
                string sqlValues = "";
                foreach (DataColumn f in dt.Columns)
                {
                    if (!f.AutoIncrement)
                    {
                        sqlFields += String.Format(",{1}{0}{2}", f.ColumnName, DBComm.FieldPrefix, DBComm.FieldSuffix);
                        sqlValues += String.Format(",@{0}", f.ColumnName);
                    }
                }
                sqlFields = sqlFields.Substring(1);
                sqlValues = sqlValues.Substring(1);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    int j = 0;

                    IDataParameter[] sp = new IDataParameter[dt.Columns.Count];
                    foreach (DataColumn f in dt.Columns)
                    {
                        if (!f.AutoIncrement)
                        {
                            IDataParameter p = CreateParameter(f.ColumnName, CommOp.TestNull(dt.Rows[i][j]));
                            sp[j] = p;
                        }
                        j++;
                    }
                    string sql = String.Format("INSERT INTO {3}{0}{4}({1}) VALUES({2})", tableName, sqlFields, sqlValues, DBComm.FieldPrefix, DBComm.FieldSuffix);
                    try
                    {
                        TransNonQuery(sql, sp);
                    }
                    catch (Exception ex)
                    {
                        RollbackTrans();
                        throw new TableImportException(ex, i + 1, 0);
                    }
                    if (i % notifyAfter == 0 || i == rowCount)
                        onRowsCopied(i);
                }
                EndTrans();
            }
            rowsCopied = dt.Rows.Count;
            return rowsCopied;
        }


        /// <summary>
        /// 将value中的值赋给对象的属性
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="pi">对象的属性信息</param>
        /// <param name="value">值</param>
        void SetValue(object obj, PropertyInfo pi, object value)
        {
            pi.SetValue(obj, CommOp.HackType(value, pi.PropertyType), null);
        }

        public void Dispose()
        {
            if (_trans != null)
            {
                ((IDisposable)_trans).Dispose();
                _trans = null;
            }
        }
    }
}
