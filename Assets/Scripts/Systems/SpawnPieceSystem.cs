using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnPieceSystem : ComponentSystem
{
    NextPieceQueue nextPieceQueue_;

    protected override void OnCreate()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    protected override void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        nextPieceQueue_ = GameObject.FindObjectOfType<NextPieceQueue>();
    }

    protected override void OnUpdate()
    {
        GameObject nextPiece = null;
        Entities.WithAll<SpawnPiece>().ForEach(
            (Entity e) =>
            {
                PostUpdateCommands.RemoveComponent<SpawnPiece>(e);
                nextPiece = nextPieceQueue_.GetNextPiece();
            });

        if( nextPiece != null )
        {
            nextPiece.AddComponent<ActivePieceProxy>();
            var e = GameObjectConversionUtility.ConvertGameObjectHierarchy(nextPiece, World);
            GameObject.Destroy(nextPiece);

            var heightMapEntity = GetSingletonEntity<HeightMap>();
            var heightMap = EntityManager.GetBuffer<HeightMapCell>(heightMapEntity);

            var buffer = EntityManager.GetBuffer<PieceTiles>(e);
            int offset = 0;

            var piecePos = EntityManager.GetComponentData<Translation>(e).Value;
            for( int i = 0; i < buffer.Length; ++i )
            {
                var cell = BoardUtility.ToCellPos(piecePos, buffer[i]);
                if(heightMap[cell.x] >= cell.y )
                {
                    offset = math.max(offset, heightMap[cell.x] - cell.y);
                }
            }
            piecePos.y += offset;
            EntityManager.SetComponentData<Translation>(e, new Translation { Value = piecePos });
        }
    }
}