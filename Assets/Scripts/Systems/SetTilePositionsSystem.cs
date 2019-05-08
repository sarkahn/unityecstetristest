using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Jobs;

[UpdateAfter(typeof(PieceMovementSystem))]
public class SetTilePositionsSystem : JobComponentSystem
{
    [BurstCompile]
    [RequireComponentTag(typeof(Child))]
    [ExcludeComponent(typeof(QueuedPiece))]
    struct SetTilePositionsSystemJob : IJobForEachWithEntity<Piece>
    {
        [ReadOnly]
        public BufferFromEntity<Child> childBufferLookup;
        [ReadOnly]
        public BufferFromEntity<PieceTiles> tilesBufferLookup;

        public ComponentDataFromEntity<Translation> translationLookup;

        public void Execute(Entity entity, int index, [ReadOnly] ref Piece piece)
        {
            var childBuffer = childBufferLookup[entity];
            var tilesBuffer = tilesBufferLookup[entity];

            for( int i = 0; i < childBuffer.Length; ++i )
            {
                var child = childBuffer[i].Value;
                translationLookup[child] = new Translation { Value = tilesBuffer[i] };

            }
        }
        
    }

    protected override void OnCreate()
    {
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new SetTilePositionsSystemJob
        {
            childBufferLookup = GetBufferFromEntity<Child>(true),
            tilesBufferLookup = GetBufferFromEntity<PieceTiles>(true),
            translationLookup = GetComponentDataFromEntity<Translation>(false),
        }.ScheduleSingle(this, inputDependencies);
        

        // Now that the job is set up, schedule it to be run. 
        return job;
    }
}