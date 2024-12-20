using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    public static class ICollectionEx
    {
        public static void Add<T>(this List<T> list, params T[] items)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list.Add(items[i]);
            }
        }
    }
}
