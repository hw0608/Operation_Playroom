using UnityEngine;

public class OccupySystem : MonoBehaviour
{
    /// TODO
    /// 특정 팀으로부터 지어진 건물은 체력을 가지고 있고 (건물 프리팹에 새 스크립트 작성)
    /// 그 건물은 다른 팀들이 파괴할 수 있음
    /// 건물이 파괴된 점령지는 중립으로 변경됨

    // 각 팀이 점령지에 적재한 자원 카운트
    int redTeamResourceCount = 0;
    int blueTeamResourceCount = 0;

    // 채워야 할 자원 총량
    const int resourceFillCount = 3;
    
    // 점령지 초기 상태
    Owner currentOwner = Owner.Neutral;

    // 건물 프리팹
    [SerializeField] GameObject redTeamBuildingPrefab;
    [SerializeField] GameObject blueTeamBuildingPrefab;

    void Update() => DetectResources();

    /// <summary>
    /// 자원 감지 함수 (점령지 내 자원을 감지)
    /// </summary>
    void DetectResources()
    {
        // 중립 점령지가 아니면 함수 탈출
        if (currentOwner != Owner.Neutral) return;

        // 점령지 범위 내 콜라이더 감지
        Collider[] colliders = Physics.OverlapSphere(transform.position, gameObject.transform.localScale.x / 2);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Resource"))
            {
                HandleResource(collider);
                CheckOwnership();
            }
        }
    }

    /// <summary>
    /// 자원 처리 함수 (감지된 자원을 적재)
    /// </summary>
    void HandleResource(Collider collider)
    {
        Owner owner = collider.gameObject.GetComponent<ResourceData>().CurrentOwner;

        // 중립 자원이라면 함수 탈출
        if (owner == Owner.Neutral) return;

        // 적재 성공
        if (owner == Owner.Red) redTeamResourceCount++;
        else if (owner == Owner.Blue) blueTeamResourceCount++;
        Debug.Log($"{owner} 팀 자원이 점령지에 들어옴");

        // 적재한 자원은 단순 파괴 (아직 가시적인 부분은 고려X)
        Destroy(collider.gameObject);
    }

    /// <summary>
    /// 점령지 소유권 검사 함수 (중립 점령지가 맞는지 확인)
    /// </summary>
    void CheckOwnership()
    {
        // 중립일 때만 소유권 변경
        if (currentOwner == Owner.Neutral)
        {
            // 자원 채웠니?
            if (redTeamResourceCount >= resourceFillCount) ChangeOwnership(Owner.Red);
            else if (blueTeamResourceCount >= resourceFillCount) ChangeOwnership(Owner.Blue);
        }
    }

    /// <summary>
    /// 중립 점령지의 소유권 변경 함수 (적재를 끝낸 팀의 점령지로 지정)
    /// </summary>
    void ChangeOwnership(Owner newOwner)
    {
        // 점령한 팀의 소유지로 변경
        currentOwner = newOwner;
        Debug.Log($"{newOwner} 팀이 점령지 점령 완료");

        // 점령지 색상을 팀 색으로 변경
        GetComponent<Renderer>().material.color = newOwner == Owner.Red ? Color.red : Color.blue;

        // 건물 짓기
        GameObject buildingPrefab = newOwner == Owner.Red ? redTeamBuildingPrefab : blueTeamBuildingPrefab;
        Instantiate(buildingPrefab, transform.position, Quaternion.Euler(-90f, 0f, 0f));
    }

    /// <summary>
    /// 소유권 초기화 (건물이 파괴됐을 때 작동)
    /// </summary>
    public void ResetOwnership()
    {
        // 소유권 및 색상 초기화
        // 건물 파괴 로직이 만들어진 뒤 작성 예정
    }
}