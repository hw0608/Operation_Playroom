using UnityEngine;

public class OccupySystem : MonoBehaviour
{
    /// <summary>
    /// 범위 내 플레이어 검출 → 해당 플레이어의 자원 데이터 가져오기 → 변수에 저장 (최대 3개)
    /// → 변수에서 인덱스를 하나씩 빼서 해당 점령지에 자원 보내기 → 점령지에 자원이 3개가 차면 건물 건설
    /// 단, 한 점령지에 레드팀과 블루팀이 보낸 자원은 따로 계산됨
    /// (ex. 1번 점령지에 레드팀이 자원 2개, 블루팀이 자원 3개를 넣으면, 블루팀의 건물이 생성됨)
    /// </summary>
    
    void Update()
    {
        SearchResources();
    }

    void SearchResources()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.35f);

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                Debug.Log($"{collider.name} 검출");

                // 플레이어의 팀 정보 가져오기
                string whatTeam = collider.gameObject.GetComponent<TestTeam>().team;
                
                if (ResourceManagerYJ.Instance.GetResource(whatTeam) >= 3)
                {
                    Build(whatTeam);
                }
                else
                {
                    Debug.Log("자원 부족");
                }
            }
        }
    }

    // 건물 건설 함수
    void Build(string teamName)
    {
        Debug.Log($"{teamName} 팀의 건물이 건설되었습니다");
        Debug.Log($"남은 자원 : {ResourceManagerYJ.Instance.GetResource(teamName)}개");

        // 건물 생성 로직 추가
        ResourceManagerYJ.Instance.ResetResource(teamName);
        Debug.Log("자원 초기화");
    }
}