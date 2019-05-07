using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class PieceTilesProxy : MonoBehaviour, IConvertGameObjectToEntity
{
    // Add fields to your component here. Remember that:
    //
    // * The purpose of this class is to store data for authoring purposes - it is not for use while the game is
    //   running.
    // 
    // * Traditional Unity serialization rules apply: fields must be public or marked with [SerializeField], and
    //   must be one of the supported types.
    //
    // For example,
    //    public float scale;
    
    

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        //var tilesBuffer = dstManager.AddBuffer<PieceTiles>(entity);

        
        //Debug.Log("TilesBufferSize: " + tilesBuffer.Length);
        

        //for (int i = 0; i < childBuffer.Length; ++i)
        //{
        //    Child c = childBuffer[i];
        //    var localToParent = dstManager.GetComponentData<LocalToParent>(c.Value);
        //    //tilesBuffer[i] = localToParent;
        //}

    }
}
