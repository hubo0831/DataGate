using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataGate.Com
{
    public interface IIdNameParentId<TId> : IIdName<TId>
    {
        /// <summary>
        /// 用于表示父对象的ID
        /// </summary>
        TId ParentId { get; set; }
    }
}
