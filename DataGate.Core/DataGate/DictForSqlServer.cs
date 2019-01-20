using System;
using System.Collections.Generic;
using System.Text;

namespace DataGate
{
    /// <summary>
    /// Oracle数据字典Sql语句提供程序
    /// </summary>
    public class DictForSqlServer : IDictFor
    {
        /// <summary>
        /// sqlserver数据字典语句
        /// TableName, Remark, ColumnName, PrimaryKey， TypeName, MaxLength,Precision,Scale,Required,Value,Title
        /// </summary>
        public string DictSql
        {
            get
            {
                //https://www.cnblogs.com/gouguo/p/8510008.html
                return @"SELECT 
[TableName]=d.name, 
[Remark]=f.value,
a.colorder, 
[ColumnName]=a.name, 
IsIdentity=case when COLUMNPROPERTY(a.id,a.name,'IsIdentity')=1 then 1 else null end, 
[PrimaryKey]=case when exists(SELECT 1 FROM sysobjects where xtype='PK' and name in (
SELECT name FROM sysindexes WHERE indid in(
SELECT indid FROM sysindexkeys WHERE id = a.id AND colid=a.colid 
))) then 1 else null end, 
[TypeName]=b.name, 
[MaxLength]=a.length, 
[Precision]=COLUMNPROPERTY(a.id,a.name,'PRECISION'), 
[Scale]=isnull(COLUMNPROPERTY(a.id,a.name,'Scale'),0), 
[Required]=case when a.isnullable=1 then null else 1 end, 
[Value]= e.text, 
[Title]= g.[value]  
FROM syscolumns a 
left join systypes b on a.xtype=b.xusertype 
inner join sysobjects d on a.id=d.id and d.xtype='U' and d.name<>'dtproperties' 
left join syscomments e on a.cdefault=e.id 
left join sys.extended_properties g on a.id=g.major_id and a.colid=g.minor_id 
left join sys.extended_properties f on d.id=f.major_id and f.minor_id =0 
--where d.name='要查询的表' --如果只查询指定表,加上此条件 
order by d.name,a.colorder";
            }
        }

    }
}
