using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct VelocityJob : IJob
{
    [ReadOnly]
    public NativeArray<Vector3> velocity;
    public NativeArray<Vector3> position;
    public float deltaTime;

    public void Execute()
    {
        for (int i = 0; i < position.Length; i++)
        {
            position[i] += velocity[i] * deltaTime;
        }
    }

    public static void Test()
    {
        var position = new NativeArray<Vector3>(500, Allocator.Persistent);
        var velocity = new NativeArray<Vector3>(500, Allocator.Persistent);
        for (int i = 0; i < velocity.Length; i++)
        {
            position[i] = Random.insideUnitSphere;
            velocity[i] = new Vector3(0, 10, 0);
        }
        var job = new VelocityJob
        {
            position = position,
            velocity = velocity,
            deltaTime = Time.deltaTime
        };
        var jobHandle = job.Schedule();
        jobHandle.Complete();

        Debug.Log(string.Join(" ", position));

        position.Dispose();
        velocity.Dispose();
    }
}
