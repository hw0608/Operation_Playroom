using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class OccupyManager : NetworkBehaviour
{
    [SerializeField] GameObject occupyPrefab;
    [SerializeField] Transform occupyPoints;
    [SerializeField] Transform occupyPool;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            GenerateOccupy();
        }
    }

    private void GenerateOccupy()
    {
        foreach (Transform child in occupyPoints)
        {
            //GameObject occupyInstance = Instantiate(occupyPrefab, child.position, Quaternion.identity);
            GameObject occupyInstance = Managers.Resource.Instantiate("Occupy");

            NetworkObject networkObject = occupyInstance.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                networkObject.Spawn(true);
                networkObject.TrySetParent(occupyPool.GetComponent<NetworkObject>());
            }
            occupyInstance.transform.position = child.position; 
            occupyInstance.transform.SetParent(occupyPool, false);
        }
    }

    public Vector3[] GetRandomPoints()
    {
        Vector3[] paths = new Vector3[3];
        int pointsCount = occupyPoints.childCount;
        int i = 0;
        while (i < 3)
        {
            int num = Random.Range(0, pointsCount);

            if (!paths.Contains(occupyPoints.GetChild(num).position))
            {
                paths[i] = occupyPoints.GetChild(num).position;
                i++;
                Debug.Log(occupyPoints.GetChild(num).name);
            }
        }

        return paths;
    }
}