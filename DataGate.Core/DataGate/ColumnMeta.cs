using System;
using System.Collections.Generic;
using System.Text;

namespace DataGate
{
    /// <summary>
    /// 字段的字典定义
    /// </summary>
    public class ColumnMeta
    {
        public string TableName { get; set; }

        public string Remark { get; set; }

        public string ColumnName { get; set; }

        public bool? PrimaryKey { get; set; }

        public string TypeName { get; set; }

        public int? MaxLength { get; set; }

        public int? Precision { get; set; }

        public int? Scale { get; set; }

        public bool? Required { get; set; }

        public string Value { get; set; }

        public string Title { get; set; }
    }
}
