using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(InitializeHeightmapSystem))]
public class PieceDropSystem : JobComponentSystem
{
    BeginInitializationEntityCommandBufferSystem initBufferSystem_;
    EntityQuery query_;

    [RequireComponentTag(typeof(PieceTiles), typeof(ActivePiece))]
    struct PieceDropSystemJob : IJobForEachWithEntity<Translation>
    {
        [ReadOnly]
        public NativeArray<HeightMapCell> heightMap;

        [ReadOnly]
        public BufferFromEntity<PieceTiles> tilesLookup;

        [ReadOnly]
        public EntityCommandBuffer.Concurrent commandBuffer;

        public void Execute(Entity entity, int index, ref Translation translation)
        {
            float3 piecePos = translation.Value;

            int shortestDistance = int.MaxValue;
            int3 touchingCell = int3.zero;

            var tilesBuffer = tilesLookup[entity];
            for (int i = 0; i < tilesBuffer.Length; ++i)
            {
                float3 tilePos = tilesBuffer[i];
                int3 cell = BoardUtility.ToCellPos(tilePos, piecePos);
                if (BoardUtility.InBounds(cell))
                {
                    int dist = cell.y - heightMap[cell.x];
                    shortestDistance = math.min(shortestDistance, dist);
                }
            }

            piecePos.y -= shortestDistance;

            translation.Value = piecePos;

            commandBuffer.RemoveComponent<ActivePiece>(index, entity);
        }
    }

    protected override void OnCreate()
    {
        initBufferSystem_ = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        query_ = GetEntityQuery(typeof(HeightMap), typeof(HeightMapCell));
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = inputDependencies;

        if( InputHandling.InstantDrop() )
        {
            var heightMapEntity = GetSingletonEntity<HeightMap>();
            var heightMap = EntityManager.GetBuffer<HeightMapCell>(heightMapEntity);

            job = new PieceDropSystemJob
            {
                commandBuffer = initBufferSystem_.CreateCommandBuffer().ToConcurrent(),
                tilesLookup = GetBufferFromEntity<PieceTiles>(true),
                heightMap = heightMap.AsNativeArray(),
            }.Schedule(this, job);
        }

        // Now that the job is set up, schedule it to be run. 
        return job;
    }
}