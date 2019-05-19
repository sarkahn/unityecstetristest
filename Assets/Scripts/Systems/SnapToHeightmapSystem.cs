using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

//[DisableAutoCreation]
// Set the correct height for newly spawned pieces
[UpdateInGroup(typeof(TransformSystemGroup))]
[UpdateBefore(typeof(EndFrameParentSystem))]
public class SnapToHeightmapSystem : JobComponentSystem
{
    BeginInitializationEntityCommandBufferSystem initBufferSystem_;
    EntityQuery heightMapQuery_;
    
    [RequireComponentTag(typeof(SnapToHeightmap), typeof(Child))]
    struct SnapPiecesToHeightmap : IJobForEachWithEntity<Piece>
    {
        [ReadOnly]
        public EntityCommandBuffer.Concurrent commandBuffer;
        
        [ReadOnly]
        [DeallocateOnJobCompletion]
        public NativeArray<HeightmapCell> heightMap;
        
        [NativeDisableParallelForRestriction]
        public ComponentDataFromEntity<Translation> posFromEntity;

        [ReadOnly]
        public ComponentDataFromEntity<ActivePiece> activePieceFromEntity;

        [ReadOnly]
        public BufferFromEntity<Child> tilesFromEntity;

        public void Execute(Entity entity, int index, ref Piece piece)
        {
            commandBuffer.RemoveComponent<SnapToHeightmap>(index, entity);

            var children = tilesFromEntity[entity];

            if ( activePieceFromEntity.Exists(entity) )
            {
                commandBuffer.AddComponent(index, entity, new SpawnNextPiece());
                commandBuffer.AddComponent(index, entity, new DroppedPiece());
                commandBuffer.RemoveComponent<ActivePiece>(index, entity);

                for (int j = 0; j < children.Length; ++j)
                    commandBuffer.RemoveComponent<ActiveTile>(index, children[j].Value);
            }
            
            float3 piecePos = posFromEntity[entity].Value;

            int highestPoint = -99;
            int lowestCell = 99;

            for( int i = 0; i < children.Length; ++i )
            {
                float3 tilePos = posFromEntity[children[i].Value].Value;
                int3 cell = BoardUtility.CellFromWorldPos(tilePos + piecePos);

                if (cell.x < 0 || cell.x >= heightMap.Length)
                    continue;

                highestPoint = math.max(highestPoint, heightMap[cell.x]);
                lowestCell = math.min(lowestCell, cell.y);
            }
            //Debug.LogFormat("Snapping {0} to heightmap, offset {1}", entity, offset);

            piecePos.y += (highestPoint - lowestCell);
            posFromEntity[entity] = new Translation { Value = piecePos };
        }
    }

    protected override void OnCreate()
    {
        initBufferSystem_ = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        
        heightMapQuery_ = GetEntityQuery(typeof(HeightmapCell));
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = inputDeps;
        
        // Get our heightmap
        var heightMap = heightMapQuery_.ToComponentDataArray<HeightmapCell>(Allocator.TempJob);
            
        job = new SnapPiecesToHeightmap
        {
            commandBuffer = initBufferSystem_.CreateCommandBuffer().ToConcurrent(),
            heightMap = heightMap,
            tilesFromEntity = GetBufferFromEntity<Child>(true),
            posFromEntity = GetComponentDataFromEntity<Translation>(false),
            activePieceFromEntity = GetComponentDataFromEntity<ActivePiece>(true),
        }.Schedule(this, job);

        return job;
    }
}