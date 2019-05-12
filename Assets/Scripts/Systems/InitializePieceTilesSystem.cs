using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

//[DisableAutoCreation]
[UpdateBefore(typeof(BoardSystem))]
public class InitializePieceTilesSystem : JobComponentSystem
{
    //[BurstCompile]
    [RequireComponentTag(typeof(PieceTiles), typeof(Child))]
    public struct Job : IJobForEachWithEntity<Piece>
    {
        [ReadOnly]
        public BufferFromEntity<Child> childLookup;
        [ReadOnly]
        public BufferFromEntity<PieceTiles> tilesLookup;
        [ReadOnly]
        public ComponentDataFromEntity<Translation> translationLookup;

        public void Execute(Entity entity, int index, ref Piece piece)
        {
            var childBuffer = childLookup[entity];
            var tilesBuffer = tilesLookup[entity];

            // Children may have been removed from line clears.
            while (tilesBuffer.Length != childBuffer.Length)
                tilesBuffer.RemoveAt(tilesBuffer.Length - 1);

            for( int i = 0; i < childBuffer.Length; ++i)
            {
                var child = childBuffer[i].Value;
                tilesBuffer[i] = translationLookup[child].Value;
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var jobHandle = new Job
        {
            childLookup = GetBufferFromEntity<Child>(true),
            tilesLookup = GetBufferFromEntity<PieceTiles>(false),
            translationLookup = GetComponentDataFromEntity<Translation>(false),
        }.Schedule(this, inputDependencies);
        
        return jobHandle;
    }

}
