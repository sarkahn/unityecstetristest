using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class BoardData
{
    public Entity board;
    // List of job handles that operate on the board.
    // We will add them to the list as systems process the data, then
    // call JobHandle.CombineDependencies before we schedule them.
    public NativeList<JobHandle> jobHandles;
}
