namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// show GUI.Button for bool,(int [0,1])
    /// 
    ///     [EditorButton(onClickCall ="Test")]
    ///     public bool isTest;
    ///     
    ///     Test(){}
    /// </summary>
    public class EditorButtonAttribute : PropertyAttribute
    {
        public enum NameType
        {
            MethodName, // method name called
            FieldName, // current property name
            AttributeText // text
        }
        /// <summary>
        /// call instance method
        /// </summary>
        public string onClickCall;
        /// <summary>
        /// button 's text
        /// </summary>
        public string text;
        /// <summary>
        /// button ' texture
        /// </summary>
        public string imageAssetPath;

        public NameType nameType = NameType.MethodName;
        public string tooltip;
    }
}
