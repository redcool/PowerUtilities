using PowerUtilities;
using Unity.Jobs;
using UnityEngine;


public class TestJobs : MonoBehaviour
{
    [EditorButton(onClickCall = nameof(OnClick))]
    public int isTest;

    void OnClick()
    {
        //VelocityJob.TestJobs();
        //VelocityJobFor.TestJobs();
        //VelocityJobParallelFor.TestJobs();
        //VelocityJobParallelForBatch.TestJobs();
        //VelocityJobParallelForDefer.TestJobs();
        VelocityJobFilter.Test();
    }
}
