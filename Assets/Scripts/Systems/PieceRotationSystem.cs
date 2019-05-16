using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Transforms;
using Unity.Collections;

//[DisableAutoCreation]
[UpdateAfter(typeof(InitBoardSystem))]
public class PieceRotationSystem : JobComponentSystem
{
    EntityQuery boardQuery_;

    [BurstCompile]
    [RequireComponentTag(typeof(ActivePiece), typeof(Child), typeof(Translation))]
    struct PieceRotationSystemJob : IJobForEachWithEntity<Piece>
    {
        [ReadOnly]
        public BufferFromEntity<Child> childrenFromEntity;
        
        public ComponentDataFromEntity<Translation> posFromEntity;
        
        [NativeDisableParallelForRestriction]
        [DeallocateOnJobCompletion]
        public NativeArray<BoardCell> board;

        [ReadOnly]
        public ComponentDataFromEntity<Parent> parentFromEntity;
        
        public int rotationDirection;

        public void Execute(Entity entity, int index, ref Piece piece)
        {
            //if (piece.pieceType == PieceType.OPiece)
            //    return;

            var children = childrenFromEntity[entity];
            var piecePos = posFromEntity[entity].Value;

            var rot = quaternion.RotateZ(math.radians(90f * rotationDirection));

            NativeArray<float3> oldPositions = new NativeArray<float3>(children.Length, Allocator.Temp);
            NativeArray<float3> newPositions = new NativeArray<float3>(children.Length, Allocator.Temp);

            {
                //int idx = BoardUtility.IndexFromCellPos(new int3(1, 1, 0));
                //Debug.LogFormat("Piece {0} ({1}), Board Cell 1,1: {2}", entity, piece.pieceType, board[idx].value);
            }

            for ( int i = 0; i < children.Length; ++i )
            {
                var child = children[i].Value;
                var tilePos = posFromEntity[children[i].Value].Value;
                
                var rotated = math.rotate(rot, tilePos);
                rotated = BoardUtility.RoundedStep(rotated, .5f);
                int3 cell = BoardUtility.CellFromWorldPos(piecePos + rotated);

                //Debug.LogFormat("Rotating from {0} to {1}:", 
                //    BoardUtility.CellFromWorldPos(piecePos + tilePos), cell);

                int idx = BoardUtility.IndexFromCellPos(cell);

                if (cell.x < 0 || cell.x >= BoardUtility.BoardSize.x || 
                    cell.y < 0 ||
                    !BoardSpaceIsClear(entity, cell))

                {
                    //Debug.LogFormat("PIECE {0} CANNOT BE ROTATED. CELL {1} CONTAINS {2}", entity, cell, board[idx].value);
                    return;
                }

                oldPositions[i] = tilePos;
                newPositions[i] = rotated;
            }

            // Clear old positions
            for( int i = 0; i < oldPositions.Length; ++i )
            {
                int idx = BoardUtility.IndexFromWorldPos(piecePos + oldPositions[i]);
                if (BoardUtility.IndexInBounds(idx))
                    board[idx] = new BoardCell { value = Entity.Null };
            }

            // Update new positions
            for( int i = 0; i < newPositions.Length; ++i )
            {
                var tile = children[i].Value;
                posFromEntity[tile] = new Translation { Value = newPositions[i] };

                int idx = BoardUtility.IndexFromWorldPos(piecePos + newPositions[i]);
                if(BoardUtility.IndexInBounds(idx))
                    board[idx] = new BoardCell { value = tile };
            }
        }

        bool BoardSpaceIsClear(Entity self, int3 cell)
        {
            int idx = cell.y * BoardUtility.BoardSize.x + cell.x;
            return (!BoardUtility.IndexInBounds(idx)) || board[idx].value == Entity.Null ||
                parentFromEntity[board[idx].value].Value == self;
        }


    }

    struct RotationJob : IJobForEachWithEntity<Piece>
    {
        [NativeDisableParallelForRestriction]
        public ComponentDataFromEntity<Translation> posFromEntity;

        [ReadOnly]
        public BufferFromEntity<Child> childrenFromEntity;
        
        public void Execute(Entity entity, int index, ref Piece piece)
        {
            float3 piecePos = posFromEntity[entity].Value;

            var children = childrenFromEntity[entity];
        }
    }

    protected override void OnCreate()
    {
        boardQuery_ = GetEntityQuery(ComponentType.ReadWrite<BoardCell>());
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = inputDeps;

        int rotation = InputHandling.GetRotationInput();

        if (rotation != 0 )
        {
            JobHandle getBoardJob;
            var board = boardQuery_.ToComponentDataArray<BoardCell>(Allocator.TempJob, out getBoardJob);

            job = new PieceRotationSystemJob
            {
                childrenFromEntity = GetBufferFromEntity<Child>(true),
                posFromEntity = GetComponentDataFromEntity<Translation>(false),
                board = board,
                rotationDirection = rotation,
                parentFromEntity = GetComponentDataFromEntity<Parent>(true),
            }.ScheduleSingle(this, JobHandle.CombineDependencies(getBoardJob, job));
        }

        return job;
    }
}
