using EMILtools.Extensions;
using UnityEngine;

public class EntityFactory<T> : IEntityFactory<T> where T : Entity
{
    EntityData[] data;
    
    public EntityFactory(EntityData[] _data)
    {
        data = _data;
    }
    
    public T Create(Transform spawnPoint)
    {
        EntityData entityData = data.Rand();
        return GameObject.Instantiate(entityData.prefab, spawnPoint.position, spawnPoint.rotation)
            .Get<T>();
    }
}