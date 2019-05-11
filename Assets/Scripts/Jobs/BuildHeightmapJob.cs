
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[RequireComponentTag(typeof(Piece))]
[ExcludeComponent(typeof(ActivePieceState))]
struct BuildHeightmapJob : IJobForEachWithEntity<Translation>
{
    [NativeDisableParallelForRestriction]
    public NativeArray<int> heightMap;

    [ReadOnly]
    public BufferFromEntity<PieceTiles> tilesLookup;

    public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation)
    {
        var buffer = tilesLookup[entity];
        float3 piecePos = translation.Value;

        for( int i = 0; i < buffer.Length; ++i )
        {
            float3 tilePos = buffer[i];
            int3 cell = BoardUtility.ToCellPos(tilePos, piecePos);
            if( BoardUtility.InBounds(cell) )
            {
                heightMap[cell.x] = math.max(heightMap[cell.x], cell.y + 1);
            }
        }
    }
}
