using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class OccupySystem : MonoBehaviour
{
    [SerializeField] int redTeamResourceCount = 0; // 적재한 자원
    [SerializeField] int blueTeamResourceCount = 0;
    const int resourceFillCount = 3; // 채워야 할 자원
    Owner currentOwner = Owner.Neutral; // 점령지 초기 상태
    [SerializeField] Image redTeamResourceCountImage; // 이미지 위치
    [SerializeField] Image blueTeamResourceCountImage;
    [SerializeField] OccupyScriptableObject occupyData; // 점령지 데이터 스크립터블 오브젝트

<<<<<<< HEAD
    void Update() => DetectResources();
=======
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
>>>>>>> yj

    void DetectResources() // 점령지 내 자원 감지
    {
        if (currentOwner != Owner.Neutral) return;

        Collider[] colliders = Physics.OverlapSphere(transform.position, gameObject.transform.localScale.x / 2);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Resource"))
            {
<<<<<<< HEAD
                HandleResource(collider);
                CheckOwnership();
                UpdateVisuals();
=======
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
>>>>>>> yj
            }
        }
    }

    void HandleResource(Collider collider) // 감지된 자원 적재
    {
        Owner owner = collider.gameObject.GetComponent<ResourceData>().CurrentOwner;

        if (owner == Owner.Neutral) return;

        if (owner == Owner.Red) redTeamResourceCount++;
        else if (owner == Owner.Blue) blueTeamResourceCount++;

        Destroy(collider.gameObject);
    }

    void CheckOwnership() // 점령지 소유권 검사
    {
        if (currentOwner == Owner.Neutral)
        {
            if (redTeamResourceCount >= resourceFillCount) ChangeOwnership(Owner.Red);
            else if (blueTeamResourceCount >= resourceFillCount) ChangeOwnership(Owner.Blue);
        }
    }

    void ChangeOwnership(Owner newOwner) // 중립 점령지의 소유권을 팀으로 변경
    {
        currentOwner = newOwner;

        GetComponent<Renderer>().material.color = newOwner == Owner.Red ? Color.red : Color.blue;

        InstantiateBuilding(newOwner);
    }

    void InstantiateBuilding(Owner newOwner) // 건물 건설
    {
        ResetFillAmount();

<<<<<<< HEAD
        GameObject buildingPrefab = newOwner == Owner.Red ? occupyData.buildingPrefabTeamRed : occupyData.buildingPrefabTeamBlue;
        GameObject buildingInstance = Instantiate(buildingPrefab, new Vector3(transform.position.x, -0.3f, transform.position.z), Quaternion.Euler(-90f, 0f, 0f));
        buildingInstance.transform.SetParent(transform);
=======
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
>>>>>>> yj
    }

    void UpdateVisuals() // 자원 적재 시각 효과
    {
        float redTeamFillAmount = Mathf.Clamp((float)redTeamResourceCount / resourceFillCount, 0f, 1f);
        float blueTeamFillAmount = Mathf.Clamp((float)blueTeamResourceCount / resourceFillCount, 0f, 1f);

        redTeamResourceCountImage.fillAmount = redTeamFillAmount;
        blueTeamResourceCountImage.fillAmount = blueTeamFillAmount;
    }

    async void ResetFillAmount() // 자원 적재 시각 효과 초기화
    {
        await Task.Delay(10);

        redTeamResourceCountImage.fillAmount = 0f;
        blueTeamResourceCountImage.fillAmount = 0f;
    }

    public void ResetOwnership() // 건물 파괴 시 소유권 초기화
    {
        ResetFillAmount();

        currentOwner = Owner.Neutral;

        GetComponent<Renderer>().material.color = new Color(0, 0, 0);

        redTeamResourceCount = 0;
        blueTeamResourceCount = 0;
    }
}