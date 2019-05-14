using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[UpdateBefore(typeof(PieceMovementSystem))]
public class InitializeHeightmapSystem : JobComponentSystem
{
    EntityQuery query_;

    [BurstCompile]
    [RequireComponentTag(typeof(Piece))]
    [ExcludeComponent(typeof(ActivePieceState))]
    struct BuildHeightmapJob : IJobForEachWithEntity<Translation>
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<HeightMapCell> heightMap;

        [ReadOnly]
        public BufferFromEntity<PieceTiles> tilesLookup;

        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation)
        {
            var buffer = tilesLookup[entity];
            float3 piecePos = translation.Value;

            for (int i = 0; i < buffer.Length; ++i)
            {
                float3 tilePos = buffer[i];
                int3 cell = BoardUtility.ToCellPos(tilePos, piecePos);
                if (BoardUtility.InBounds(cell))
                {
                    heightMap[cell.x] = math.max(heightMap[cell.x], cell.y + 1);
                }
            }
        }
    }

    protected override void OnCreate()
    {
        query_ = GetEntityQuery(typeof(HeightMap), typeof(HeightMapCell));
    }


    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var heightMapEntity = GetSingletonEntity<HeightMap>();
        var heightMap = EntityManager.GetBuffer<HeightMapCell>(heightMapEntity);

        var job = new BuildHeightmapJob
        {
            tilesLookup = GetBufferFromEntity<PieceTiles>(true),
            heightMap = heightMap.AsNativeArray(),
        }.Schedule(this, inputDependencies);
        
        return job;
    }
    
}