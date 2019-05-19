using ECSTetris.Tests;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

[TestFixture]
public class LineClearTest : ECSTestsFixture
{
    [Test]
    public void FillLineClears()
    {
        var em = m_Manager;
        var arch = em.CreateArchetype(typeof(BoardCell));
        NativeArray<ArchetypeChunk> chunks = new NativeArray<ArchetypeChunk>(1, Allocator.Temp);
        em.CreateChunk(arch, chunks, BoardUtility.BoardCellCount);

        var boardQuery = em.CreateEntityQuery(typeof(BoardCell));
        var board = boardQuery.ToComponentDataArray<BoardCell>(Allocator.TempJob);

        for( int x = 0; x < BoardUtility.BoardSize.x; ++x )
        {
            int idx = BoardUtility.IndexFromCellPos(new int3(x, 0, 0));
            board[idx] = em.CreateEntity();
        }
        board.Dispose();

        var sys = World.CreateSystem<LineClearSystem>();
        sys.Update();

        for( int x = 0; x < BoardUtility.BoardSize.x; ++x )
        {
            int idx = BoardUtility.IndexFromCellPos(new int3(x, 0, 0));
            Assert.AreEqual(Entity.Null, board[idx].value);
        }


    }
    
}
