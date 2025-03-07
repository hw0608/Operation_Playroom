using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using Unity.Netcode;

public class OccupySystem : NetworkBehaviour
{
    // 적재한 자원
    [SerializeField] NetworkVariable<int> redTeamResourceCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] NetworkVariable<int> blueTeamResourceCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // 채워야 할 자원
    const int resourceFillCount = 3;

    // 점령지 초기 상태
    Owner currentOwner = Owner.Neutral;

    // 이미지 위치
    [SerializeField] Image redTeamResourceCountImage;
    [SerializeField] Image blueTeamResourceCountImage;

    // 점령지 데이터 스크립터블 오브젝트
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

    void DetectResources() // 점령지 내 자원 감지
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
    void HandleResource(Collider collider) // 감지된 자원 적재
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
    void HandleResourceServerRpc(ulong resourceId, Owner owner) // 감지된 자원 적재 서버RPC
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

    void CheckOwnership() // 점령지 소유권 검사
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
    void ChangeOwnershipServerRpc(Owner newOwner) // 중립 점령지의 소유권을 팀으로 변경 서버RPC
    {
        currentOwner = newOwner;
        GetComponent<Renderer>().material.color = (newOwner == Owner.Red) ? Color.red : Color.blue;

        InstantiateBuildingServerRpc(newOwner);
        ChangeOwnershipClientRpc(newOwner);
    }

    [ClientRpc]
    void ChangeOwnershipClientRpc(Owner newOwner) // 중립 점령지의 소유권을 팀으로 변경 클라RPC
    {
        if (IsServer) return;

        currentOwner = newOwner;
        GetComponent<Renderer>().material.color = (newOwner == Owner.Red) ? Color.red : Color.blue;
    }

    [ServerRpc(RequireOwnership = false)]
    void InstantiateBuildingServerRpc(Owner newOwner) // 건물 건설 서버RPC
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
    void InstantiateBuildingClientRpc(Owner newOwner) // 건물 건설 클라RPC
    {
        ResetFillAmountClient();
    }

    void UpdateVisuals() // 자원 적재 시각 효과
    {
        float redFill = Mathf.Clamp((float)redTeamResourceCount.Value / resourceFillCount, 0f, 1f);
        float blueFill = Mathf.Clamp((float)blueTeamResourceCount.Value / resourceFillCount, 0f, 1f);

        redTeamResourceCountImage.fillAmount = redFill;
        blueTeamResourceCountImage.fillAmount = blueFill;
    }

    async void ResetFillAmount() // 자원 적재 시각 효과 초기화
    {
        await Task.Delay(100);
        redTeamResourceCountImage.fillAmount = 0f;
        blueTeamResourceCountImage.fillAmount = 0f;
    }

    void ResetFillAmountClient() // 자원 적재 시각 효과 초기화 클라이언트
    {
        if (redTeamResourceCountImage != null && blueTeamResourceCountImage != null)
        {
            ResetFillAmount();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetOwnershipServerRpc() // 건물 파괴 시 소유권 초기화 서버RPC
    {
        ResetFillAmount();
        currentOwner = Owner.Neutral;
        GetComponent<Renderer>().material.color = new Color(0, 0, 0);

        redTeamResourceCount.Value = 0;
        blueTeamResourceCount.Value = 0;
    }
}
