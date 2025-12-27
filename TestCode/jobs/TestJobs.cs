using PowerUtilities;
using Unity.Jobs;
using UnityEngine;


public class TestJobs : MonoBehaviour
{
    [EditorButton(onClickCall = nameof(OnClick))]
    public int isTest;

    void OnClick()
    {
        VelocityJob.Test();
        VelocityJobFor.Test();
        VelocityJobParallelFor.Test();
        //VelocityJobParallelForBatch.TestJobs();
        //VelocityJobParallelForDefer.TestJobs();
        //VelocityJobFilter.Test();
    }
}
