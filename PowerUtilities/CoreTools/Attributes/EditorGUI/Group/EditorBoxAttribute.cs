namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;

    /**
     
        [EditorBoxAttribute("Path Tools Buttons", "isSetSpline,isReadSpline,isStampPaths", isShowFoldout = true, boxType = EditorBoxAttribute.BoxType.HBox)]
        [EditorButton(onClickCall = "SetSpline")]
        [Tooltip("set poslist to mainSpline")]
        public bool isSetSpline;

        [EditorButton(onClickCall = "ReadSpline")]
        [HideInInspector] public bool isReadSpline;

        [EditorButton(onClickCall = "StampPaths")]
        [HideInInspector] public bool isStampPaths;
     */
    public class EditorBoxAttribute : PropertyAttribute
    {
        public enum BoxType
        {
            VBox,HBox
        }

        public BoxType boxType;
        
        /// <summary>
        /// property name array,like Name1,Name2
        /// </summary>
        string[] propNames;
        public string[] PropNames => propNames;

        /// <summary>
        /// Show a title
        /// </summary>
        public string header;
        /// <summary>
        /// Show Content
        /// </summary>
        public bool isShowFoldout;

        [HideInInspector]
        public bool isFolded=true;

        public string boxStyle = "Box";

        /// <summary>
        /// Show EditorGUI in Box(hbox,vbox)
        /// </summary>
        /// <param name="header"></param>
        /// <param name="propNamesStr"></param>
        public EditorBoxAttribute(string header, string propNamesStr, string propWidthRatesStr = "")
        {
            this.header = header;
            if (!string.IsNullOrEmpty(propNamesStr))
                propNames = propNamesStr.SplitBy();

        }
    }
}
