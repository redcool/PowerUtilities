using System.Collections.Generic;

namespace PowerUtilities
{
    public static partial class Tags
    {
        /// <summary>
        /// this tags will auto add to UntiyEditor
        /// </summary>
        public const string
            Player = nameof(Player)
            ,Monster = nameof(Monster)
            ,MainCamera = nameof(MainCamera)
            ,UICamera = nameof(UICamera)
            ,FXCamera = nameof(FXCamera)
            ,ReflectionCamera = nameof(ReflectionCamera)
            ,EditorOnly = nameof(EditorOnly)
            ,BigShadowLight = nameof(BigShadowLight)
            //,Test="Test tag"
            ;

        public static List<string> GetTags() => ReflectionTools.GetFieldValues<string>(typeof(Tags));
        
    }
}
