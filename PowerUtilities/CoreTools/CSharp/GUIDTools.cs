using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    public static class GUIDTools
    {
        /// <summary>
        /// Get new when guid is invalid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static string GetGUID(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                guid = Guid.NewGuid().ToString();
            }
            return guid;
        }
    }
}
