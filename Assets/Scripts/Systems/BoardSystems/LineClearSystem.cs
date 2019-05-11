using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[DisableAutoCreation]
[UpdateAfter(typeof(PieceMovementSystem))]
public class LineClearSystem : JobComponentSystem
{

    [BurstCompile]
    struct LineClearSystemJob : IJobForEach<Translation, Rotation>
    {

        public void Execute(ref Translation translation, [ReadOnly] ref Rotation rotation)
        {
            
        }
    }

    protected override void OnCreate()
    {
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new LineClearSystemJob
        {

        }.Schedule(this, inputDependencies);
        

        // Now that the job is set up, schedule it to be run. 
        return job;
    }

}