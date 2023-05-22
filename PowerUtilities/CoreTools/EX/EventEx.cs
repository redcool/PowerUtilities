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
            return e.type == EventType.MouseDown && e.button == 0;
        }
    }
}
