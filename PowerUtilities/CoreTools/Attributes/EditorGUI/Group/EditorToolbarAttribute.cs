namespace PowerUtilities
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// show GUI.Toolbat for int 
    /// 
    /// like: 
    ///     [EditorToolbar(onClickCall = "Test", texts = new[] { "a", "b" })]
    ///     public int test;
    ///     
    ///     Test(int buttonId){}
    /// </summary>
    public class EditorToolbarAttribute : PropertyAttribute
    {
        /// <summary>
        /// Func(int buttonId)
        /// </summary>
        public string onClickCall;
        public string[] texts;
        public string[] imageAssetPaths;
        public string[] tooltips;
    }
}
