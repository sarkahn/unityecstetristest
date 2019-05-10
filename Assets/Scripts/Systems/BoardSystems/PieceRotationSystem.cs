using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(InitializeBoardSystem))]
public class PieceRotationSystem : JobComponentSystem, IBoardSystem
{
    //BeginInitializationEntityCommandBufferSystem initBufferSystem_;
    IBoardSystem initBoardSystem_;
    
    //[BurstCompile]
    [RequireComponentTag(typeof(Child), typeof(ActivePiece))]
    struct PieceRotationSystemJob : IJobForEachWithEntity<PlayerInput, Piece, Translation, Rotation>
    {
        [ReadOnly]
        public NativeArray<bool> board;

       //[ReadOnly]
       // public BufferFromEntity<Child> childLookup;
        
        //[ReadOnly]
        //public ComponentDataFromEntity<Translation> posLookup;

        //[ReadOnly]
        //public EntityCommandBuffer.Concurrent commandBuffer;

        [ReadOnly]
        public BufferFromEntity<PieceTiles> tilesLookup;

        [DeallocateOnJobCompletion]
        public NativeArray<float3> posBuffer;

        public int2 boardSize;
        
        public void Execute(Entity entity, int index, 
            [ReadOnly] ref PlayerInput input, 
            [ReadOnly] ref Piece piece, 
            [ReadOnly] ref Translation translation, 
            ref Rotation rot )
        {
            if( input.rotation != 0 )
            {
                float3 piecePos = translation.Value;
                var tilesBuffer = tilesLookup[entity];

                quaternion rotation = quaternion.RotateZ(math.radians(90f * input.rotation));
                
                for ( int i = 0; i < tilesBuffer.Length; ++i )
                {
                    float3 tilePos = tilesBuffer[i];
                    float3 rotatedPos = math.rotate(rotation, tilePos);
                    rotatedPos = RoundedStep(rotatedPos, .5f);

                    int3 newCellPos = BoardUtility.ToCellPos(rotatedPos, piecePos);

                    if (!BoardUtility.IsValidPosition(newCellPos, boardSize, ref board))
                        return;

                    posBuffer[i] = RoundedStep(rotatedPos, .5f);
                }
                
                // If our rotated positions are valid we can assign them back
                // to our tiles buffer, which will be used in the movement system
                for( int i = 0; i < tilesBuffer.Length; ++i )
                {
                    tilesBuffer[i] = posBuffer[i];
                }
            }
        }

        // Round each component to the nearest given step
        float3 RoundedStep(float3 val, float step)
        {
            step = math.max(step, 0.01f);
            val = math.round(val / step) * step;
            return val;
        }


    }

    protected override void OnCreate()
    {
        //initBufferSystem_ = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        initBoardSystem_ = World.GetOrCreateSystem<InitializeBoardSystem>() as IBoardSystem;
    }


    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new PieceRotationSystemJob
        {
            board = GetBoard(),
            tilesLookup = GetBufferFromEntity<PieceTiles>(false),
            //commandBuffer = initBufferSystem_.CreateCommandBuffer().ToConcurrent(),
            boardSize = GetBoardSize(),
            posBuffer = new NativeArray<float3>(4, Allocator.TempJob),
        }.ScheduleSingle(this, inputDependencies);

        //initBufferSystem_.AddJobHandleForProducer(job);

        var boardJobs = GetBoardJobs();

        boardJobs.Add(job);

        return job;
    }

    public NativeArray<bool> GetBoard()
    {
        return initBoardSystem_.GetBoard();
    }
    public NativeList<JobHandle> GetBoardJobs()
    {
        return initBoardSystem_.GetBoardJobs();
    }

    public int2 GetBoardSize()
    {
        return initBoardSystem_.GetBoardSize();
    }
}