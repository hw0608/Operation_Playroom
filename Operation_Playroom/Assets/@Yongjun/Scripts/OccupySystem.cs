using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class OccupySystem : NetworkBehaviour
{
    [SerializeField] NetworkVariable<int> redTeamResourceCount = new NetworkVariable<int>(0); // ������ �ڿ�
    [SerializeField] NetworkVariable<int> blueTeamResourceCount = new NetworkVariable<int>(0);
    const int resourceFillCount = 3; // ä���� �� �ڿ�
    Owner currentOwner = Owner.Neutral; // ������ �ʱ� ����
    [SerializeField] Image redTeamResourceCountImage; // �̹��� ��ġ
    [SerializeField] Image blueTeamResourceCountImage;
    [SerializeField] OccupyScriptableObject occupyData; // ������ ������ ��ũ���ͺ� ������Ʈ


    void Update()
    {
        if (IsServer || IsClient)
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

        Collider[] colliders = Physics.OverlapSphere(transform.position, gameObject.transform.localScale.x / 2);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Resource"))
            {

                ulong resourceId = collider.GetComponent<NetworkObject>().NetworkObjectId;
                Owner owner = collider.GetComponent<ResourceData>().CurrentOwner;
                DetectResourceServerRpc(resourceId, owner);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void DetectResourceServerRpc(ulong resourceId, Owner owner)
    {
        if (currentOwner != Owner.Neutral) return;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(resourceId, out NetworkObject resourceObject))
        {
            ResourceData resourceData = resourceObject.GetComponent<ResourceData>();
            if (resourceData != null && resourceData.CurrentOwner == owner)
            {
                HandleResource(resourceObject.GetComponent<Collider>());
            }
        }
    }

    void HandleResource(Collider collider) // ������ �ڿ� ����
    {
        Owner owner = collider.gameObject.GetComponent<ResourceData>().CurrentOwner;

        if (owner == Owner.Neutral) return;

        if (owner == Owner.Red) redTeamResourceCount.Value++;
        else if (owner == Owner.Blue) blueTeamResourceCount.Value++;

        Destroy(collider.gameObject);
    }

    void CheckOwnership() // ������ ������ �˻�
    {
        if (currentOwner == Owner.Neutral)
        {
            if (redTeamResourceCount.Value >= resourceFillCount) ChangeOwnership(Owner.Red);
            else if (blueTeamResourceCount.Value >= resourceFillCount) ChangeOwnership(Owner.Blue);
        }
    }

    void ChangeOwnership(Owner newOwner) // �߸� �������� �������� ������ ����
    {
        currentOwner = newOwner;

        GetComponent<Renderer>().material.color = newOwner == Owner.Red ? Color.red : Color.blue;

        InstantiateBuilding(newOwner);
    }

    void InstantiateBuilding(Owner newOwner) // �ǹ� �Ǽ�
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

        //InstantiateBuildingClientRpc(newOwner);
    }

    void UpdateVisuals() // �ڿ� ���� �ð� ȿ��
    {
        float redTeamFillAmount = Mathf.Clamp((float)redTeamResourceCount.Value / resourceFillCount, 0f, 1f);
        float blueTeamFillAmount = Mathf.Clamp((float)blueTeamResourceCount.Value / resourceFillCount, 0f, 1f);

        redTeamResourceCountImage.fillAmount = redTeamFillAmount;
        blueTeamResourceCountImage.fillAmount = blueTeamFillAmount;
    }

    async void ResetFillAmount() // �ڿ� ���� �ð� ȿ�� �ʱ�ȭ
    {
        await Task.Delay(10);

        redTeamResourceCountImage.fillAmount = 0f;
        blueTeamResourceCountImage.fillAmount = 0f;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetOwnershipServerRpc() // �ǹ� �ı� �� ������ �ʱ�ȭ
    {
        ResetFillAmount();

        currentOwner = Owner.Neutral;

        GetComponent<Renderer>().material.color = new Color(0, 0, 0);

        redTeamResourceCount.Value = 0;
        blueTeamResourceCount.Value = 0;
    }
}