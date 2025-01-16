using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    public static partial class SortingLayers
    {
        public static string Default = nameof(Default),
            TestLayer = "TestLayer"
            ;

        public static List<string> GetSortingLayers() => ReflectionTools.GetFieldValues<string>(typeof(SortingLayers));
    }
}
