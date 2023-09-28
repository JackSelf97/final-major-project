using UnityEngine;

[CreateAssetMenu(fileName = "New Spawn Point", menuName = "Spawn Point")]
public class SpawnPointSO : ScriptableObject
{
    public Vector3[] playerSpawnPoint;
    public Vector3[] enemySpawnPoint;
}