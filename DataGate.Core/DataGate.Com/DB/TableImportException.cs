using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataGate.Com.DB
{
    /// <summary>
    /// 从DataTable导入到数据库表出错时引发的异常
    /// </summary>
    [Serializable]
    public class TableImportException : Exception
    {
        /// <summary>
        /// 出错的行号
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// 出错的列号
        /// </summary>
        public int Col { get; set; }

        /// <summary>
        /// 生成新的异常
        /// </summary>
        /// <param name="ex">原始异常</param>
        /// <param name="row">Excel的行号</param>
        /// <param name="col">Excel的列号</param>
        public TableImportException(Exception ex, int row, int col)
            :base(ex.Message, ex)
        {
            Row = row;
            Col = col;
        }
    }
}
