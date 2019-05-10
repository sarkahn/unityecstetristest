using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public static class BoardUtility
{

    public static int3 ToCellPos(float3 tilePos, float3 piecePos)
    {
        return (int3)math.floor(tilePos + piecePos - .5f);
    }
    
    public static bool IsValidPosition(int3 cell, int2 boardSize, ref NativeArray<bool> board)
    {
        int idx = cell.y * boardSize.x + cell.x;

        bool inBounds =
            cell.x >= 0 && cell.x < boardSize.x &&
            cell.y >= 0 && cell.y < boardSize.y;

        if (!inBounds || board[idx])
            return false;

        return true;
    }
}
