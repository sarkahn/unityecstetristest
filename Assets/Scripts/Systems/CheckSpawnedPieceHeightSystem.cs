using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

//[DisableAutoCreation]
public class CheckSpawnedPieceHeightSystem : JobComponentSystem
{
    EntityQuery heightMapQuery_;
    EndSimulationEntityCommandBufferSystem initBufferSystem_;
    
    [RequireComponentTag(typeof(SpawnedPiece))]
    struct CheckSpawnedPieceHeightSystemJob : IJobForEachWithEntity<Piece>
    {
        [ReadOnly]
        [DeallocateOnJobCompletion]
        public NativeArray<HeightmapCell> heightMap;

        [ReadOnly]
        public EntityCommandBuffer.Concurrent commandBuffer;

        [ReadOnly]
        public ComponentDataFromEntity<Translation> posFromEntity;
        [ReadOnly]
        public BufferFromEntity<Child> tilesFromEntity;

        public void Execute(Entity entity, int index, ref Piece piece)
        {
            commandBuffer.RemoveComponent(index, entity, typeof(SpawnedPiece));
            float3 piecePos = posFromEntity[entity].Value;
            var tiles = tilesFromEntity[entity];
            for( int i = 0; i < tiles.Length; ++i )
            {
                var tilePos = posFromEntity[tiles[i].Value].Value;
                int3 cell = BoardUtility.CellFromWorldPos(tilePos + piecePos);

                if (cell.x < 0 || cell.x >= heightMap.Length)
                    continue;

                if (cell.y <= heightMap[cell.x].value)
                {
                    //Debug.LogFormat("Cell {0} one piece {1} is higher than heightmap {2}", cell, entity, heightMap[cell.x].value);
                    commandBuffer.AddComponent(index, entity, new SnapToHeightmap());
                    return;
                }
            }
        }
    }

    protected override void OnCreate()
    {
        heightMapQuery_ = GetEntityQuery(ComponentType.ReadOnly<HeightmapCell>());
        initBufferSystem_ = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var heightMap = heightMapQuery_.ToComponentDataArray<HeightmapCell>(Allocator.TempJob);

        var job = inputDependencies;

        job = new CheckSpawnedPieceHeightSystemJob
        {
            heightMap = heightMap,
            commandBuffer = initBufferSystem_.CreateCommandBuffer().ToConcurrent(),
            posFromEntity = GetComponentDataFromEntity<Translation>(true),
            tilesFromEntity = GetBufferFromEntity<Child>(true),
        }.Schedule(this, job);

        initBufferSystem_.AddJobHandleForProducer(job);

        return job;
    }
}