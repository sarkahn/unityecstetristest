using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BoardUtility
{
    public static readonly int2 BoardSize = new int2(10, 20);
    public static int BoardCellCount { get { return BoardSize.x * BoardSize.y; } }

    public static int IndexFromCellPos(int x, int y )
    {
        return IndexFromCellPos(new int3(x, y, 0));
    }

    public static int IndexFromWorldPosition(float3 worldPos)
    {
        int3 cellPos = (int3)math.floor(worldPos);
        int index = IndexFromCellPos(cellPos);

        //Debug.LogFormat("WorldPos {0}, Floored {1}, CellPos {2}, Index {3}", worldPos, math.floor(worldPos), cellPos, index);

        return index;
    }
    
    public static int IndexFromWorldPos(float3 worldPos)
    {
        return IndexFromCellPos((int3)math.floor(worldPos));
    }

    public static int IndexFromCellPos(int3 cellPos)
    {
        return cellPos.y * BoardSize.x + cellPos.x;
    }

    public static bool InBounds(int3 cell)
    {
        return cell.x >= 0 && cell.x < BoardSize.x &&
            cell.y >= 0 && cell.y < BoardSize.y;
    }

    public static bool IndexInBounds(int idx)
    {
        return idx >= 0 && idx < BoardCellCount;
    }

    public static int3 CellFromWorldPos(float3 worldPos)
    {
        return (int3)math.floor(worldPos);
    }

    // Round each component to the nearest given step
    public static float3 RoundedStep(float3 val, float step)
    {
        step = math.max(step, 0.01f);
        val = math.round(val / step) * step;
        return val;
    }

    public static void GameOver()
    {
        foreach (var system in World.Active.Systems)
            system.Enabled = false;

        Time.timeScale = 0;
        SceneManager.LoadScene("GameOverScene", LoadSceneMode.Additive);
        
    }
}
