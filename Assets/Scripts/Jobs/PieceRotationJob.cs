
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// May be a better way to do this without a buffer that I'm not thinking of
// I want to be able to account for multiple active pieces on the board
[BurstCompile]
[RequireComponentTag(typeof(PieceTiles), typeof(ActivePiece))]
struct PieceRotationJob : IJobForEachWithEntity<Translation>
{
    [ReadOnly]
    public NativeArray<Entity> board;
    [ReadOnly]
    public BufferFromEntity<PieceTiles> tilesLookup;
    [DeallocateOnJobCompletion]
    [NativeDisableParallelForRestriction]
    public NativeArray<float3> posBuffer;

    public int inputRot;

    public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation)
    {
        var rotation = quaternion.RotateZ(math.radians(90f * inputRot));

        var piecePos = translation.Value;

        var tilesBuffer = tilesLookup[entity];
        for (int i = 0; i < tilesBuffer.Length; ++i)
        {
            float3 tilePos = tilesBuffer[i];
            float3 rotatedPos = math.rotate(rotation, tilePos);
            rotatedPos = BoardUtility.RoundedStep(rotatedPos, .5f);

            int3 cell = BoardUtility.ToCellPos(rotatedPos, piecePos);
            int idx = BoardUtility.IndexFromCellPos(cell);

            // Special case for rotation - we want to be able to rotate even it would 
            // cause tiles to go "Above" the board
            bool inBounds = cell.x >= 0 && cell.x < BoardUtility.BoardSize.x &&
            cell.y >= 0 && cell.y < BoardUtility.BoardSize.y + 5;

            if (!inBounds || (idx < board.Length && board[idx] != Entity.Null) )
            {
                //Debug.Log("Unable to rotate");
                //Debug.LogFormat("TilePos {0}, Rotated {1}, CellPos {2}, Index {3}", tilePos, rotatedPos, newCellPos, idx);
                return;
            }

            posBuffer[(index * 4) + i] = rotatedPos;
        }

        for (int i = 0; i < 4; ++i)
            tilesBuffer[i] = posBuffer[(index * 4) + i];
    }
};
