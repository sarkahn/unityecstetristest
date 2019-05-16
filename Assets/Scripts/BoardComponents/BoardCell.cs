using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


/// <summary>
/// A component representing a single cell of our 
/// game board. See <see cref="CreateBoardChunk"/>
/// </summary>
[Serializable]
public struct BoardCell : IComponentData
{
    public Entity value;
    //public static implicit operator Entity(BoardCell cell) { return cell.value; }
    //public static implicit operator BoardCell(Entity e) { return new BoardCell { value = e }; }
}
