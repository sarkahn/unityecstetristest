using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[DisableAutoCreation]
public class BoardSystem : JobComponentSystem
{
    public NativeArray<bool> board_;
    int2 boardSize = new int2(10, 20);
    AABB bounds = new AABB();
    public JobHandle boardJob_;

    //[BurstCompile]
    struct BoardSystemJob : IJobForEach</*Tile, */LocalToWorld>
    {
        [WriteOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<bool> board;
        public int2 boardSize;
        public AABB bounds;

        public void Execute(/*ref Tile tile,*/ [ReadOnly] ref LocalToWorld localToWorld)
        {
            int2 pos = ((int3)localToWorld.Position).xy;
            int index = pos.x * boardSize.x + pos.y;
            index = math.clamp(index, 0, boardSize.x * boardSize.y);

            //tile.onBoard = bounds.Contains(localToWorld.Position);

            //if( tile.onBoard )
            //    board[index] = true;
        }
    }

    protected override void OnCreate()
    {
        board_ = new NativeArray<bool>(boardSize.x * boardSize.y, Allocator.Persistent);
        bounds.Center = new float3(0);
        bounds.Extents = new float3(boardSize.x * .5f, boardSize.y * .5f, 10000);
    }

    protected override void OnDestroy()
    {
        board_.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        for (int i = 0; i < board_.Length; ++i)
            board_[i] = false;

        var lookup = GetBufferFromEntity<Child>(true);
        

        var job = new BoardSystemJob
        {
            board = board_,
            boardSize = boardSize,
            bounds = bounds,
        };
        
        boardJob_ = job.Schedule(this, inputDependencies);
        // Now that the job is set up, schedule it to be run. 
        return boardJob_;
    }
}