using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct VelocityJobParallelFor : IJobParallelFor
{
    public NativeArray<Vector3> position;
    public NativeArray<Vector3> velocity;
    public float deltaTime;

    public void Execute(int index)
    {
        position[index] += velocity[index] * deltaTime;
    }

    public static void Test()
    {
        var position = new NativeArray<Vector3>(1000, Allocator.Persistent);
        var velocity = new NativeArray<Vector3>(1000, Allocator.Persistent);
        var job = new VelocityJobParallelFor
        {
            deltaTime = Time.deltaTime,
            position = position,
            velocity = velocity
        };
        var parallelHandle = job.Schedule(position.Length, 64);
        parallelHandle.Complete();
        Debug.Log("parallel for done");
        Debug.Log(string.Join(" ", position));
    }
}
