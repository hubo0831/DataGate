using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataGate.Com
{
    public class TreeDataManager<T> : TreeDataManagerBase<T, string>
           where T : class,IIdNameParentId<string>
    {

    }

    public class TreeDataManagerInt<T> : TreeDataManagerBase<T, int>
       where T : class,IIdNameParentId<int>
    {

    }

}
