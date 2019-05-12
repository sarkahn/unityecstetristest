
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

//[BurstCompile]
[RequireComponentTag(typeof(PieceTiles), typeof(Child))]
[ExcludeComponent(typeof(ActivePiece))]
struct InitializeBoardJob : IJobForEachWithEntity<Piece, Translation>
{
    [WriteOnly]
    [NativeDisableParallelForRestriction]
    public NativeArray<Entity> board;

    [ReadOnly]
    public BufferFromEntity<PieceTiles> tilesLookup;
    [ReadOnly]
    public BufferFromEntity<Child> childLookup;

    public void Execute(Entity entity, int index, [ReadOnly] ref Piece piece, [ReadOnly] ref Translation translation)
    {
        float3 piecePos = translation.Value;
        var tilesBuffer = tilesLookup[entity];
        var childBuffer = childLookup[entity];

        for (int i = 0; i < tilesBuffer.Length; ++i)
        {
            float3 tilePos = tilesBuffer[i];
            int3 cell = BoardUtility.ToCellPos(tilePos, piecePos);
            int idx = BoardUtility.IndexFromCellPos(cell);
            if (BoardUtility.InBounds(cell))
            {
                var child = childBuffer[i].Value;
                board[idx] = child;
            }
        }
    }
}
