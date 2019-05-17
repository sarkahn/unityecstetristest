using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(LineClearSystem))]
public class GameOverCheckSystem : JobComponentSystem
{
    EntityQuery boardQuery_;
    BeginInitializationEntityCommandBufferSystem initCommandBuffer_;
    
    [ExcludeComponent(typeof(ActivePiece))]
    [RequireComponentTag(typeof(DroppedPiece), typeof(Translation), typeof(Child))]
    struct GameOverCheckSystemJob : IJobForEachWithEntity<Piece>
    {
        [ReadOnly]
        public ComponentDataFromEntity<Translation> posFromEntity;

        [ReadOnly]
        public BufferFromEntity<Child> tilesFromEntity;

        [ReadOnly]
        [DeallocateOnJobCompletion]
        public NativeArray<BoardCell> board;

        [ReadOnly]
        public EntityCommandBuffer.Concurrent commandBuffer;
        
        public void Execute(Entity entity, int index, [ReadOnly] ref Piece piece)
        {
            var piecePos = posFromEntity[entity].Value;

            var tiles = tilesFromEntity[entity];
            for( int i = 0; i < tiles.Length; ++i )
            {
                var tile = tiles[i].Value;
                
                // Somehow we are ending up with tiles with no translation (probably null)
                // I'm guessing from LineClearSystem but I can't seem to figure out why.
                // LineClearSystem.UpdatePieceChildrenJob is supposed to remove
                // any null children, since it's using BufferFromEntity to do it
                // I thought the ECS system would automatically know it has to wait
                // for that job to finish before it runs this one...but it isn't? At
                // least I think that's what's happening. We can work around it by
                // checking if it exists first but I would like to know what I'm
                // misunderstanding.
                if(posFromEntity.Exists(tile))
                {
                    var tilePos = posFromEntity[tile].Value;
                    int3 cell = BoardUtility.CellFromWorldPos(piecePos + tilePos);

                    if (cell.y >= BoardUtility.BoardSize.y)
                    {
                        commandBuffer.AddComponent<GameOver>(index, entity, new GameOver());
                    }
                }
            }
        }
    }

    protected override void OnCreate()
    {
        boardQuery_ = GetEntityQuery(ComponentType.ReadOnly<BoardCell>());
        initCommandBuffer_ = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var board = boardQuery_.ToComponentDataArray<BoardCell>(Allocator.TempJob);

        var job = new GameOverCheckSystemJob
        {
            board = board,
            posFromEntity = GetComponentDataFromEntity<Translation>(true),
            tilesFromEntity = GetBufferFromEntity<Child>(true),
            commandBuffer = initCommandBuffer_.CreateCommandBuffer().ToConcurrent(),

        }.Schedule(this, inputDependencies);

        initCommandBuffer_.AddJobHandleForProducer(job);

        return job;
    }
}