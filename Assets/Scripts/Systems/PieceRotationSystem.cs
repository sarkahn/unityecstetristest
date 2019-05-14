using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Transforms;
using Unity.Collections;

public class PieceRotationSystem : JobComponentSystem
{
    EntityQuery boardQuery_;

    //[BurstCompile]
    
    [RequireComponentTag(typeof(ActivePiece), typeof(Child), typeof(Translation))]
    struct PieceRotationSystemJob : IJobForEachWithEntity<Piece>
    {
        [ReadOnly]
        public BufferFromEntity<Child> childrenFromEntity;
        
        [NativeDisableParallelForRestriction]
        public ComponentDataFromEntity<Translation> posFromEntity;

        [ReadOnly]
        [DeallocateOnJobCompletion]
        public NativeArray<BoardCell> board;

        public int rotationDirection;

        public void Execute(Entity entity, int index, ref Piece piece)
        {
            var children = childrenFromEntity[entity];
            var piecePos = posFromEntity[entity].Value;

            var rot = quaternion.RotateZ(math.radians(90f * rotationDirection));

            for( int i = 0; i < children.Length; ++i )
            {
                var child = children[i].Value;
                var tilePos = posFromEntity[children[i].Value].Value;

                var rotated = math.rotate(rot, tilePos);
                int3 cell = (int3)math.floor(tilePos + piecePos);

                int idx = cell.y * BoardUtility.BoardSize.x + cell.x;

                if (idx < 0 || idx >= board.Length || board[idx] != Entity.Null )
                {
                    Debug.LogFormat("Unable to rotate. Idx {0}, Cell {1}", idx, cell);
                    break;
                }

                posFromEntity[child] = new Translation { Value = rotated };
            }
        }


    }

    protected override void OnCreate()
    {
        boardQuery_ = GetEntityQuery(ComponentType.ReadOnly<BoardCell>());
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = inputDeps;

        int rotation = InputHandling.GetRotationInput();

        if (rotation != 0 )
        {
            //Debug.Log("ROTATING");
            var board = boardQuery_.ToComponentDataArray<BoardCell>(Allocator.TempJob);

            job = new PieceRotationSystemJob
            {
                childrenFromEntity = GetBufferFromEntity<Child>(true),
                posFromEntity = GetComponentDataFromEntity<Translation>(false),
                board = board,
                rotationDirection = rotation,
            }.Schedule(this, job);
        }


        return job;
    }
}
