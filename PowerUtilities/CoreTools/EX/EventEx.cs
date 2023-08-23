using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    public static class EventEx
    {
        public static bool IsMouseDown(this Event e, int mouseButton = 0)
        {
            if (e.control
                || e.alt
                || e.shift
                )
            {
                return false;
            }
            return e.type == EventType.MouseDown && e.button == mouseButton;
        }
        public static bool IsMouseLeftDown(this Event e)
        => IsMouseDown(e);
        public static bool IsmouseRightButton(this Event e)
        => IsMouseDown(e, 1);
    }
}
