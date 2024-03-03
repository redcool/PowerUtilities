﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    /// <summary>
    /// Handle common compare functions
    /// </summary>
    public static class CompareTools
    {
        /// <summary>
        /// Compare lastValue,currentValue,
        ///     not equals :
        ///         1 set lastValue = currentValue
        ///         2 return true
        ///     equals : 
        ///         1 do nothing
        ///         2 return false
        /// </summary>
        /// <param name="lastValue"></param>
        /// <param name="currentValue"></param>
        /// <returns>is changed triggered</returns>
        public static bool CompareAndSet<T>(ref T lastValue,ref T currentValue) where T : IEquatable<T> 
        {
            if(!lastValue.Equals(currentValue))
            {
                lastValue = currentValue;
                return true;
            }
            return false;
        }
        /// <summary>
        /// is true frist time, v will set after called
        /// 
        /// like this : 
        /// if(!v){
        ///     // do actions
        ///     v = true
        /// }
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="v"></param>
        /// <param name="initValue"></param>
        /// <returns></returns>
        public static bool IsSetFirstTime(ref bool v)
        {
            if (!v)
            {
                v = true;
                return true;
            }
            return false;
        }
    }
}
