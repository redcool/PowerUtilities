using System;

namespace PowerUtilities
{
    [Serializable]
    public struct RangeInfo
    {
        public int min, max;
        public RangeInfo(int min, int max)
        {
            this.min = min;
            this.max = max;
        }
    }
}
