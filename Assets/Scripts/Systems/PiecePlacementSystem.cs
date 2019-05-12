
using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Transforms;
using Unity.Mathematics;
using System.Collections;

[UpdateAfter(typeof(BoardSystem))]
public class GetNextPieceSystem : ComponentSystem
{
    NextPieceQueue pieceQueue_;
    
    protected override void OnCreate()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        //query_ = GetEntityQuery(typeof(ActivePieceState), ComponentType.Exclude<ActivePiece>());
    }

    protected override void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        pieceQueue_ = GameObject.FindObjectOfType<NextPieceQueue>();
    }
    
    protected override void OnUpdate()
    {
        GameObject nextPiece = null;

        bool gameOver = false;

        Entities.WithAll<ActivePieceState>().WithNone<ActivePiece>().ForEach( 
            (Entity e,  ref Translation translation) =>
            {
                var tiles = EntityManager.GetBuffer<PieceTiles>(e);
                float3 piecePos = translation.Value;

                for( int i = 0; i < tiles.Length; ++i )
                {
                    float3 tilePos = tiles[i];
                    int3 cell = BoardUtility.ToCellPos(tilePos, piecePos);

                    if( cell.y >= BoardUtility.BoardSize.y )
                    {
                        gameOver = true;
                        return;
                    }
                }

                PostUpdateCommands.RemoveComponent<ActivePieceState>(e);
                
                nextPiece = pieceQueue_.GetNextPiece();
            });

        if( Input.GetButtonDown("Jump"))
        {
            pieceQueue_.StartCoroutine(GameOverRoutine());
        }

        if( nextPiece != null )
        {
            InputHandling.ResetDropTimer();
            nextPiece.AddComponent<ActivePieceProxy>();
            nextPiece.AddComponent<ConvertToEntity>();
        }
    }

    IEnumerator GameOverRoutine()
    {
        yield return null;
        BoardUtility.GameOver();
    }
}


