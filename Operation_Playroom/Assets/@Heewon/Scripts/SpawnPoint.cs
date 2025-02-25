using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    static Dictionary<GameTeam, List<GameObject>> spawnPoints = new Dictionary<GameTeam, List<GameObject>>();
    [SerializeField] GameTeam team;

    private void OnEnable()
    {
        if (spawnPoints.ContainsKey(team))
            spawnPoints[team].Add(gameObject);
        else
        {
            spawnPoints.Add(team, new List<GameObject> { gameObject });
        }
    }

    private void OnDisable()
    {
        spawnPoints[team].Remove(gameObject);
    }

    public static Vector3 GetRandomSpawnPoint(GameTeam team)
    {
        if (spawnPoints.Count == 0)
        {
            return Vector3.zero;
        }

        int idx = Random.Range(0, spawnPoints.Count);
        Vector3 randomPoint = spawnPoints[team][idx].transform.position;
        spawnPoints[team][idx].gameObject.SetActive(false);

        return randomPoint;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = team == GameTeam.Blue ? Color.blue : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }
}
