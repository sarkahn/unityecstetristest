using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class BoardProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent(entity, typeof(Board));
        var buffer = dstManager.AddBuffer<BoardCell>(entity);
        for( int i = 0; i < BoardUtility.BoardCellCount; ++i )
        {
            buffer.Add(Entity.Null);
        }
    }
}
