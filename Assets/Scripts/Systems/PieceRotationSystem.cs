using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[DisableAutoCreation]
[UpdateAfter(typeof(BoardSystem))]
public class PieceRotationSystem : JobComponentSystem
{
    NativeArray<bool> board_;
    JobHandle boardJob_;
    BeginInitializationEntityCommandBufferSystem initBufferSystem_;
    
    [BurstCompile]
    [RequireComponentTag(typeof(Piece),typeof(Child))]
    struct PieceRotationSystemJob : IJobForEachWithEntity<PlayerInput, Rotation>
    {
        [ReadOnly]
        public NativeArray<bool> board;
        [ReadOnly]
        public BufferFromEntity<Child> childLookup;
        [ReadOnly]
        public ComponentDataFromEntity<Translation> posLookup;

        [DeallocateOnJobCompletion]
        [NativeDisableParallelForRestriction]
        public NativeArray<float3> tilePositions;

        [ReadOnly]
        public EntityCommandBuffer commandBuffer;
        
        public void Execute(Entity entity, int index, [ReadOnly] ref PlayerInput input, ref Rotation rot )
        {
            if( input.rotation != 0 )
            {
                //UnityEngine.Debug.Log("Positions for entity " + entity);
                var childBuffer = childLookup[entity];
                for( int i = 0; i < childBuffer.Length && i < tilePositions.Length; ++i )
                {
                    var tile = childBuffer[i].Value;
                    tilePositions[i] = posLookup[tile].Value;
                    //UnityEngine.Debug.Log("Child " + tile + " pos: " + posLookup[tile].Value);
                }

                quaternion rotation = quaternion.RotateZ(math.radians(90f * input.rotation));

                bool coll = false;
                for ( int i = 0; i < tilePositions.Length; ++i )
                {
                    tilePositions[i] = math.rotate(rotation, tilePositions[i]);
                    tilePositions[i] = RoundedStep(tilePositions[i], 0.5f);
                    //if( )
                }
                
                int angle = 90 * input.rotation;
                rot.Value = math.mul(math.normalize(rot.Value), quaternion.AxisAngle(new float3(0, 0, 1), math.radians(angle)));
            }
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
        initBufferSystem_ = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }


    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var boardSystem = World.GetExistingSystem<BoardSystem>();

        var board = boardSystem.board_;
        
        var job = new PieceRotationSystemJob
        {
            board = board,
            childLookup = GetBufferFromEntity<Child>(true),
            tilePositions = new NativeArray<float3>(4, Allocator.TempJob),
            commandBuffer = initBufferSystem_.CreateCommandBuffer(),
            posLookup = GetComponentDataFromEntity<Translation>(false),
        };

        var query = GetEntityQuery(typeof(Child));
        


        var combinedJob = JobHandle.CombineDependencies(inputDependencies, boardSystem.boardJob_);

        boardJob_ = job.Schedule(this, combinedJob);

        initBufferSystem_.AddJobHandleForProducer(boardJob_);

        // Now that the job is set up, schedule it to be run. 
        return boardJob_;
    }
}