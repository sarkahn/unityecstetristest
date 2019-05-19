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
    
    [RequireComponentTag(typeof(ActivePiece))]
    struct PieceMovementSystemJob : IJobForEachWithEntity<Piece>
    {
        [ReadOnly]
        public EntityCommandBuffer.Concurrent commandBuffer;
        
        // Tiles
        [ReadOnly]
        public BufferFromEntity<Child> childrenFromEntity;
        
        // Since we're only writing to children of the piece being iterated and
        // no pieces share children this is guaranteed safe, even
        // across chunks (even though we would likely never have enough
        // pieces to cross chunks)
        [NativeDisableParallelForRestriction]
        public ComponentDataFromEntity<Translation> posFromEntity;
        
        [NativeDisableParallelForRestriction]
        public NativeArray<BoardCell> board;

        [ReadOnly]
        public ComponentDataFromEntity<Parent> parentFromEntity;

        [ReadOnly]
        public ComponentDataFromEntity<ActiveTile> activeTileFromEntity;

        // Make sure we don't change velocity between iterations!
        // If we change this it changes the velocity for the next piece!
        [ReadOnly]
        public int3 inputVel;
        
        public void Execute(Entity entity, int index, [ReadOnly] ref Piece piece )
        {
            if (!childrenFromEntity.Exists(entity))
                return;

            var children = childrenFromEntity[entity];
            float3 piecePos = posFromEntity[entity].Value;
            float3 oldPiecePos = piecePos;

            var tilePositions = new NativeArray<float3>(children.Length, Allocator.Temp);

            int3 vel = inputVel;

            // Uncomment to disable gravity
            //vel.y = 0;
            
            for (int i = 0; i < children.Length; ++i)
            {
                var tilePos = posFromEntity[children[i].Value].Value;
                tilePositions[i] = tilePos;
            }
            
            // Piece movement gets processed one at a time - horizonal first, then
            // vertical. This guarantees tiles will never overlap no matter what

            // Check if horizontal movement is valid.
            if ( vel.x != 0 )
            {
                for (int i = 0; i < children.Length; ++i)
                {
                    var cell = (int3)math.floor(piecePos + tilePositions[i]);
                    cell.x += vel.x;

                    //Debug.LogFormat("Tile {0}, TargetCell {1}", children[i].Value, cell);
                    
                    if( cell.x < 0 || cell.x >= BoardUtility.BoardSize.x 
                     || !BoardSpaceIsClear(entity, cell) )
                    {
                        vel.x = 0;
                        break;
                    }
                }
            }

            piecePos.x += vel.x;

            // Check if vertical movement is valid
            if( vel.y != 0 )
            {
                for ( int i = 0; i < children.Length; ++i )
                {
                    int3 cell = (int3)math.floor(piecePos + tilePositions[i]);
                    cell.y += vel.y;
                    
                    // Check if we've hit the bottom of the board or if
                    // the tile in the cell we're dropping against is inactive
                    // If so we change this piece and it's tiles to inactive
                    if (cell.y < 0 || IsInactiveTile(cell))
                    {
                        commandBuffer.AddComponent(index, entity, new SpawnNextPiece());
                        commandBuffer.AddComponent(index, entity, new DroppedPiece());
                        commandBuffer.RemoveComponent<ActivePiece>(index, entity);
                        for (int j = 0; j < children.Length; ++j)
                            commandBuffer.RemoveComponent<ActiveTile>(index, children[j].Value);
                    }

                    // Account for other active pieces on the board
                    if( cell.y < 0 || !BoardSpaceIsClear(entity, cell))
                    {
                        vel.y = 0;

                        break;
                    }
                    
                }
            }

            piecePos.y += vel.y;

            if( math.lengthsq(vel) != 0 )
            {
                //Debug.LogFormat("Moving piece {0}. Velocity {1}. OldPiecePos {2}. NewPiecePos {3}", 
                //    entity, vel, oldPiecePos, piecePos);
                posFromEntity[entity] = new Translation { Value = piecePos };
                
                // Update board state
                for( int i = 0; i < children.Length; ++i )
                {
                    int3 oldCellPos = BoardUtility.CellFromWorldPos(oldPiecePos + tilePositions[i]);
                    int oldIdx = BoardUtility.IndexFromCellPos(oldCellPos);
                    if( BoardUtility.IndexInBounds(oldIdx))
                        board[oldIdx] = new BoardCell { value = Entity.Null };

                    //Debug.LogFormat("Nulling board at {0}", oldCellPos);
                }

                for( int i = 0; i < children.Length; ++i )
                {
                    int3 newCellPos = BoardUtility.CellFromWorldPos(piecePos + tilePositions[i]);
                    int newIdx = BoardUtility.IndexFromCellPos(newCellPos);
                    if (BoardUtility.IndexInBounds(newIdx))
                    {
                        board[newIdx] = new BoardCell { value = children[i].Value };
                        //Debug.LogFormat("Board after insertion at index {0} : {1}", newIdx, board[newIdx].value);
                    }

                }
            }
        }

        // In this case "out of bounds" == clear so bounds checks need to be made separately
        bool BoardSpaceIsClear(Entity self, int3 cell)
        {
            int idx = cell.y * BoardUtility.BoardSize.x + cell.x;
            return !(BoardUtility.IndexInBounds(idx)) || board[idx].value == Entity.Null || 
                parentFromEntity[board[idx].value].Value == self;
        }

        bool IsInactiveTile(int3 cell)
        {
            int idx = BoardUtility.IndexFromCellPos(cell);
            return BoardUtility.IndexInBounds(idx) && board[idx].value != Entity.Null 
                && !activeTileFromEntity.Exists(board[idx].value);
        }

    }

    protected override void OnCreate()
    {
        boardQuery_ = GetEntityQuery(typeof(BoardCell));
        initCommandBufferSystem_ = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = inputDependencies;

        var vel = InputHandling.GetVelocity();

        if (math.lengthsq(vel) != 0)
        {
            JobHandle getBoardJob;
            var board = boardQuery_.ToComponentDataArray<BoardCell>(Allocator.TempJob, out getBoardJob);

            job = new PieceMovementSystemJob
            {
                posFromEntity = GetComponentDataFromEntity<Translation>(false),
                childrenFromEntity = GetBufferFromEntity<Child>(true),
                board = board,
                inputVel = vel,
                commandBuffer = initCommandBufferSystem_.CreateCommandBuffer().ToConcurrent(),
                parentFromEntity = GetComponentDataFromEntity<Parent>(true),
                activeTileFromEntity = GetComponentDataFromEntity<ActiveTile>(true),
            }.Schedule(this, JobHandle.CombineDependencies(getBoardJob, job));

            initCommandBufferSystem_.AddJobHandleForProducer(job);

            job = new UpdateBoardJob
            {
                newValues = board,
                cellType = GetArchetypeChunkComponentType<BoardCell>(false),
            }.Schedule(boardQuery_, job);
        }
        
        return job;
    }
}