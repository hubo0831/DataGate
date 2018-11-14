using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DataGate.App.DataService
{
    /// <summary>
    /// 表的元数据
    /// </summary>
    public class TableMeta
    {
        public string Name { get; set; }

        /// <summary>
        /// 在实际数据库中的名称
        /// </summary>
        [JsonIgnore]
        public string DbName { get; set; }

        /// <summary>
        /// 带限定符的数据库中的名称，如sqlserver带[], oracle带""
        /// </summary>
        public string FixDbName { get; set; }

        private IEnumerable<FieldMeta> _fields;
        public IEnumerable<FieldMeta> Fields
        {
            get { return _fields; }
            set
            {
                _fields = value;
                var pkey = Fields.FirstOrDefault(f => f.PrimaryKey);
                if (pkey == null)
                {
                    pkey = Fields.FirstOrDefault(f => 
                    f.Name.Equals(Consts.DefaultKeyName, StringComparison.OrdinalIgnoreCase));
                    if (pkey != null) pkey.PrimaryKey = true;
                }
                var sort = Fields.FirstOrDefault(f => f.DataType == Consts.DataTypeSortOrder);
                if (sort == null)
                {
                    sort = Fields.FirstOrDefault(f => 
                    f.Name.Equals(Consts.DefaultSortName, StringComparison.OrdinalIgnoreCase));
                    if (sort != null) sort.DataType = Consts.DataTypeSortOrder;
                }
            }
        }

        [JsonIgnore]
        public IEnumerable<string> PropNames
        {
            get
            {
                return Fields.Select(f => f.Name);
            }
        }

        /// <summary>
        /// 获取主键集合
        /// </summary>
        public IEnumerable<FieldMeta> PrimaryKeys
        {
            get { return Fields.Where(f => f.PrimaryKey); }
        }

        /// <summary>
        /// 根据DBName生成DataTable对象
        /// </summary>
        /// <returns></returns>
        //public DataTable CreateDataTable()
        //{
        //    var columns = Fields
        //        .Select(p => new DataColumn(p.DbName));
        //    var pkey = PrimaryKeys;
        //    DataTable newDt = new DataTable(DbName);
        //    newDt.Columns.AddRange(columns.ToArray());
        //    newDt.PrimaryKey = columns.Where(col => PrimaryKeys.Any(pk => pk.Name == col.ColumnName))
        //        .ToArray();

        //    return newDt;
        //}

        /// <summary>
        /// 获取唯一主键，主键不只一个就会报错
        /// </summary>
        [JsonIgnore]
        public FieldMeta PrimaryKey
        {
            get
            {
                if (PrimaryKeys.Count() > 1)
                {
                    throw new ArgumentOutOfRangeException("该表不止一个主键");
                }
                return PrimaryKeys.FirstOrDefault();
            }
        }

    }
}
