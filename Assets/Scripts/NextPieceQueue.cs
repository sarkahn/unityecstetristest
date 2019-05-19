using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using System.Linq;
using Unity.Entities;
using Unity.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NextPieceQueue : MonoBehaviour
{
    [SerializeField]
    public List<GameObject> piecePrefabs_;

    [SerializeField]
    List<Transform> queuePositions_;

    [SerializeField]
    Transform activePieceSpawnPoint_;

    [SerializeField]
    int queueSize_ = 6;

    [SerializeField]
    Canvas canvas_;
    
    List<int> shuffleBag_ = new List<int>();

    Queue<GameObject> pieceQueue_ = new Queue<GameObject>();

    [SerializeField]
    bool lockPiece_ = false;

    [SerializeField]
    PieceType lockedPieceType_;


    [ContextMenu("PrintPositions")]
    void PrintPositions()
    {
        foreach (var t in queuePositions_)
        {
            Debug.LogFormat("POSITION of {0}: {1}", t.name, t.position.ToString());
        }
    }



    private void Update()
    {
        FillQueue();
    }

    public GameObject GetNextPiece()
    {
        return GetNextPiece(activePieceSpawnPoint_.position);
    }

    public GameObject GetNextPiece(Vector3 pos)
    {
        FillQueue();

        var nextPiece = pieceQueue_.Dequeue();
        nextPiece.transform.SetParent(null);
        nextPiece.transform.localScale = Vector3.one;


        var p = new float3(pos);
        var pieceComponent = nextPiece.GetComponent<PieceComponent>();
        var spawnOffset = new float3(pieceComponent.spawnOffset_, pieceComponent.spawnOffset_, 0);

        p = math.floor(p) + new float3(0.5f, 0.5f, 0) + spawnOffset;
        nextPiece.transform.position = p;

        nextPiece.AddComponent<SpawnedPieceComponent>();
        nextPiece.AddComponent<ConvertToEntity>();

        return nextPiece;
    }

    [ContextMenu("FillQueue")]
    void FillQueue()
    {
        while(pieceQueue_.Count < queueSize_)
        {
            var prefab = piecePrefabs_[PullFromShuffleBag()];
            var newPiece = Instantiate(prefab, transform, false);
            pieceQueue_.Enqueue(newPiece);
        }

        int i = 0;
        foreach (var piece in pieceQueue_)
            piece.transform.position = queuePositions_[i++].position;
    }
    
    int PullFromShuffleBag()
    {
        if (shuffleBag_.Count == 0)
            FillShuffleBag();

        int i = shuffleBag_[shuffleBag_.Count - 1];
        shuffleBag_.RemoveAt(shuffleBag_.Count - 1);

        return i;
    }

    void FillShuffleBag()
    {
        for (int i = 0; i < piecePrefabs_.Count; ++i)
        {
            if( lockPiece_ )
            {
                shuffleBag_.Add((int)lockedPieceType_);
            }
            else
            {
                shuffleBag_.Add(i);
            }
        }

        for (int i = shuffleBag_.Count - 1; i > 0; --i)
        {
            int j = UnityEngine.Random.Range(0, i);
            int swap = shuffleBag_[j];
            shuffleBag_[j] = shuffleBag_[i];
            shuffleBag_[i] = swap;
        }
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        
        queueSize_ = Mathf.Min(queueSize_, queuePositions_.Count);

        if( Application.isPlaying )
        {
            while (pieceQueue_.Count > queueSize_ && queueSize_ > 0)
            {
                var piece = pieceQueue_.Dequeue();
                DestroyImmediate(piece.gameObject, false);
            }
        }
    }
#endif
}
