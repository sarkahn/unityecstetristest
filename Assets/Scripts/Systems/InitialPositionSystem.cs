using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(InitializeBoardSystem))]
public class InitialPiecePositionsSystem : JobComponentSystem
{
    [BurstCompile]
    [RequireComponentTag(typeof(PieceTiles))]
    struct InitialPositionSystemJob : IJobForEachWithEntity<Piece>
    {

        public void Execute(Entity entity, int index, ref Piece piece)
        {
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new InitialPositionSystemJob
        {
        }.Schedule(this, inputDependencies);
        

        // Now that the job is set up, schedule it to be run. 
        return job;
    }
}