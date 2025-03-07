using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using Unity.Netcode;

public class OccupySystem : NetworkBehaviour
{
    // ������ �ڿ�
    [SerializeField] NetworkVariable<int> redTeamResourceCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] NetworkVariable<int> blueTeamResourceCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // ä���� �� �ڿ�
    const int resourceFillCount = 3;

    // ������ �ʱ� ����
    Owner currentOwner = Owner.Neutral;

    // �̹��� ��ġ
    [SerializeField] Image redTeamResourceCountImage;
    [SerializeField] Image blueTeamResourceCountImage;

    // ������ ������ ��ũ���ͺ� ������Ʈ
    [SerializeField] OccupyScriptableObject occupyData;

    void Update()
    {
        if (IsServer)
            DetectResources();
    }
    public override void OnNetworkSpawn()
    {
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

                //data.PushObjectClientRpc();
                //PushObjectClientRpc(resourceId);

                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(resourceId, out NetworkObject resourceObject))
                {
                    resourceObject.Despawn();
                }
            }
        }
    }

    [ClientRpc(RequireOwnership = false)]
    public void PushObjectClientRpc(ulong resourceId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(resourceId, out NetworkObject resourceObject))
        {
            bool a = Managers.Pool.Push(resourceObject.gameObject);
            Debug.Log(a);
        }
    }
    void HandleResource(Collider collider) // ������ �ڿ� ����
    {
        if (!IsServer) return;

        ResourceData resourceData = collider.GetComponent<ResourceData>();
        if (resourceData == null || resourceData.CurrentOwner == Owner.Neutral) return;

        ulong resourceId = collider.GetComponent<NetworkObject>().NetworkObjectId;
        if (resourceData.CurrentOwner == Owner.Red) redTeamResourceCount.Value++;
        else if (resourceData.CurrentOwner == Owner.Blue) blueTeamResourceCount.Value++;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(resourceId, out NetworkObject resourceObject))
        {
            resourceObject.Despawn();
        }

        //HandleResourceServerRpc(resourceId, resourceData.CurrentOwner);
    }

    [ServerRpc(RequireOwnership = false)]
    void HandleResourceServerRpc(ulong resourceId, Owner owner) // ������ �ڿ� ���� ����RPC
    {
        if (owner == Owner.Red) redTeamResourceCount.Value++;
        else if (owner == Owner.Blue) blueTeamResourceCount.Value++;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(resourceId, out NetworkObject resourceObject))
        {
            resourceObject.Despawn();
        }

        //CheckOwnership();
        //UpdateVisuals();
    }

    void CheckOwnership() // ������ ������ �˻�
    {
        if (currentOwner == Owner.Neutral)
        {
            if (redTeamResourceCount.Value >= resourceFillCount)
                ChangeOwnershipServerRpc(Owner.Red);
            else if (blueTeamResourceCount.Value >= resourceFillCount)
                ChangeOwnershipServerRpc(Owner.Blue);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ChangeOwnershipServerRpc(Owner newOwner) // �߸� �������� �������� ������ ���� ����RPC
    {
        currentOwner = newOwner;
        GetComponent<Renderer>().material.color = (newOwner == Owner.Red) ? Color.red : Color.blue;

        InstantiateBuildingServerRpc(newOwner);
        ChangeOwnershipClientRpc(newOwner);
    }

    [ClientRpc]
    void ChangeOwnershipClientRpc(Owner newOwner) // �߸� �������� �������� ������ ���� Ŭ��RPC
    {
        if (IsServer) return;

        currentOwner = newOwner;
        GetComponent<Renderer>().material.color = (newOwner == Owner.Red) ? Color.red : Color.blue;
    }

    [ServerRpc(RequireOwnership = false)]
    void InstantiateBuildingServerRpc(Owner newOwner) // �ǹ� �Ǽ� ����RPC
    {
        ResetFillAmount();

        GameObject prefab = (newOwner == Owner.Red) ? occupyData.buildingPrefabTeamRed : occupyData.buildingPrefabTeamBlue;

        GameObject building = Instantiate(prefab, new Vector3(0f, -40f, 0f), Quaternion.Euler(-90f, 0f, 0f));
        building.transform.localPosition = new Vector3(0f, -40f, 0f);

        NetworkObject networkObject = building.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
            networkObject.TrySetParent(transform.GetComponent<NetworkObject>());
        }

        InstantiateBuildingClientRpc(newOwner);
    }

    [ClientRpc]
    void InstantiateBuildingClientRpc(Owner newOwner) // �ǹ� �Ǽ� Ŭ��RPC
    {
        ResetFillAmountClient();
    }

    void UpdateVisuals() // �ڿ� ���� �ð� ȿ��
    {
        float redFill = Mathf.Clamp((float)redTeamResourceCount.Value / resourceFillCount, 0f, 1f);
        float blueFill = Mathf.Clamp((float)blueTeamResourceCount.Value / resourceFillCount, 0f, 1f);

        redTeamResourceCountImage.fillAmount = redFill;
        blueTeamResourceCountImage.fillAmount = blueFill;
    }

    async void ResetFillAmount() // �ڿ� ���� �ð� ȿ�� �ʱ�ȭ
    {
        await Task.Delay(100);
        redTeamResourceCountImage.fillAmount = 0f;
        blueTeamResourceCountImage.fillAmount = 0f;
    }

    void ResetFillAmountClient() // �ڿ� ���� �ð� ȿ�� �ʱ�ȭ Ŭ���̾�Ʈ
    {
        if (redTeamResourceCountImage != null && blueTeamResourceCountImage != null)
        {
            ResetFillAmount();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetOwnershipServerRpc() // �ǹ� �ı� �� ������ �ʱ�ȭ ����RPC
    {
        ResetFillAmount();
        currentOwner = Owner.Neutral;
        GetComponent<Renderer>().material.color = new Color(0, 0, 0);

        redTeamResourceCount.Value = 0;
        blueTeamResourceCount.Value = 0;
    }
}
