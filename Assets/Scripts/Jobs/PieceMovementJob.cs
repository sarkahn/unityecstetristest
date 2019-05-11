
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[RequireComponentTag(typeof(PieceTiles), typeof(ActivePiece))]
struct PieceMovementJob : IJobForEachWithEntity<Translation>
{
    [ReadOnly]
    public NativeArray<Entity> board;
    [ReadOnly]
    public BufferFromEntity<PieceTiles> tilesLookup;
    [ReadOnly]
    public EntityCommandBuffer.Concurrent commandBuffer;

    public int3 vel;

    public void Execute(Entity entity, int index, ref Translation translation)
    {
        var tilesBuffer = tilesLookup[entity];
        var piecePos = translation.Value;

        for (int i = 0; i < tilesBuffer.Length; ++i)
        {
            float3 tilePos = tilesBuffer[i];
            int3 cellPos = BoardUtility.ToCellPos(tilePos, piecePos);
            int3 horDest = cellPos + new int3(vel.x, 0, 0);
            int idx = BoardUtility.IndexFromCellPos(horDest);

            if (!BoardUtility.InBounds(horDest) || board[idx] != Entity.Null)
            {
                //Debug.LogFormat("Unable to move horizontally");
                vel.x = 0;
                break;
            }
        }

        for (int i = 0; i < tilesBuffer.Length; ++i)
        {
            float3 tilePos = tilesBuffer[i];
            int3 cell = BoardUtility.ToCellPos(tilePos, piecePos);
            int3 vertDest = cell + new int3(0, vel.y, 0);
            int idx = BoardUtility.IndexFromCellPos(vertDest);

            if (!BoardUtility.InBounds(vertDest) || board[idx] != Entity.Null)
            {
                vel.y = 0;
                commandBuffer.RemoveComponent<ActivePiece>(index, entity);
                break;
            }
        }

        if (math.lengthsq(vel) != 0)
        {
            piecePos += vel;
            translation.Value = piecePos;
        }
    }
}