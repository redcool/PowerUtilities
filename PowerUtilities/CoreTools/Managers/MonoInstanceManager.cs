using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// Manager objects(like monoBehaviour instance)
    /// 
    /// 1 Add 
    /// 2 Remove
    /// 3 UpdateMonoEnable
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonoInstanceManager<T> where T : MonoBehaviour
    {
        // trace instances
        List<T> instanceList = new List<T>();
        public T TopInstance { get; set; }

        public void SetAllEnable(bool enabled)
        {
            for (int i = 0; i < instanceList.Count; i++)
            {
                var item = instanceList[i];
                item.enabled = enabled;
            }
        }

        /// <summary>
        /// call when Start
        /// </summary>
        /// <param name="instance"></param>
        public void Add(T instance)
        {
            if (instanceList.Contains(instance))
                return;

            SetAllEnable(false);

            instanceList.Add(instance);
            TopInstance = instanceList[instanceList.Count - 1];

            TopInstance.enabled = true;
        }

        /// <summary>
        /// call when OnDestroy
        /// </summary>
        /// <param name="instance"></param>
        public void Remove(T instance)
        {
            TopInstance = default;
            instanceList.Remove(instance);

            SetAllEnable(false);

            if (instanceList.Count > 0)
            {
                TopInstance = instanceList[instanceList.Count - 1];

                TopInstance.enabled = true;
            }
        }

        public List<T> InstanceList
            => instanceList;
    }
}
