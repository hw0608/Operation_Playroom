using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using Unity.Netcode;

public class OccupySystem : NetworkBehaviour
{
    [SerializeField]
    NetworkVariable<int> redTeamResourceCount = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField]
    NetworkVariable<int> blueTeamResourceCount = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    const int resourceFillCount = 3; // ���ɿ� �ʿ��� �ڿ� ����
    Owner currentOwner = Owner.Neutral; // ���� ���� ����

    [SerializeField] Image redTeamResourceCountImage;
    [SerializeField] Image blueTeamResourceCountImage;
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

    // �������� �ڿ��� ���Դ��� üũ
    void DetectResources()
    {
        if (currentOwner != Owner.Neutral) return;

        Collider[] colliders = Physics.OverlapSphere(transform.position, transform.localScale.x / 2);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Resource"))
            {
                HandleResource(collider);
            }
        }
    }

    // ������ �ڿ��� ��� �ұ�?
    void HandleResource(Collider collider)
    {
        if (!IsServer) return;

        ResourceData resourceData = collider.GetComponent<ResourceData>();
        if (resourceData == null || resourceData.CurrentOwner == Owner.Neutral) return;

        ulong resourceId = collider.GetComponent<NetworkObject>().NetworkObjectId;
        HandleResourceServerRpc(resourceId, resourceData.CurrentOwner);
    }

    // � ���� �ڿ��� �����ߴ��� Ȯ��
    [ServerRpc(RequireOwnership = false)]
    void HandleResourceServerRpc(ulong resourceId, Owner owner)
    {
        if (owner == Owner.Red) redTeamResourceCount.Value++;
        else if (owner == Owner.Blue) blueTeamResourceCount.Value++;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(resourceId, out NetworkObject resourceObject))
        {
            resourceObject.Despawn();
        }

        CheckOwnership(); // Ȥ�� �ڿ��� ��á���� ��� Ȯ��
        UpdateVisuals(); // ����� ������ �ð�ȿ�� ������Ʈ
    }

    // �ڿ��� ��á���� Ȯ���ϴ� �Լ�
    void CheckOwnership()
    {
        if (currentOwner == Owner.Neutral)
        {
            if (redTeamResourceCount.Value >= resourceFillCount)
                ChangeOwnershipServerRpc(Owner.Red);
            else if (blueTeamResourceCount.Value >= resourceFillCount)
                ChangeOwnershipServerRpc(Owner.Blue);
        }
    }

    // �ڿ��� ��á���� �� ���� �������� ����
    [ServerRpc(RequireOwnership = false)]
    void ChangeOwnershipServerRpc(Owner newOwner)
    {
        currentOwner = newOwner;
        GetComponent<Renderer>().material.color = (newOwner == Owner.Red) ? Color.red : Color.blue;

        InstantiateBuildingServerRpc(newOwner); // �׸��� �ǹ��� ����
        ChangeOwnershipClientRpc(newOwner);
    }

    // �ڿ��� ��á���� �� ���� �������� ����
    [ClientRpc]
    void ChangeOwnershipClientRpc(Owner newOwner)
    {
        if (IsServer) return;

        currentOwner = newOwner;
        GetComponent<Renderer>().material.color = (newOwner == Owner.Red) ? Color.red : Color.blue;
    }

    // �ǹ��� �����ϴ� �Լ�
    [ServerRpc(RequireOwnership = false)]
    void InstantiateBuildingServerRpc(Owner newOwner)
    {
        ResetFillAmount();

        GameObject prefab = (newOwner == Owner.Red) ? occupyData.buildingPrefabTeamRed : occupyData.buildingPrefabTeamBlue;
        GameObject building = Instantiate(prefab, new Vector3(transform.position.x, -0.3f, transform.position.z), Quaternion.Euler(-90f, 0f, 0f));

        NetworkObject networkObject = building.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
        }

        InstantiateBuildingClientRpc(newOwner);
    }

    // �ǹ��� �����ϴ� �Լ�
    [ClientRpc]
    void InstantiateBuildingClientRpc(Owner newOwner)
    {
        if (IsServer) return;
        ResetFillAmount();
    }

    // fillAmount ������Ʈ
    void UpdateVisuals()
    {
        float redFill = Mathf.Clamp((float)redTeamResourceCount.Value / resourceFillCount, 0f, 1f);
        float blueFill = Mathf.Clamp((float)blueTeamResourceCount.Value / resourceFillCount, 0f, 1f);

        redTeamResourceCountImage.fillAmount = redFill;
        blueTeamResourceCountImage.fillAmount = blueFill;
    }

    // fillAmount �ʱ�ȭ
    async void ResetFillAmount()
    {
        await Task.Delay(10);
        redTeamResourceCountImage.fillAmount = 0f;
        blueTeamResourceCountImage.fillAmount = 0f;
    }

    // ������ �ʱ�ȭ
    [ServerRpc(RequireOwnership = false)]
    public void ResetOwnershipServerRpc()
    {
        ResetFillAmount();
        currentOwner = Owner.Neutral;
        GetComponent<Renderer>().material.color = new Color(0, 0, 0);

        redTeamResourceCount.Value = 0;
        blueTeamResourceCount.Value = 0;
    }
}
