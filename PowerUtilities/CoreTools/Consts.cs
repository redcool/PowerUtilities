using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    public static partial class Consts
    {
        public const string PowerUtlities = nameof(PowerUtlities);
    }

    public static partial class Tags
    {
        public const string 
            Player = nameof(Player),
            Monster = nameof(Monster),
            MainCamera = nameof(MainCamera),
            ReflectionCamera = nameof(ReflectionCamera),
            EditorOnly = nameof(EditorOnly)
            ;
    }
}
