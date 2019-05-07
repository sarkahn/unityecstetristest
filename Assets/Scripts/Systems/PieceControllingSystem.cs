using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class PieceControllingSystem : JobComponentSystem
{
    [BurstCompile]
    [RequireComponentTag(typeof(ActivePiece))]
    struct PieceControllingSystemJob : IJobForEach<PlayerInput, Translation, Rotation>
    {
        // Add fields here that your job needs to do its work.
        // For example,
        //    public float deltaTime;
        public void Execute([ReadOnly] ref PlayerInput input, ref Translation pos, ref Rotation rot)
        {
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new PieceControllingSystemJob();
        
        // Now that the job is set up, schedule it to be run. 
        return job.Schedule(this, inputDependencies);
    }
}