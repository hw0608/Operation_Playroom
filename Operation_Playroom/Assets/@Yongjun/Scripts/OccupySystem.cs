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

    const int resourceFillCount = 3; // 점령에 필요한 자원 개수
    Owner currentOwner = Owner.Neutral; // 현재 점령 상태

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

    // 점령지에 자원이 들어왔는지 체크
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

    // 감지된 자원을 어떻게 할까?
    void HandleResource(Collider collider)
    {
        if (!IsServer) return;

        ResourceData resourceData = collider.GetComponent<ResourceData>();
        if (resourceData == null || resourceData.CurrentOwner == Owner.Neutral) return;

        ulong resourceId = collider.GetComponent<NetworkObject>().NetworkObjectId;
        HandleResourceServerRpc(resourceId, resourceData.CurrentOwner);
    }

    // 어떤 팀이 자원을 적재했는지 확인
    [ServerRpc(RequireOwnership = false)]
    void HandleResourceServerRpc(ulong resourceId, Owner owner)
    {
        if (owner == Owner.Red) redTeamResourceCount.Value++;
        else if (owner == Owner.Blue) blueTeamResourceCount.Value++;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(resourceId, out NetworkObject resourceObject))
        {
            resourceObject.Despawn();
        }

        CheckOwnership(); // 혹시 자원이 꽉찼는지 계속 확인
        UpdateVisuals(); // 적재될 때마다 시각효과 업데이트
    }

    // 자원이 꽉찼는지 확인하는 함수
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

    // 자원이 꽉찼으면 그 팀의 점령지로 변경
    [ServerRpc(RequireOwnership = false)]
    void ChangeOwnershipServerRpc(Owner newOwner)
    {
        currentOwner = newOwner;
        GetComponent<Renderer>().material.color = (newOwner == Owner.Red) ? Color.red : Color.blue;

        InstantiateBuildingServerRpc(newOwner); // 그리고 건물을 생성
        ChangeOwnershipClientRpc(newOwner);
    }

    // 자원이 꽉찼으면 그 팀의 점령지로 변경
    [ClientRpc]
    void ChangeOwnershipClientRpc(Owner newOwner)
    {
        if (IsServer) return;

        currentOwner = newOwner;
        GetComponent<Renderer>().material.color = (newOwner == Owner.Red) ? Color.red : Color.blue;
    }

    // 건물을 생성하는 함수
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

    // 건물을 생성하는 함수
    [ClientRpc]
    void InstantiateBuildingClientRpc(Owner newOwner)
    {
        if (IsServer) return;
        ResetFillAmount();
    }

    // fillAmount 업데이트
    void UpdateVisuals()
    {
        float redFill = Mathf.Clamp((float)redTeamResourceCount.Value / resourceFillCount, 0f, 1f);
        float blueFill = Mathf.Clamp((float)blueTeamResourceCount.Value / resourceFillCount, 0f, 1f);

        redTeamResourceCountImage.fillAmount = redFill;
        blueTeamResourceCountImage.fillAmount = blueFill;
    }

    // fillAmount 초기화
    async void ResetFillAmount()
    {
        await Task.Delay(10);
        redTeamResourceCountImage.fillAmount = 0f;
        blueTeamResourceCountImage.fillAmount = 0f;
    }

    // 점령지 초기화
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
