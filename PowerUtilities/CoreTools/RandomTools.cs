using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PowerUtilities
{
    public static class RandomTools
    {
        public static Vector3 Range(Vector3 a,Vector3 b)
        {
            a.x = Random.Range(a.x, b.x);
            a.y = Random.Range(a.y, b.y);
            a.z = Random.Range(a.z, b.z);
            return a;
        }
    }
}
