using System;
using System.Collections.Generic;
using System.Text;

namespace DataGate
{
    /// <summary>
    /// Oracle数据字典Sql语句提供程序
    /// </summary>
    public class DictForOracle : IDictFor
    {
        /// <summary>
        /// oracle数据字典语句
        /// </summary>
        public string DictSql
        {
            get
            {
                //https://blog.csdn.net/KnuthZ/article/details/77864120
                return @"SELECT 
       COL.TABLE_NAME TableName,
       TT.COMMENTS Remark,
       COL.COLUMN_NAME AS ColumnName,
       CASE WHEN PKCOL.COLUMN_POSITION > 0 THEN 1 ELSE null END AS PrimaryKey,
       COL.DATA_TYPE AS TypeName,
       COL.DATA_LENGTH MaxLength,
       COL.DATA_PRECISION AS Precision,
       COL.DATA_SCALE AS Scale,
       CASE WHEN COL.NULLABLE = 'Y' THEN null ELSE 1 END AS Required,
       COL.DATA_DEFAULT  AS Value,
       CCOM.COMMENTS AS Title 
FROM 
       USER_TAB_COLUMNS COL,
       USER_COL_COMMENTS CCOM,
       (SELECT AA.TABLE_NAME,
               AA.INDEX_NAME,
               AA.COLUMN_NAME,
               AA.COLUMN_POSITION
          FROM USER_IND_COLUMNS AA, USER_CONSTRAINTS BB
         WHERE BB.CONSTRAINT_TYPE = 'P'
           AND AA.TABLE_NAME = BB.TABLE_NAME
           AND AA.INDEX_NAME = BB.CONSTRAINT_NAME
        ) PKCOL,
       USER_TAB_COMMENTS TT
WHERE COL.TABLE_NAME = CCOM.TABLE_NAME
      AND COL.COLUMN_NAME = CCOM.COLUMN_NAME
      AND COL.TABLE_NAME = TT.TABLE_NAME(+)
      AND COL.COLUMN_NAME = PKCOL.COLUMN_NAME(+)
      AND COL.TABLE_NAME = PKCOL.TABLE_NAME(+)
ORDER BY COL.TABLE_NAME,col.column_id";
            }
        }

    }
}
