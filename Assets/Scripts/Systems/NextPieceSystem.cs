using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;

[UpdateAfter(typeof(LineClearSystem))]
public class NextPieceSystem : ComponentSystem
{
    NextPieceQueue pieceQueue_;
    EntityQuery droppedPieceQuery_;

    protected override void OnCreate()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        droppedPieceQuery_ = GetEntityQuery(typeof(DroppedPiece));
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        pieceQueue_ = GameObject.FindObjectOfType<NextPieceQueue>();
    }
    

    protected override void OnUpdate()
    {
        Entities.With(droppedPieceQuery_).ForEach(
            (Entity e)=>
            {
                PostUpdateCommands.RemoveComponent(e, typeof(DroppedPiece));
                
            });

        pieceQueue_.GetNextPiece();
    }
}