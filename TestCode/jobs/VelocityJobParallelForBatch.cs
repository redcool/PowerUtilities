using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct VelocityJobParallelForBatch : IJobParallelForBatch
{
    public NativeArray<Vector3> position;
    public NativeArray<Vector3> velocity;
    public float deltaTime;

    public void Execute(int startIndex, int count)
    {
        for (int i = startIndex; i < startIndex + count; i++)
        {
            position[i] += velocity[i] * deltaTime;
        }
    }

    public static void Test()
    {
        var position = new NativeArray<Vector3>(1000, Allocator.Persistent);
        var velocity = new NativeArray<Vector3>(1000, Allocator.Persistent);
        for (int i = 0; i < velocity.Length; i++)
        {
            velocity[i] += new Vector3(0, 10, 0);
            position[i] = Random.insideUnitSphere * 10;
        }
        var job = new VelocityJobParallelForBatch
        {
            deltaTime = Time.deltaTime,
            position = position,
            velocity = velocity
        };
        var jobHandle = job.Schedule(position.Length, 64);
        jobHandle.Complete();
        Debug.Log("parallel for batch done");
        Debug.Log(string.Join(" ", position));
    }
}