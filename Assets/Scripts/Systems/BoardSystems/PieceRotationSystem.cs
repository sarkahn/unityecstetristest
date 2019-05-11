using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[DisableAutoCreation]
[UpdateAfter(typeof(GameLoopSystem))]
public class PieceRotationSystem : JobComponentSystem
{
    [BurstCompile]
    [RequireComponentTag(typeof(Child), typeof(ActivePiece))]
    struct PieceRotationSystemJob : IJobForEachWithEntity<PlayerInput, Piece, Translation, Rotation>
    {
        //[ReadOnly]
        //public NativeArray<BoardCell> board;

        [ReadOnly]
        public DynamicBuffer<BoardCell> boardBuffer;
        
        [ReadOnly]
        public BufferFromEntity<PieceTiles> tilesLookup;

        [DeallocateOnJobCompletion]
        public NativeArray<float3> posBuffer;
        
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
                    rotatedPos = BoardUtility.RoundedStep(rotatedPos, .5f);

                    int3 newCellPos = BoardUtility.ToCellPos(rotatedPos, piecePos);

                    //if (!BoardUtility.IsValidPosition(newCellPos, ref boardBuffer))
                    //    return;

                    posBuffer[i] = BoardUtility.RoundedStep(rotatedPos, .5f);
                }
                
                // If our rotated positions are valid we can assign them back
                // to our tiles buffer, which will be used in the movement system
                for( int i = 0; i < tilesBuffer.Length; ++i )
                {
                    tilesBuffer[i] = posBuffer[i];
                }
            }
        }



    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var boardEntity = GetSingletonEntity<Board>();
        var boardBuffer = EntityManager.GetBuffer<BoardCell>(boardEntity);
        //var board = boardBuffer.AsNativeArray();

        var job = new PieceRotationSystemJob
        {
            //board = board,
            boardBuffer = boardBuffer,
            tilesLookup = GetBufferFromEntity<PieceTiles>(false),
            posBuffer = new NativeArray<float3>(4, Allocator.TempJob),
        }.ScheduleSingle(this, inputDependencies);

        return job;
    }
    
}