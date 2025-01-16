using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    public static partial class Layers
    {
        public static string
            Default = nameof(Default)
            //,TestLayer = "test layer"
            ;

        public static List<string> GetLayers() => ReflectionTools.GetFieldValues<string>(typeof(Layers));
    }
}
