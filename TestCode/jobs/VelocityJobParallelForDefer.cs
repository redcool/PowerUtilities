using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct VelocityJobParallelForDefer : IJobParallelForDefer
{
    public NativeArray<Vector3> velocity;
    public NativeArray<Vector3> position;
    public float deltaTime;
    public void Execute(int index)
    {
        position[index] = velocity[index] * deltaTime;
    }

    public static unsafe void Test()
    {
        var position = new NativeArray<Vector3>(1000, Allocator.Persistent);
        var velocity = new NativeArray<Vector3>(1000, Allocator.Persistent);

        for (int i = 0; i < velocity.Length; i++)
        {
            velocity[i] = new Vector3(0, 10, 0);
            position[i] = Random.insideUnitSphere * 10;
        }
        var job = new VelocityJobParallelForDefer
        {
            deltaTime = Time.deltaTime,
            position = position,
            velocity = velocity
        };
        // skip count
        var count = 100;
        var jobhandle = job.Schedule(&count, 64);
        jobhandle.Complete();

        Debug.Log("VelocityJobParallelForDefer 2 done");
        Debug.Log(string.Join(" ", position));

        position.Dispose();
        velocity.Dispose();
    }
}