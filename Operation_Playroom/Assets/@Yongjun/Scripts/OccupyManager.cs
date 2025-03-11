using System.Linq;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class OccupyManager : NetworkBehaviour
{
    [SerializeField] GameObject occupyPrefab;
    [SerializeField] Transform occupyPoints;
    [SerializeField] Transform occupyPool;

    [SerializeField] TextMeshProUGUI redTeamOccupyCountText;
    [SerializeField] TextMeshProUGUI blueTeamOccupyCountText;

    private NetworkVariable<int> redTeamOccupyCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> blueTeamOccupyCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Managers.Resource.LoadAllAsync<GameObject>("default", null);
            GenerateOccupy();
        }

        redTeamOccupyCount.OnValueChanged += OnRedTeamOccupyCountChanged;
        blueTeamOccupyCount.OnValueChanged += OnBlueTeamOccupyCountChanged;

        UpdateUI();
    }

    private void OnRedTeamOccupyCountChanged(int oldValue, int newValue)
    {
        UpdateUI();
    }

    private void OnBlueTeamOccupyCountChanged(int oldValue, int newValue)
    {
        UpdateUI();
    }

    private void GenerateOccupy()
    {
        foreach (Transform child in occupyPoints)
        {
            GameObject occupyInstance = Managers.Resource.Instantiate("Occupy");
            NetworkObject networkObject = occupyInstance.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                networkObject.TrySetParent(occupyPool.GetComponent<NetworkObject>());
            }
            occupyInstance.transform.position = child.position;
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

    public void UpdateOccupyCount(Owner owner, int amount)
    {
        if (IsServer)
        {
            if (owner == Owner.Red)
            {
                redTeamOccupyCount.Value += amount;
            }
            else if (owner == Owner.Blue)
            {
                blueTeamOccupyCount.Value += amount;
            }
        }
    }

    private void UpdateUI()
    {
        redTeamOccupyCountText.text = $"{redTeamOccupyCount.Value}";
        blueTeamOccupyCountText.text = $"{blueTeamOccupyCount.Value}";
    }
}