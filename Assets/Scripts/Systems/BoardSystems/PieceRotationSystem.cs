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
                //UnityEngine.Debug.Log("Positions for entity " + entity);
                //var childBuffer = childLookup[entity];
                var tilesBuffer = tilesLookup[entity];

                quaternion rotation = quaternion.RotateZ(math.radians(90f * input.rotation));

                // Test rotatations
                for ( int i = 0; i < tilesBuffer.Length; ++i )
                {
                    float3 tilePos = tilesBuffer[i];
                    float3 rotatedPos = math.rotate(rotation, tilePos);
                    int3 rotatedBoardPos = (int3)math.floor(rotatedPos + piecePos + piece.pivotOffset);

                    if (!IsValidPosition(rotatedBoardPos))
                        return;

                    posBuffer[i] = RoundedStep(rotatedPos, .5f);
                    //UnityEngine.Debug.LogFormat("Setting posBuffer {0} to {1}. Pre rotated: {2}", i, posBuffer[i], pos);
                }
                

                for( int i = 0; i < tilesBuffer.Length; ++i )
                {
                    tilesBuffer[i] = posBuffer[i];
                }
                
                //int angle = 90 * input.rotation;
                //var newRot = math.mul(math.normalize(rot.Value), rotation);
                //commandBuffer.SetComponent(entity, new Rotation() { Value = newRot });
            }
        }

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