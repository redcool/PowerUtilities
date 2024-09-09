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
        public static bool IsMouseTrigger(this Event e,EventType eventType, int mouseButton = 0)
        {
            if (e.control
                || e.alt
                || e.shift
                )
            {
                return false;
            }
            return e.type == eventType && e.button == mouseButton;
        }

        public static bool IsMouseLeftDown(this Event e)
        => IsMouseTrigger(e, EventType.MouseDown);
    }
}
