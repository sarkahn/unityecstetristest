using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class HeightMapProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent(entity, typeof(HeightMap));
        var buffer = dstManager.AddBuffer<HeightMapCell>(entity);
        for( int i = 0; i < BoardUtility.BoardSize.x; ++i )
        {
            buffer.Add(0);
        }
    }
}
