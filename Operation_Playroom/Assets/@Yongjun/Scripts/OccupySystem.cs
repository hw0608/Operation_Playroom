using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class OccupySystem : MonoBehaviour
{
    /// TODO
    /// 특정 팀으로부터 지어진 건물은 체력을 가지고 있고 (건물 프리팹에 새 스크립트 작성)
    /// 그 건물은 다른 팀들이 파괴할 수 있음
    /// 건물이 파괴된 점령지는 중립으로 변경됨

    // 적재된 자원 카운트
    int redTeamResourceCount = 0;
    int blueTeamResourceCount = 0;

    // 채워야 할 자원 총량
    const int resourceFillCount = 3;
    
    // 점령지 초기 상태
    Owner currentOwner = Owner.Neutral;

    // 이미지
    [SerializeField] Image redTeamResourceCountImage;
    [SerializeField] Image blueTeamResourceCountImage;

    // 건물 프리팹
    [SerializeField] GameObject redTeamBuildingPrefab;
    [SerializeField] GameObject blueTeamBuildingPrefab;

    void Update() => DetectResources();

    void DetectResources() // 점령지 내 자원 감지 함수
    {
        if (currentOwner != Owner.Neutral) return;

        Collider[] colliders = Physics.OverlapSphere(transform.position, gameObject.transform.localScale.x / 2);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Resource"))
            {
                HandleResource(collider);
                CheckOwnership();
                UpdateVisuals();
            }
        }
    }

    void HandleResource(Collider collider) // 감지된 자원 적재 함수
    {
        Owner owner = collider.gameObject.GetComponent<ResourceData>().CurrentOwner;

        if (owner == Owner.Neutral) return;

        if (owner == Owner.Red) redTeamResourceCount++;
        else if (owner == Owner.Blue) blueTeamResourceCount++;

        Debug.Log($"{owner} 팀 자원이 점령지에 들어옴");

        Destroy(collider.gameObject);
    }

    void CheckOwnership() // 점령지 소유권 검사 함수
    {
        if (currentOwner == Owner.Neutral)
        {
            if (redTeamResourceCount >= resourceFillCount) ChangeOwnership(Owner.Red);
            else if (blueTeamResourceCount >= resourceFillCount) ChangeOwnership(Owner.Blue);
        }
    }

    void ChangeOwnership(Owner newOwner) // 중립 점령지를 적재를 완료한 팀의 소유권으로 변경하는 함수
    {
        currentOwner = newOwner;

        Debug.Log($"{newOwner} 팀이 점령지 점령 완료");

        GetComponent<Renderer>().material.color = newOwner == Owner.Red ? Color.red : Color.blue;

        InstantiateBuilding(newOwner);
    }

    void InstantiateBuilding(Owner newOwner) // 건물 건설 함수
    {
        ResetFillAmount();

        GameObject buildingPrefab = newOwner == Owner.Red ? redTeamBuildingPrefab : blueTeamBuildingPrefab;
        Instantiate(buildingPrefab, transform.position, Quaternion.Euler(-90f, 0f, 0f));
    }


    void UpdateVisuals() // 자원 적재 시각 효과 업데이트 함수
    {
        float redTeamFillAmount = Mathf.Clamp((float)redTeamResourceCount / resourceFillCount, 0f, 1f);
        float blueTeamFillAmount = Mathf.Clamp((float)blueTeamResourceCount / resourceFillCount, 0f, 1f);

        redTeamResourceCountImage.fillAmount = redTeamFillAmount;
        blueTeamResourceCountImage.fillAmount = blueTeamFillAmount;
    }

    async void ResetFillAmount() // 적재 시각 효과 초기화 함수
    {
        await Task.Delay(10);

        redTeamResourceCountImage.fillAmount = 0f;
        blueTeamResourceCountImage.fillAmount = 0f;
    }

    public void ResetOwnership() // 건물이 파괴됐을 때 소유권 초기화 함수
    {
        // 건물 파괴 로직이 만들어진 뒤 작성 예정

        // 소유권 초기화

        // 색상 초기화 
        
        ResetFillAmount(); // 적재 시각 효과 초기화
    }
}