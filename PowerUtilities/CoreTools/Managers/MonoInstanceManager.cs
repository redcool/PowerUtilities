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
        T topInstance;

        public void Add(T instance)
        {
            instanceList.Add(instance);
            topInstance = instanceList[instanceList.Count - 1];
        }

        public void Remove(T instance)
        {
            topInstance = default;
            instanceList.Remove(instance);

            if (instanceList.Count > 0)
                topInstance = instanceList[instanceList.Count - 1];
        }

        /// <summary>
        /// Get topInstance or defaultInst 
        /// </summary>
        /// <param name="defaultInst"> topInstance not exist use defaultInst </param>
        /// <returns></returns>
        public T GetTopInstanceOrDefault(T defaultInst)
            => topInstance ?? defaultInst;

        /// <summary>
        /// Inst is used?
        /// </summary>
        /// <param name="inst"></param>
        /// <returns></returns>
        public bool IsUsed(T inst)
        {
            if (topInstance != null)
                return topInstance.Equals(inst);
            return true;
        }
        /// <summary>
        /// Update mono's enable,
        /// return false when inst not used
        /// </summary>
        /// <param name="inst"></param>
        /// <returns></returns>
        public bool UpdateMonoEnable(T inst)
        {
            return inst.enabled = IsUsed(inst);
        }

        /// <summary>
        /// Update mono's enable first,
        /// when enable is true invoke action
        /// </summary>
        /// <param name="inst"></param>
        /// <param name="action"></param>
        public void UpdateMonoEnable(T inst, Action action)
        {
            if (UpdateMonoEnable(inst))
                action?.Invoke();
        }

        public List<T> InstanceList
            => instanceList;
    }
}
