using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// Cooperate with EditorGroup(decorator)
    /// </summary>
    public class EditorGroupItemAttribute : PropertyAttribute
    {
        public string groupName;

        public EditorGroupItemAttribute(string groupName)
        {
            this.groupName = groupName;
        }
    }
}
