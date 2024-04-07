using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    /// <summary>
    /// for Manager objects(like monoBehaviour instance)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InstanceManager<T>
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
        public T GetTopInstance(T defaultInst)
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

        public List<T> InstanceList
            => instanceList;
    }
}
