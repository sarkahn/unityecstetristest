using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NextPieceQueue : MonoBehaviour
{
    [SerializeField]
    List<GameObject> piecePrefabs_;

    [SerializeField]
    public List<Transform> queuePositions_;

    [SerializeField]
    int queueSize_ = 6;

    [SerializeField]
    Canvas canvas_;

    //[SerializeField]
    //Transform testTransform_;

    List<int> shuffleBag_ = new List<int>();

    Queue<GameObject> pieceQueue_ = new Queue<GameObject>();

    private void OnEnable()
    {
    }

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
        var piece = pieceQueue_.Dequeue();
        return piece;
    }

    [ContextMenu("FillQueue")]
    void FillQueue()
    {
        while(pieceQueue_.Count < queueSize_)
        {
            var prefab = piecePrefabs_[PullFromShuffleBag()];
            var newPiece = Instantiate(prefab, queuePositions_[pieceQueue_.Count], false);
            pieceQueue_.Enqueue(newPiece);
        }
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

        //PrintShuffleBagContents();

        for (int i = shuffleBag_.Count - 1; i > 0; --i)
        {
            int j = UnityEngine.Random.Range(0, i);
            int swap = shuffleBag_[j];
            shuffleBag_[j] = shuffleBag_[i];
            shuffleBag_[i] = swap;
        }

        //PrintShuffleBagContents();
    }

    void PrintShuffleBagContents()
    {
        var contents = string.Join(", ", shuffleBag_.Select(i => i.ToString()));
        Debug.LogFormat("Contents of shuffle bag: {0}", contents);
    }

    private void OnGUI()
    {
        if( queuePositions_ != null )
        {
            foreach (var t in queuePositions_)
            {
                GUILayout.Label(t.position.ToString());
            }
        }

        if( Camera.main != null )
        {
            //var p = testTransform_.position;
            //p = Input.mousePosition;
            //p = Camera.main.ScreenToViewportPoint(p);
            //GUILayout.Label(p.ToString());
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        
        queueSize_ = Mathf.Min(queueSize_, queuePositions_.Count);

        if( Application.isPlaying )
        {
            //FillQueue();

            while (pieceQueue_.Count > queueSize_ && queueSize_ > 0)
            {
                var piece = pieceQueue_.Dequeue();
                DestroyImmediate(piece.gameObject, false);
            }
        }
    }
#endif
}
