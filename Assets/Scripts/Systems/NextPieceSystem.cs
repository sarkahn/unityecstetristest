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
        droppedPieceQuery_ = GetEntityQuery(typeof(SpawnNextPiece));
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        pieceQueue_ = GameObject.FindObjectOfType<NextPieceQueue>();
        if (pieceQueue_ == null)
            Enabled = false;
    }
    

    protected override void OnUpdate()
    {
        Entities.With(droppedPieceQuery_).ForEach(
            (Entity e)=>
            {
                PostUpdateCommands.RemoveComponent(e, typeof(SpawnNextPiece));
                
            });

        pieceQueue_.GetNextPiece();
        InputHandling.ResetDropTimer();
    }
}