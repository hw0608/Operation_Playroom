using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class OccupySystem : NetworkBehaviour
{
    // ������ �ڿ�
    [SerializeField] NetworkVariable<int> redTeamResourceCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] NetworkVariable<int> blueTeamResourceCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // ä���� �� �ڿ�
    const int resourceFillCount = 3 ;

    // ������ �ʱ� ����
    Owner currentOwner = Owner.Neutral;

    // �̹��� ��ġ
    [SerializeField] Image redTeamResourceCountImage;
    [SerializeField] Image blueTeamResourceCountImage;

    // ������ ������ ��ũ���ͺ� ������Ʈ
    [SerializeField] OccupyScriptableObject occupyData;

    ResourceSpawner resourceSpawner;

    void Update()
    {
        if (IsServer)
            DetectResources();
    }
    public override void OnNetworkSpawn()
    {
        resourceSpawner = GameObject.FindFirstObjectByType<ResourceSpawner>();
        if (!IsServer)
        {
            redTeamResourceCount.OnValueChanged += (oldValue, newValue) => UpdateVisuals();
            blueTeamResourceCount.OnValueChanged += (oldValue, newValue) => UpdateVisuals();
        }
    }

    void DetectResources() // ������ �� �ڿ� ����
    {
        if (currentOwner != Owner.Neutral) return;

        Collider[] colliders = Physics.OverlapSphere(transform.position, transform.localScale.x / 2);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Item"))
            {
                ulong resourceId = collider.GetComponent<NetworkObject>().NetworkObjectId;
                ResourceData data = collider.GetComponent<ResourceData>();
                Debug.Log("detect!");

                if (data.CurrentOwner == Owner.Red) redTeamResourceCount.Value++;
                else if (data.CurrentOwner == Owner.Blue) blueTeamResourceCount.Value++;
                data.isColliderEnable.Value = false;
                Managers.Pool.Push(collider.gameObject);
                PushObjectClientRpc(resourceId);
                resourceSpawner.currentSpawnCount--;
                CheckOwnership();
            }
        }
    }

    [ClientRpc(RequireOwnership = false)]
    public void PushObjectClientRpc(ulong resourceId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(resourceId, out NetworkObject resourceObject))
        {
            resourceObject.gameObject.SetActive(false);
            for (int i = 0; i < resourceObject.transform.childCount; i++)
            {
                resourceObject.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    void CheckOwnership() // ������ ������ �˻�
    {
        if (currentOwner == Owner.Neutral)
        {
            if (redTeamResourceCount.Value >= resourceFillCount)
                ChangeOwnership(Owner.Red);
            else if (blueTeamResourceCount.Value >= resourceFillCount)
                ChangeOwnership(Owner.Blue);
        }
    }

    void ChangeOwnership(Owner newOwner)
    {
        currentOwner = newOwner;

        InstantiateBuilding(newOwner);
        ChangeOwnershipClientRpc(newOwner);
    }

    [ClientRpc]
    void ChangeOwnershipClientRpc(Owner newOwner) // �߸� �������� �������� ������ ���� Ŭ��RPC
    {
        currentOwner = newOwner;
        if (newOwner == Owner.Neutral)
        {
            GetComponent<Renderer>().material.color = new Color(0, 0, 0);
        }
        else
        {
            GetComponent<Renderer>().material.color = (newOwner == Owner.Red) ? Color.red : Color.blue;
        }
    }

    void InstantiateBuilding(Owner newOwner)
    {
        ResetFillAmount();
        GameObject building = (newOwner == Owner.Red) ?
            Managers.Resource.Instantiate("BuildingRed", null, true) :
            Managers.Resource.Instantiate("BuildingBlue", null, true);

        building.transform.localPosition = new Vector3(0f, -40f, 0f);
        building.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);

        NetworkObject networkObject = building.GetComponent<NetworkObject>();
        networkObject.TrySetParent(transform.GetComponent<NetworkObject>());
        ActiveNetworkObjectClientRpc(networkObject.NetworkObjectId,true);
        building.GetComponent<Building>().BuildingInit();
    }

    [ClientRpc]
    public void ActiveNetworkObjectClientRpc(ulong networkObjectId, bool isActive)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject no))
        {
            no.gameObject.SetActive(isActive);
        }
    }
    
    void UpdateVisuals() // �ڿ� ���� �ð� ȿ��
    {
        float redFill = Mathf.Clamp((float)redTeamResourceCount.Value / resourceFillCount, 0f, 1f);
        float blueFill = Mathf.Clamp((float)blueTeamResourceCount.Value / resourceFillCount, 0f, 1f);

        redTeamResourceCountImage.fillAmount = redFill;
        blueTeamResourceCountImage.fillAmount = blueFill;
    }

    void ResetFillAmount() // �ڿ� ���� �ð� ȿ�� �ʱ�ȭ
    {
        redTeamResourceCount.Value = 0;
        blueTeamResourceCount.Value = 0;
    }

    public void ResetOwnership() // �ǹ� �ı� �� ������ �ʱ�ȭ ����RPC
    {
        currentOwner = Owner.Neutral;
        ChangeOwnershipClientRpc(currentOwner);

        redTeamResourceCount.Value = 0;
        blueTeamResourceCount.Value = 0;
    }
}
