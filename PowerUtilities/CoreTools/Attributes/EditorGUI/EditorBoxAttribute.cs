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
        
        string[] propNames;
        public string[] PropNames => propNames;
        public string propName;

        public string header;
        public bool isShowFoldout;

        [HideInInspector]
        public bool isFolded=true;

        /// <summary>
        /// Show EditorGUI in Box(hbox,vbox)
        /// </summary>
        /// <param name="header"></param>
        /// <param name="propName"></param>
        public EditorBoxAttribute(string header, string propName)
        {
            if (!string.IsNullOrEmpty(propName))
                propNames = propName.SplitBy();
            this.header = header;
        }
    }
}
