
using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Transforms;
using Unity.Mathematics;
using System.Collections;

[UpdateBefore(typeof(BoardSystem))]
public class GetNextPieceSystem : ComponentSystem
{
    NextPieceQueue pieceQueue_;
    BoardSystem boardSystem_;

    protected override void OnCreate()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        boardSystem_ = World.GetOrCreateSystem<BoardSystem>();
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

        bool gameOver = false;
        //GameObject nextPiece = null;

        var heightMapEntity = GetSingletonEntity<HeightMap>();
        var heightMap = EntityManager.GetBuffer<HeightMapCell>(heightMapEntity).AsNativeArray();

        Entities.WithAll<ActivePieceState>().WithNone<ActivePiece>().ForEach( 
            (Entity e,  ref Translation translation) =>
            {
                var children = EntityManager.GetBuffer<Child>(e);
                
                float3 piecePos = translation.Value;

                for( int i = 0; i < children.Length; ++i )
                {
                    var child = children[i].Value;

                    float3 tilePos = EntityManager.GetComponentData<Translation>(child).Value;
                    int3 cell = BoardUtility.ToCellPos(tilePos, piecePos);

                    // Update height map with newly placed piece
                    heightMap[cell.x] = math.max(heightMap[cell.x], cell.y + 1);

                    PostUpdateCommands.RemoveComponent<ActiveTile>(child);

                    if( cell.y >= BoardUtility.BoardSize.y )
                    {
                        Debug.Log("GAMEOVER");
                        gameOver = true;
                        return;
                    }
                }

                PostUpdateCommands.RemoveComponent<ActivePieceState>(e);
                PostUpdateCommands.AddComponent<SpawnPiece>(e, new SpawnPiece());
                //nextPiece = true;
                
            });

        if( gameOver )
        {
            // Workaround for a bug that causes warning spam in the editor if you
            // disable a system when it's scheduling a job that uses an
            // entitycommandbuffer
            pieceQueue_.StartCoroutine(GameOverRoutine());
            return;
        }

        //if( nextPiece != null )
        //{
        //    nextPiece.AddComponent<ActivePieceProxy>();
        //    nextPiece.AddComponent<ConvertToEntity>();
        //}
        //if( nextPiece )
        //{
        //    //pieceQueue_.StartCoroutine(SetupNextPiece());

        //}
    }

    IEnumerator GameOverRoutine()
    {
        yield return null;
        BoardUtility.GameOver();
    }

    //IEnumerator SetupNextPiece()
    //{

    //    yield return null;
    //    nextPiece.AddComponent<ActivePieceProxy>();
    //    nextPiece.AddComponent<ConvertToEntity>();
    //}
}


