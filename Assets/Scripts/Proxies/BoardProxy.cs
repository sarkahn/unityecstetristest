using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class BoardProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField]
    int2 size_ = new int2(10, 20);

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {

        var buffer = dstManager.AddBuffer<BoardCell>(entity);

        for (int i = 0; i < size_.x * size_.y; ++i)
            buffer.Add(new BoardCell());

        dstManager.AddComponentData<Board>(entity, new Board() );
    }
}
