using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Data;

namespace DataGate.Com.DB
{
    /// <summary>
    /// 分页类基类,定义从大数据集返回部分数据的位置和数量等信息
    /// </summary>
    public class PagerInfo : IPager
    {
        private int recordCount;
        private int absRowIndex;

        /// <summary>
        /// 分页对象名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 页编号, 从0开始计数
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int PageCount
        {
            get { return (RecordCount - 1) / PageSize + 1; }
        }

        /// <summary>
        /// 记录数量
        /// </summary>
        public int RecordCount
        {
            get { return recordCount; }
            set
            {
                recordCount = value;
               // Validate();
            }
        }

        /// <summary>
        /// 当前的绝对行号, 从0开始
        /// </summary>
        public int AbsRowIndex
        {
            get { return absRowIndex; }
            set { absRowIndex = value; }
        }

        /// <summary>
        /// 本页起始的行号, 从0开始计数
        /// </summary>
        public int StartIndex
        {
            get { return PageIndex * PageSize; }
            set { PageIndex = value / PageSize; }
        }

        /// <summary>
        /// 本页实际的起始行号
        /// </summary>
        public int Offset
        {
            get;
            set;
        }

        /// <summary>
        /// 以recordcount为基准，验证页码的有效性
        /// </summary>
        public void Validate()
        {
            if (StartIndex >= recordCount) StartIndex = recordCount - PageSize;
            if (StartIndex < 0) StartIndex = 0;
        }
    }
}
