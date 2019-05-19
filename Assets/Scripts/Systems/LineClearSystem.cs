using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(PieceMovementSystem))]
public class LineClearSystem : JobComponentSystem
{
    EntityQuery boardQuery_;
    BeginInitializationEntityCommandBufferSystem initCommmandBufferSystem_;

    public struct LineClearSystemJob : IJobChunk
    {
        [ReadOnly]
        public EntityCommandBuffer.Concurrent commandBuffer;

        [ReadOnly]
        public ComponentDataFromEntity<Parent> parentFromEntity;

        [ReadOnly]
        public ComponentDataFromEntity<ActiveTile> activeTileFromEntity;

        // If our board was big enough to cross chunks this could be bad
        [NativeDisableParallelForRestriction]
        public NativeArray<bool> linesCleared;
        
        public ArchetypeChunkComponentType<BoardCell> cellType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            //Debug.Log("Checking lines");
            var cells = chunk.GetNativeArray(cellType);

            //if( cells[0] != Entity.Null)
            //    Debug.LogFormat("State of 0,0:", cells[0]);

            for ( int y = 0; y < BoardUtility.BoardSize.y; ++y )
            {
                bool lineFull = true;
                for( int x = 0; x < BoardUtility.BoardSize.x; ++x )
                {
                    int idx = y * BoardUtility.BoardSize.x + x;

                    if (cells[idx].value == Entity.Null ||
                        activeTileFromEntity.Exists(cells[idx].value))
                    {
                        if( y == 0 )
                        {
                            //Debug.LogFormat("Line {0} is not full so can't be cleared. Pos {1} is empty", y, new int2(x,y));
                        }

                        lineFull = false;
                        break;
                    }
                }

                if (lineFull)
                {
                    //Debug.LogFormat("Clearing line {0}", y);
                    linesCleared[y] = true;
                    for (int x = 0; x < BoardUtility.BoardSize.x; ++x)
                    {
                        int idx = BoardUtility.IndexFromCellPos(x,y);
                        int3 cell = new int3(x, y, 0);

                        //Debug.LogFormat("Destroying {0} at cell {1}, idx {2}", cells[idx].value, cell, idx);
        
                        commandBuffer.DestroyEntity(chunkIndex, cells[idx].value);
                        
                        cells[idx] = new BoardCell { value = Entity.Null };
                    }
                }
            }
        }
    }

    // Update piece tiles after line clears
    [RequireComponentTag(typeof(Child))]
    struct UpdatePieceChildrenJob : IJobForEachWithEntity<Piece>
    {
        [NativeDisableParallelForRestriction]
        public BufferFromEntity<Child> tilesFromEntity;

        [ReadOnly]
        public EntityCommandBuffer.Concurrent commandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly] ref Piece piece)
        {

            var tiles = tilesFromEntity[entity];

            for( int i = tiles.Length - 1; i >= 0; --i )
            {
                if (tiles[i].Value == Entity.Null)
                    tiles.RemoveAt(i);
            }

            //Destroy empty pieces
            if (tiles.Length == 0)
                commandBuffer.DestroyEntity(index, entity);
        }
    };

    [BurstCompile]
    [ExcludeComponent(typeof(ActiveTile))]
    [RequireComponentTag(typeof(PieceTile))]
    struct MoveTilesAfterLineClear : IJobForEachWithEntity<Parent>
    {
        [ReadOnly]
        public NativeArray<bool> linesCleared;
        
        // Guaranteed safe since we only write to tile translation
        [NativeDisableParallelForRestriction]
        public ComponentDataFromEntity<Translation> posFromEntity;

        public void Execute(Entity entity, int index, [ReadOnly] ref Parent piece)
        {
            float3 piecePos = posFromEntity[piece.Value].Value;
            float3 tilePos = posFromEntity[entity].Value;

            //Debug.Log("MOVING TILES AFTER LINECLEAR");

            for (int height = 0; height < linesCleared.Length; ++height)
            {
                // We can't use localtoworld since our SnapToGrid system
                // might have changed the parent translation and our
                // ltw would be out of date
                float3 worldPos = piecePos + tilePos;
                int yPos = (int)math.floor(worldPos.y);
                if (linesCleared[height] && yPos >= height)
                {
                    var oldPos = tilePos;
                    tilePos.y--;
                    posFromEntity[entity] = new Translation { Value = tilePos };

                    //Debug.LogFormat("Line cleared at {0}, moving {1} down from {2} to {3}", height, entity, oldPos.y, tilePos.y);
                }

            }
        }
    }
    
    // Update the "Lines Cleared" UI
    struct NotifyUIJob : IJob
    {
        [ReadOnly]
        public EntityCommandBuffer buffer;

        [ReadOnly]
        [DeallocateOnJobCompletion]
        public NativeArray<bool> linesCleared;

        public void Execute()
        {
            int lineClearCount = 0;
            for (int i = 0; i < linesCleared.Length; ++i)
                if (linesCleared[i])
                    lineClearCount++;

            var ent = buffer.CreateEntity();
            buffer.AddComponent(ent, new UpdateLineClearUI { linesClearedCount = lineClearCount});
        }
    };

    protected override void OnCreate()
    {
        boardQuery_ = GetEntityQuery(typeof(BoardCell));
        initCommmandBufferSystem_ = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = inputDependencies;

        NativeArray<bool> linesCleared = new NativeArray<bool>(BoardUtility.BoardSize.y, Allocator.Persistent);
        
        job = new LineClearSystemJob
        { 
            cellType = GetArchetypeChunkComponentType<BoardCell>(false),
            commandBuffer = initCommmandBufferSystem_.CreateCommandBuffer().ToConcurrent(),
            parentFromEntity = GetComponentDataFromEntity<Parent>(true),
            linesCleared = linesCleared,
            activeTileFromEntity = GetComponentDataFromEntity<ActiveTile>(true),
        }.Schedule(boardQuery_, job);

        initCommmandBufferSystem_.AddJobHandleForProducer(job);
        
        job = new UpdatePieceChildrenJob
        {
            tilesFromEntity = GetBufferFromEntity<Child>(false),
            commandBuffer = initCommmandBufferSystem_.CreateCommandBuffer().ToConcurrent(),
        }.Schedule(this, job);

        initCommmandBufferSystem_.AddJobHandleForProducer(job);

        job = new MoveTilesAfterLineClear
        {
            linesCleared = linesCleared,
            posFromEntity = GetComponentDataFromEntity<Translation>(false),
        }.Schedule(this, job);

        job = new NotifyUIJob
        {
            linesCleared = linesCleared,
            buffer = initCommmandBufferSystem_.CreateCommandBuffer(),
        }.Schedule(job);

        initCommmandBufferSystem_.AddJobHandleForProducer(job);

        return job;
    }
}