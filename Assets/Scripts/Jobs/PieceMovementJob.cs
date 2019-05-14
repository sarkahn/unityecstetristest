
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
        
        if( vel.x != 0 )
        {
            for (int i = 0; i < tilesBuffer.Length; ++i)
            {
                float3 tilePos = tilesBuffer[i];
                int3 cell = BoardUtility.ToCellPos(tilePos, piecePos);
                cell = cell + new int3(vel.x, 0, 0);
                int idx = BoardUtility.IndexFromCellPos(cell);
                bool inBounds = BoardUtility.InBounds(cell);

                if (cell.x < 0 || cell.x >= BoardUtility.BoardSize.x ||
                    (inBounds && board[idx] != Entity.Null))
                {
                    //Debug.LogFormat("Unable to move horizontally");
                    vel.x = 0;
                    break;
                }
            }
        }

        if( vel.y != 0 )
        {
            for (int i = 0; i < tilesBuffer.Length; ++i)
            {
                float3 tilePos = tilesBuffer[i];
                int3 cell = BoardUtility.ToCellPos(tilePos, piecePos);
                cell = cell + new int3(0, vel.y, 0);

                bool inBounds = BoardUtility.InBounds(cell);

                int idx = BoardUtility.IndexFromCellPos(cell);

                if (cell.y < 0 || (inBounds && board[idx] != Entity.Null))
                {
                    vel.y = 0;
                    //UnityEngine.Debug.Log("PLACING PIECE");
                    commandBuffer.RemoveComponent<ActivePiece>(index, entity);
                    break;
                }
            }
        }

        if (math.lengthsq(vel) != 0)
        {
            piecePos += vel;
            translation.Value = piecePos;
        }
    }
}