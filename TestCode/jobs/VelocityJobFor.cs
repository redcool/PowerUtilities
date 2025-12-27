using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct VelocityJobFor : IJobFor
{
    [ReadOnly]
    public NativeArray<Vector3> velocity;
    public NativeArray<Vector3> position;

    public float deltaTime;
    public void Execute(int index)
    {
        position[index] += velocity[index] * deltaTime;
    }

    public static void Test()
    {
        var position = new NativeArray<Vector3>(1000, Allocator.Persistent);
        var velocity = new NativeArray<Vector3>(1000, Allocator.Persistent);

        for (int i = 0; i < velocity.Length; i++)
        {
            position[i] = Random.insideUnitSphere * 10f;
            velocity[i] = new Vector3(0, 10, 0);
        }
        var job = new VelocityJobFor
        {
            position = position,
            velocity = velocity,
            deltaTime = Time.deltaTime
        };
        job.Run(position.Length);

        Debug.Log("1 main thread done");
        Debug.Log(string.Join(" ", position));

        var idleJobHandle = new JobHandle();
        var jobForHandle = job.Schedule(position.Length, idleJobHandle);
        //jobForHandle.Complete();
        Debug.Log("2 job for done, read position,need jobForHandle.Complete");
        //Debug.Log(string.Join(" ", position));

        var jobParallelHandle = job.ScheduleParallel(position.Length, 64, jobForHandle);
        jobParallelHandle.Complete();

        Debug.Log("3 job parallel for done");
        Debug.Log(string.Join(" ", position));

        position.Dispose();
        velocity.Dispose();
    }
}

