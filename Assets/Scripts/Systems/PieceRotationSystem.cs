using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(InitializeBoardSystem))]
public class PieceRotationSystem : JobComponentSystem
{
    [BurstCompile]
    [RequireComponentTag(typeof(Piece), typeof(ActivePiece), typeof(PieceTiles))]
    struct PieceRotationSystemJob : IJobForEachWithEntity<Translation>
    {
        [ReadOnly]
        public NativeArray<BoardCell> board;

        [ReadOnly]
        public BufferFromEntity<PieceTiles> tilesLookup;

        public int rotDirection;

        public void Execute(Entity entity, int index, ref Translation c0)
        {
            var tiles = tilesLookup[entity];

            var rotation = quaternion.RotateZ(math.radians(90f * rotDirection));

            float3 piecePos = c0.Value;
            for ( int i = 0; i < tiles.Length; ++i )
            {
                float3 tilePos = tiles[i].tilePos;
                tilePos = math.rotate(rotation, tilePos);

            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        //var boardEntity = GetSingletonEntity<Board>();
        //var board = EntityManager.GetBuffer<BoardCell>(boardEntity).AsNativeArray();
        var job = inputDependencies;

        //job = new PieceRotationSystemJob
        //{
        //    board = board,
        //    tilesLookup = GetBufferFromEntity<PieceTiles>(false),
        //}.Schedule(this, inputDependencies);

        // Now that the job is set up, schedule it to be run. 
        return job;
    }
}