using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(PieceMovementSystem))]
public class InitializeBoardSystem : JobComponentSystem
{
    [BurstCompile]
    [RequireComponentTag(typeof(Tile))]
    [ExcludeComponent(typeof(ActiveTile))]
    struct IntializeBoardSystemJob : IJobForEachWithEntity<Translation, Parent>
    {
        [NativeDisableParallelForRestriction]
        [WriteOnly]
        public NativeArray<BoardCell> board;

        [ReadOnly]
        public ComponentDataFromEntity<Translation> translationLookup;
        
        public void Execute(Entity entity, int index, 
            [ReadOnly] ref Translation translation, 
            [ReadOnly] ref Parent parent)
        {
            float3 piecePos = translationLookup[parent.Value].Value;
            int3 cell = BoardUtility.ToCellPos(translation.Value, piecePos);
            int idx = BoardUtility.IndexFromCellPos(cell);
            board[idx] = entity;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var boardEntity = GetSingletonEntity<Board>();
        var board = EntityManager.GetBuffer<BoardCell>(boardEntity).AsNativeArray();


        for (int i = 0; i < board.Length; ++i)
            board[i] = Entity.Null;
        
        var job = new IntializeBoardSystemJob
        {
            board = board,
            translationLookup = GetComponentDataFromEntity<Translation>(true),
        }.Schedule(this, inputDependencies);

        return job;
    }
    
}