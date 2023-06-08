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
        public static bool IsMouseLeftDown(this Event e)
        {
            if (e.control
                || e.alt 
                || e.shift
                )
            {
                return false;
            }
            return e.type == EventType.MouseDown && e.button == 0;
        }
    }
}
