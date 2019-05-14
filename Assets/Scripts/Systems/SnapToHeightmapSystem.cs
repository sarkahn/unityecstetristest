using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

//[DisableAutoCreation]
// Set the correct height for newly spawned pieces
[UpdateAfter(typeof(InitHeightMapSystem))]
public class OnPieceSpawnedSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem initBufferSystem_;
    EntityQuery snapQuery_;
    EntityQuery heightMapQuery_;
    
    [RequireComponentTag(typeof(Piece), typeof(ActivePiece), typeof(SnapToHeightmap), typeof(Child))]
    struct SnapPiecesToHeightmap : IJobForEachWithEntity<Translation>
    {
        [ReadOnly]
        public EntityCommandBuffer.Concurrent commandBuffer;
        
        [ReadOnly]
        [DeallocateOnJobCompletion]
        [NativeDisableParallelForRestriction]
        public NativeArray<HeightmapCell> heightMap;

        [ReadOnly]
        public BufferFromEntity<Child> childrenFromEntity;
        [ReadOnly]
        public ComponentDataFromEntity<LocalToWorld> ltwFromEntity;

        public void Execute(Entity entity, int index, ref Translation t)
        {
            commandBuffer.RemoveComponent<SnapToHeightmap>(index, entity);

            int offset = 0;

            var children = childrenFromEntity[entity];
            for( int i = 0; i < children.Length; ++i )
            {
                var childLTW = ltwFromEntity[children[i].Value];
                int3 cell = (int3)math.floor(childLTW.Position);
                if (cell.x < 0 || cell.x >= heightMap.Length)
                    continue;
                //Debug.LogFormat("Tile {0} Cell {1}", positionIndex, cell);
                offset = math.max(offset, heightMap[cell.x] - cell.y);
            }

            var p = t.Value;
            p.y += offset;
            t.Value  = p;
        }
    }

    protected override void OnCreate()
    {
        initBufferSystem_ = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        
        heightMapQuery_ = GetEntityQuery(typeof(HeightmapCell));
        snapQuery_ = GetEntityQuery(typeof(SnapToHeightmap), typeof(ActivePiece));
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = inputDeps;

        if(snapQuery_.CalculateLength() != 0 )
        {
            // Get our heightmap
            var heightMap = heightMapQuery_.ToComponentDataArray<HeightmapCell>(Allocator.TempJob);
            
            job = new SnapPiecesToHeightmap
            {
                commandBuffer = initBufferSystem_.CreateCommandBuffer().ToConcurrent(),
                heightMap = heightMap,
                childrenFromEntity = GetBufferFromEntity<Child>(true),
                ltwFromEntity = GetComponentDataFromEntity<LocalToWorld>(true),
            }.Schedule(this, job);
        }

        return job;
    }
}