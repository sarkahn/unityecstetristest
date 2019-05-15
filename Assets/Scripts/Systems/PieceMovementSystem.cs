using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(PieceRotationSystem))]
public class PieceMovementSystem : JobComponentSystem
{
    EntityQuery boardQuery_;
    BeginInitializationEntityCommandBufferSystem initCommandBufferSystem_;

    //[BurstCompile]
    [RequireComponentTag(typeof(ActivePiece), typeof(Child))]
    struct PieceMovementSystemJob : IJobForEachWithEntity<Translation>
    {
        [ReadOnly]
        public EntityCommandBuffer.Concurrent commandBuffer;
        
        // Tiles
        [ReadOnly]
        public BufferFromEntity<Child> childrenFromEntity;

        // Tile world positions
        [ReadOnly]
        public ComponentDataFromEntity<LocalToWorld> ltwFromEntity;
        
        [ReadOnly]
        [DeallocateOnJobCompletion]
        public NativeArray<BoardCell> board;

        [ReadOnly]
        public int3 inputVel;
        
        public void Execute(Entity entity, int index, ref Translation translation )
        {
            var children = childrenFromEntity[entity];

            var posBuffer = new NativeArray<float3>(children.Length, Allocator.Temp);

            int3 vel = inputVel;

            for (int i = 0; i < children.Length; ++i)
            {
                var tilePos = ltwFromEntity[children[i].Value].Position;
                posBuffer[i] = tilePos;
            }

            if ( vel.x != 0 )
            {
                for (int i = 0; i < posBuffer.Length; ++i)
                {
                    var cell = (int3)math.floor(posBuffer[i]);
                    cell.x += vel.x;
                    
                    if( cell.x < 0 || cell.x >= BoardUtility.BoardSize.x 
                     || !BoardSpaceIsClear(cell) )
                    {
                        vel.x = 0;
                        break;
                    }
                }
            }

            if( vel.y != 0 )
            {
                for( int i = 0; i < children.Length; ++i )
                {
                    int3 cell = (int3)math.floor(posBuffer[i]);
                    cell.y += vel.y;

                    if( cell.y < 0 || !BoardSpaceIsClear(cell))
                    {
                        commandBuffer.RemoveComponent<ActivePiece>(index, entity);
                        vel.y = 0;
                        break;
                    }
                    
                }
            }

            if( math.lengthsq(vel) != 0 )
            {
                //Debug.Log("(Should be) moving piece");
                translation.Value += vel;
            }
        }

        // In this case "out of bounds" == clear so bounds checks need to be made separately
        bool BoardSpaceIsClear(int3 cell)
        {
            int idx = cell.y * BoardUtility.BoardSize.x + cell.x;
            return idx < 0 || idx >= board.Length || board[idx] == Entity.Null;
        }
    }

    protected override void OnCreate()
    {
        boardQuery_ = GetEntityQuery(ComponentType.ReadOnly<BoardCell>());
        initCommandBufferSystem_ = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = inputDependencies;

        var vel = InputHandling.GetVelocity();

        if( math.lengthsq(vel) != 0 )
        {
            JobHandle getBoardJob;
            var board = boardQuery_.ToComponentDataArray<BoardCell>(Allocator.TempJob, out getBoardJob);

            //Debug.Log("RUNNING MOVE JOB");
            job = new PieceMovementSystemJob
            {
                ltwFromEntity = GetComponentDataFromEntity<LocalToWorld>(true),
                childrenFromEntity = GetBufferFromEntity<Child>(true),
                board = board,
                inputVel = vel,
                commandBuffer = initCommandBufferSystem_.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, JobHandle.CombineDependencies(getBoardJob, job));
        }
        
        return job;
    }
}