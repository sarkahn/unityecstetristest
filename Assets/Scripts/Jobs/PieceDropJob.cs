
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[RequireComponentTag(typeof(PieceTiles), typeof(ActivePiece))]
struct PieceDropJob : IJobForEachWithEntity<Translation>
{
    [ReadOnly]
    public NativeArray<Entity> board;

    [ReadOnly]
    public NativeArray<int> heightMap;

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
        for( int i = 0; i < tilesBuffer.Length; ++i )
        {
            float3 tilePos = tilesBuffer[i];
            int3 cell = BoardUtility.ToCellPos(tilePos, piecePos);
            if( BoardUtility.InBounds(cell) )
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