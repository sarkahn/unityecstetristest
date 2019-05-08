using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public interface IBoardSystem
{
    NativeArray<bool> GetBoard();
    NativeList<JobHandle> GetBoardJobs();
    int2 GetBoardSize();
}
