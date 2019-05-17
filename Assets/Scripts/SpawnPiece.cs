using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

//public class SpawnPiece : MonoBehaviour
//{
//    public PieceType pieceType_;
//    NextPieceQueue pieceQueue_;

//    private void Awake()
//    {
//        pieceQueue_ = FindObjectOfType<NextPieceQueue>();
//    }

//    private void Update()
//    {
//        var piece = Instantiate(pieceQueue_.piecePrefabs_[(int)pieceType_]);

//        float3 p = math.floor(transform.position);
//        var offset = piece.GetComponent<PieceComponent>().spawnOffset_;
//        p += new float3(0.5f, 0.5f, 0.5f) + new float3(offset, offset, 0);

//        piece.transform.position = p;
//        piece.AddComponent<ConvertToEntity>();
//        Destroy(gameObject);
//    }

//    private void OnDrawGizmos()
//    {
//        Gizmos.color = Color.magenta;
//        Gizmos.DrawCube(transform.position, Vector3.one);
//    }
//} 