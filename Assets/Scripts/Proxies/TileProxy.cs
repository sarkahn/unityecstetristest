using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class TileProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        //dstManager.AddComponentData(entity, new Tile());
        //var buffer = dstManager.AddBuffer<Tile>(entity);
        //for( int i = 0; i < 4; ++i )
        //    buffer.Add(new Int3)
    }
}
