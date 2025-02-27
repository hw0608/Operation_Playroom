using UnityEngine;

public class SoldierSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] soldierPrefabs;
    [SerializeField] Transform[] soldierSpawnPoints;
    [SerializeField] Transform kingTransform;

    public void SpawnSoldiers()
    {
        int formationIndex = 0;

        for (int i = 0; i < soldierSpawnPoints.Length; i++)
        {
            // Soldier Prefab을 Spawn Point 위치에 생성
            GameObject soldier = Instantiate(soldierPrefabs[i], soldierSpawnPoints[i % soldierSpawnPoints.Length].position, Quaternion.identity);

            IFormable formableSoldier = soldier.GetComponent<IFormable>();
            if (formableSoldier != null)
            {
                formableSoldier.SoldierInitialize(kingTransform, formationIndex);
                formationIndex++;
            }
        }
    }
}