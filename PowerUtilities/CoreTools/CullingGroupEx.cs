using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PowerUtilities
{
    public static class CullingGroupEx
    {
        /// <summary>
        /// Retrieves the indices of elements within the specified range of a CullingGroup that are currently visible, and
        /// stores them in the provided result array.
        /// </summary>
        /// <remarks>If no elements are visible or if <paramref name="isVisible"/> is <see langword="false"/>, the method
        /// returns 0 and the result array is not modified. The method does not perform bounds checking on the result array;
        /// ensure it is large enough to hold all possible results.</remarks>
        /// <param name="group">The CullingGroup instance to query for visible elements. Cannot be null.</param>
        /// <param name="isVisible">A value indicating whether to include only visible elements. If <see langword="true"/>, only indices of visible
        /// elements are returned; otherwise, no indices are returned.</param>
        /// <param name="result">An array in which to store the resulting indices.indices range [0,size] Must be large enough to hold all matching indices.</param>
        /// <param name="startIndex">The zero-based index of the first element in the CullingGroup to check for visibility.</param>
        /// <param name="size">The number of elements to check for visibility, starting from <paramref name="startIndex"/>.</param>
        /// <returns>The number of indices written to the result array.</returns>
        public static int QueryIndices(this CullingGroup group, bool isVisible, int[] result, int startIndex,int size)
        {
            var count = 0;
            for (int i = 0; i < size; i++)
            {
                var id = i + startIndex;

                if (group.IsVisible(id) && isVisible)
                {
                    result[count] = i;
                    count++;
                }
            }

            return count;
        }

        public static int QueryIndices(this CullingGroup group, int distanceBand, int[] result, int startIndex, int size)
        {
            var count = 0;
            for (int i = 0; i < size; i++)
            {
                var id = i + startIndex;
                var distBand = group.GetDistance(id);
                if (distBand == distanceBand)
                {
                    result[count] = i;
                    count++;
                }
            }

            return count;
        }

        public static int QueryIndices(this CullingGroup group,bool isVisible, int distanceBand, int[] result, int startIndex, int size)
        {
            var count = 0;
            for (int i = 0; i < size; i++)
            {
                var id = i + startIndex;
                var isIdVisible = group.IsVisible(id);
                var distBand = group.GetDistance(id);
                if (distBand == distanceBand && isIdVisible == isVisible)
                {
                    result[count] = i;
                    count++;
                }
            }

            return count;
        }
        /// <summary>
        /// Disposes of the CullingGroup instance safely.
        /// </summary>
        /// <param name="cullingGroup"></param>
        public static void DestroySafe(ref CullingGroup cullingGroup)
        {
            cullingGroup?.Dispose();
            cullingGroup = null;
        }
    }
}
