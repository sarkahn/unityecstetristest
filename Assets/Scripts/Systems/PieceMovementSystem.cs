using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(InitializeBoardSystem))]
public class PieceMovementSystem : JobComponentSystem
{
    EndSimulationEntityCommandBufferSystem initCommandBufferSystem_;

    //[BurstCompile]
    [RequireComponentTag(typeof(PieceTiles), typeof(Piece), typeof(ActivePiece))]
    struct PieceMovementSystemJob : IJobForEachWithEntity<Translation>
    {
        //[ReadOnly]
        //public NativeArray<BoardCell> board;
        public DynamicBuffer<BoardCell> board;

        [ReadOnly]
        public BufferFromEntity<PieceTiles> tilesLookup;
        [ReadOnly]
        public EntityCommandBuffer.Concurrent commandBuffer;

        public int3 vel;

        public void Execute(Entity entity, int index, ref Translation translation)
        {
            var tiles = tilesLookup[entity];

            float3 piecePos = translation.Value;

            if( vel.x != 0 )
            {
                // Horizontal
                for (int i = 0; i < tiles.Length; ++i)
                {
                    var cell = BoardUtility.ToCellPos(tiles[i], piecePos);
                    cell.x += vel.x;

                    //if ( Collides(cell, board) || cell.x < 0 || cell.x >= BoardUtility.BoardSize.x )
                    {
                        vel.x = 0;
                        break;
                    }
                }
            }

            if( vel.y != 0 )
            {
                for( int i = 0; i < tiles.Length; ++i )
                {
                    var cell = BoardUtility.ToCellPos(tiles[i], piecePos);
                    cell.y += vel.y;

                    //if (Collides(cell, board) || cell.y < 0 || cell.y >= BoardUtility.BoardSize.y + 5)
                    {
                        vel.y = 0;
                        commandBuffer.RemoveComponent<ActivePiece>(index, entity);
                        break;
                    }
                }
            }

            if( math.lengthsq(vel) > 0 )
            {
                piecePos += vel;
                translation.Value = piecePos;
            }
        }

        bool Collides(int3 cell, NativeArray<BoardCell> board)
        {
            return BoardUtility.InBounds(cell) && board[BoardUtility.IndexFromCellPos(cell)] != Entity.Null;
        }
    }

    protected override void OnCreate()
    {
        initCommandBufferSystem_ = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = inputDependencies;

        int3 vel = int3.zero;
        vel.y = InputHandling.GetFallTimer() <= 0 ? -1 : 0;
        vel.x = InputHandling.GetHorizontalInput();

        if( math.lengthsq(vel) > 0 )
        {
            var boardEntity = GetSingletonEntity<Board>();
            var board = EntityManager.GetBuffer<BoardCell>(boardEntity);

            job = new PieceMovementSystemJob
            {
                vel = vel,
                tilesLookup = GetBufferFromEntity<PieceTiles>(true),
                board = board,
                commandBuffer = initCommandBufferSystem_.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, job);
        }

        // Now that the job is set up, schedule it to be run. 
        return job;
    }
}