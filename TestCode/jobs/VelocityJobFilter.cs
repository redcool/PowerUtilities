#if UNITY_COLLECTIONS
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct VelocityJobFilter : IJobFilter
{
    public NativeArray<Vector3> position, velocity;
    public float deltaTime;
    public bool Execute(int index)
    {
        position[index] += velocity[index] * deltaTime;
        return true;
    }


    public static void Test()
    {
        var position = new NativeArray<Vector3>(1000, Allocator.Persistent);
        var velocity = new NativeArray<Vector3>(1000, Allocator.Persistent);
        for (int i = 0; i < 1000; i++)
        {
            position[i] = Random.insideUnitSphere * 10;
            velocity[i] = new Vector3(0, 10, 0);
        }

        var job = new VelocityJobFilter
        {
            position = position,
            velocity = velocity,
            deltaTime = Time.deltaTime,
        };
        var results = new NativeList<int>(1000, Allocator.Persistent);
        var jobHandle = job.ScheduleFilter(results);

        jobHandle.Complete();

        Debug.Log("job for filter done");
        for (int i = 0; i < results.Length; i++)
        {
            Debug.Log(results[i]);
        }
        //Debug.Log(string.Join(" ",results.ToArray()));

        position.Dispose();
        results.Dispose();
        velocity.Dispose();
    }
}
#endif