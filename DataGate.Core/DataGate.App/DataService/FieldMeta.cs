using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataGate.App.DataService
{
    /// <summary>
    /// 字段元数据定义
    /// </summary>
    public class FieldMeta
    {
        /// <summary>
        /// 在编程模型中的实体名称
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// 用于表联查中指定 表名.名称
        /// 用于本模型中要经过联查得到其他表的字段
        /// </summary>
        [JsonProperty("foreignfield")]
        public string ForeignField { get; set; }

        /// <summary>
        /// 经名称转化后的实际数据库中的名称
        /// </summary>
        [JsonIgnore]
        public string DbName { get; set; }

        /// <summary>
        /// 带限定符的数据库中的名称，如sqlserver带[], oracle带""
        /// </summary>
        [JsonIgnore]
        public string FixDbName { get; set; }

        /// <summary>
        /// 显示在表格或表单中的列名
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// 是否是主键
        /// </summary>
        [JsonProperty("primarykey")]
        public bool PrimaryKey { get; set; }

        /// <summary>
        /// 外键对应的 表名.字段名
        /// </summary>
        [JsonProperty("foreignkey")]
        public string ForeignKey { get; set; }

        /// <summary>
        /// 该集合属性作为下拉列表时，决定下拉列表中对象的键值
        /// </summary>
        [JsonProperty("valuekey")]
        public string ValueKey { get; set; }

        /// <summary>
        /// 在表单中的控件类型
        /// </summary>
        [JsonProperty("uitype")]
        public string UIType { get; set; }

        /// <summary>
        /// 数据对像的类型，有String,Date等,默认是String
        /// 用[]号括起来是数组对象，[]中间是另一个实体名称
        /// </summary>
        [JsonProperty("datatype")]
        public string DataType { get; set; }

        /// <summary>
        /// 排序位
        /// </summary>
        [JsonProperty("order")]
        public int? Order { get; set; }

        /// <summary>
        /// 在表单中的排序位， 为空时和Order相同
        /// </summary>
        [JsonProperty("formorder")]
        public int? FormOrder { get; set; }

        /// <summary>
        /// 只读
        /// </summary>
        [JsonProperty("readonly")]
        public bool ReadOnly { get; set; }

        /// <summary>
        /// 必填项
        /// </summary>
        [JsonProperty("required")]
        public bool Required { get; set; }

        /// <summary>
        /// 最大长度
        /// </summary>
        [JsonProperty("maxlength")]
        public int MaxLength { get; set; }

        /// <summary>
        /// 显示宽度权重
        /// </summary>
        [JsonProperty("width")]
        public int Width { get; set; }

        /// <summary>
        /// 判断此属性是不是数组
        /// </summary>
        [JsonIgnore]
        public bool IsArray
        {
            get { return ArrayItemType != null; }
        }

        /// <summary>
        /// 从DataType中提取数组元素对应的实体类型
        /// </summary>
        [JsonIgnore]
        public string ArrayItemType
        {
            get
            {
                if ((DataType ?? "").StartsWith("["))
                {
                    return DataType.Substring(1, DataType.Length - 2);
                }
                return null;
            }
        }

        /// <summary>
        /// 默认值
        /// </summary>
        [JsonProperty("value")]
        public object Value { get; set; }

        /// <summary>
        /// 下拉列表中的可选项
        /// </summary>
        [JsonProperty("options")]
        public JArray Options { get; set; }

        /// <summary>
        /// 排序标志,
        /// </summary>
        [JsonProperty("sortable")]
        public bool Sortable { get; set; } = true;

        /// <summary>
        /// 注释或备注
        /// </summary>
        public string Remark { get; set; }
    }
}
