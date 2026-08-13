using System.Linq;
using UnityEngine;

public interface ISpawnPointStrategy 
{
    Transform NextSpawnPoint();
}