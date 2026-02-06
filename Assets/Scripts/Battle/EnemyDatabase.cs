using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EnemyDatabase", menuName = "Battle/EnemyDatabase")]
public class EnemyDatabase : ScriptableObject
{
    public List<GameObject> enemyPrefabs = new List<GameObject>();

    public List<GameObject> GetRandomEnemies(int count)
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogWarning("[EnemyDatabase] No enemy prefabs available!");
            return new List<GameObject>();
        }

        List<GameObject> result = new List<GameObject>();
        for (int i = 0; i < count; i++)
        {
            if (enemyPrefabs.Count > 0)
            {
                GameObject randomEnemy = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Count)];
                result.Add(randomEnemy);
            }
        }
        return result;
    }
}












