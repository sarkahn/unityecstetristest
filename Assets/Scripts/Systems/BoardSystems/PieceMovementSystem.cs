using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


[DisableAutoCreation]
[UpdateAfter(typeof(PieceRotationSystem))]
public class PieceMovementSystem : JobComponentSystem
{ 
    const float defaultFallDelay_ = 1f;
    public float normalFallDelay_ = defaultFallDelay_;
    public const float fastFallDelay_ = .065f;

    float currentFallDelay_ = defaultFallDelay_;

    float timer_ = defaultFallDelay_;
    bool isFastFalling_ = false;

    
    BeginInitializationEntityCommandBufferSystem initBufferSystem_;


    //[BurstCompile]
    [RequireComponentTag(typeof(ActivePiece))]
    struct Job : IJobForEachWithEntity<PlayerInput, Translation, Piece>
    {
        [ReadOnly]
        public NativeArray<BoardCell> board;
        [ReadOnly]
        public BufferFromEntity<PieceTiles> tilesLookup;
        [ReadOnly]
        public EntityCommandBuffer.Concurrent commandBuffer;
        public int2 boardSize;
        public float gravityTimer;
        
        public void Execute(Entity entity, int index, 
            [ReadOnly] ref PlayerInput input, 
            ref Translation translation, 
            [ReadOnly] ref Piece piece)
        {
            var piecePos = translation.Value;
            var tilesBuffer = tilesLookup[entity];

            int3 vel = new int3(input.movement, 0, 0);
            if (gravityTimer <= 0 )
                vel.y = -1;

            for( int i = 0; i < tilesBuffer.Length; ++i )
            {
                float3 tilePos = tilesBuffer[i].tilePos;
                float3 worldPos = tilePos + piecePos - .5f;
                int3 cell = BoardUtility.ToCellPos(tilePos, piecePos);

                //UnityEngine.Debug.LogFormat("{0}: PiecePos {1}, TilePos {2}, TileWorldPos {3}, Cell {4}", 
                //  i, piecePos, tilePos, worldPos, cell);
                //UnityEngine.Debug.Log("TILEPOS: " + (cell + vel));

                // Horizontal check
                int3 horDest = cell + new int3(vel.x, 0, 0);
                //if (!BoardUtility.IsValidPosition(horDest, ref boardBuffer))
                {
                    vel.x = 0;
                    break;
                }
            }

            for( int i = 0; i < tilesBuffer.Length; ++i )
            {
                float3 tilePos = tilesBuffer[i].tilePos;
                int3 cell = BoardUtility.ToCellPos(tilePos, piecePos);

                // Vertical check
                int3 verDest = cell + new int3(0, vel.y, 0);
                //if (!BoardUtility.IsValidPosition(verDest, ref boardBuffer))
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

        
        void RemoveActive( int jobIndex, Entity e)
        {
            commandBuffer.RemoveComponent(jobIndex, e, typeof(ActivePiece));
        }

    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        bool fastFall = Input.GetAxisRaw("Vertical") == -1;

        if( fastFall && fastFallDelay_ < normalFallDelay_ &&  !isFastFalling_)
        {
            timer_ = math.remap(0, normalFallDelay_, 0, fastFallDelay_, timer_);
            currentFallDelay_ = fastFallDelay_;
            isFastFalling_ = true;
        }
        
        if( !fastFall && isFastFalling_ )
        {
            timer_ = math.remap(0, fastFallDelay_, 0, normalFallDelay_, timer_);
            currentFallDelay_ = normalFallDelay_;
            isFastFalling_ = false;
        }

        timer_ -= UnityEngine.Time.deltaTime;


        var job = new Job
        {
            board = new NativeArray<BoardCell>(0, Allocator.Persistent),
            tilesLookup = GetBufferFromEntity<PieceTiles>(true),
            boardSize = new int2(),
            commandBuffer = initBufferSystem_.CreateCommandBuffer().ToConcurrent(),
            gravityTimer = timer_,
        }.Schedule(this, inputDependencies);
        

        initBufferSystem_.AddJobHandleForProducer(job);
        

        if (timer_ <= 0)
            timer_ = currentFallDelay_;
        
        return job;
    }

    protected override void OnCreate()
    {
        //rotationSystem_ = World.GetOrCreateSystem<PieceRotationSystem>() as IBoardSystem;
        //initBufferSystem_ = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }
    
}