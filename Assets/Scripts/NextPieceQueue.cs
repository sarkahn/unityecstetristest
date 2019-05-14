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
    List<GameObject> piecePrefabs_;

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
        var nextPiece = pieceQueue_.Dequeue();
        nextPiece.transform.SetParent(null);
        nextPiece.transform.localScale = Vector3.one;
        nextPiece.transform.position = activePieceSpawnPoint_.position;
        //// Don't even talk to me
        //float3 snap = new float3(.5f, .5f, 0) + new float3(nextPiece.GetComponent<PieceProxy>().snapOffset_, 0);
        //snap = math.floor(nextPiece.transform.position) + snap;
        //nextPiece.transform.position = snap;
            

        //float3 piecePos = nextPiece.transform.position;
        //int offset = 0;
        //for( int i = 0; i < nextPiece.transform.childCount; ++i )
        //{
        //    var kid = nextPiece.transform.GetChild(i);
        //    int3 cell = BoardUtility.ToCellPos(kid.localPosition, piecePos);
        //    int height = heightMap[cell.x];
        //    if (height >= cell.y)
        //        offset = math.max(offset, height - cell.y + 1);
        //}
        //nextPiece.transform.Translate(new Vector3(0, offset, 0));

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
            shuffleBag_.Add(i);

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
