using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


[UpdateAfter(typeof(PieceRotationSystem))]
public class PieceMovementSystem : JobComponentSystem, IBoardSystem
{
    const float defaultFallDelay_ = 1f;
    float timer_ = defaultFallDelay_;

    IBoardSystem rotationSystem_;
    BeginInitializationEntityCommandBufferSystem initBufferSystem_;


    //[BurstCompile]
    [RequireComponentTag(typeof(ActivePiece))]
    struct Job : IJobForEachWithEntity<PlayerInput, Translation, Piece>
    {
        [ReadOnly]
        public NativeArray<bool> board;
        [ReadOnly]
        public BufferFromEntity<PieceTiles> tilesLookup;
        [ReadOnly]
        public EntityCommandBuffer.Concurrent commandBuffer;
        public int2 boardSize;
        public float gravityTimer;
        
        public void Execute(Entity entity, int index, [ReadOnly] ref PlayerInput input, ref Translation translation, [ReadOnly] ref Piece piece)
        {
            var piecePos = translation.Value;
            var tilesBuffer = tilesLookup[entity];

            int3 vel = new int3(input.movement, 0, 0);
            if (gravityTimer <= 0 )
                vel.y = -1;

            for( int i = 0; i < tilesBuffer.Length; ++i )
            {
                float3 tilePos = tilesBuffer[i].tilePos;
                int3 cell = (int3)math.floor(tilePos + piecePos + piece.pivotOffset);

                //UnityEngine.Debug.Log("TILEPOS: " + (cell + vel));

                // Horizontal check
                int3 horDest = cell + new int3(vel.x, 0, 0);
                if (!IsValidPosition(horDest))
                {
                    vel.x = 0;
                    break;
                }
            }

            for( int i = 0; i < tilesBuffer.Length; ++i )
            {
                float3 tilePos = tilesBuffer[i].tilePos;
                int3 cell = (int3)math.floor(tilePos + piecePos + piece.pivotOffset);

                // Vertical check
                int3 verDest = cell + new int3(0, vel.y, 0);
                if (!IsValidPosition(verDest))
                {
                    RemoveActive(index, entity);
                    vel.y = 0;
                    break;
                }
            }

            if(math.lengthsq(vel) != 0)
            {
                piecePos += vel;
                translation.Value = piecePos;
            }
        }

        // TODO : Move out to it's own class - used by Movement and Rotation systems
        bool IsValidPosition(int3 cell)
        {
            int idx = cell.y * boardSize.x + cell.x;

            bool inBounds = 
                cell.x >= 0 && cell.x < boardSize.x &&
                cell.y >= 0 && cell.y < boardSize.y;

            if (!inBounds || board[idx])
                return false;
            return true;
        }
        
        void RemoveActive( int jobIndex, Entity e)
        {
            commandBuffer.RemoveComponent(jobIndex, e, typeof(ActivePiece));
        }

    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        timer_ -= UnityEngine.Time.deltaTime;

        var boardJobs = GetBoardJobs();

        var job = new Job
        {
            board = GetBoard(),
            tilesLookup = GetBufferFromEntity<PieceTiles>(true),
            boardSize = GetBoardSize(),
            commandBuffer = initBufferSystem_.CreateCommandBuffer().ToConcurrent(),
            gravityTimer = timer_,
        }.Schedule(this, inputDependencies);

        boardJobs.Add(job);

        initBufferSystem_.AddJobHandleForProducer(job);

        job = JobHandle.CombineDependencies(boardJobs);

        if (timer_ <= 0)
            timer_ = defaultFallDelay_;
        
        return job;
    }

    protected override void OnCreate()
    {
        rotationSystem_ = World.GetOrCreateSystem<PieceRotationSystem>() as IBoardSystem;
        initBufferSystem_ = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    public NativeArray<bool> GetBoard()
    {
        return rotationSystem_.GetBoard();
    }

    public NativeList<JobHandle> GetBoardJobs()
    {
        return rotationSystem_.GetBoardJobs();
    }

    public int2 GetBoardSize()
    {
        return rotationSystem_.GetBoardSize();
    }
}