using System.Collections.Generic;
using UnityEngine;

public class ResourceManagerYJ : MonoBehaviour
{
    public static ResourceManagerYJ Instance;

    private Dictionary<string, int> teamResources = new Dictionary<string, int>
    {
        { "Red Team", 0 },
        { "Blue Team", 0 }
    };

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    // 자원 추가 함수
    public void AddResource(string teamName, int amount)
    {
        if (teamResources.ContainsKey(teamName))
        {
            teamResources[teamName] += amount;
            Debug.Log($"{teamName} 팀 자원: {teamResources[teamName]}개");
        }
    }

    // 자원 수량 가져오기
    public int GetResource(string teamName)
    {
        if (teamResources.ContainsKey(teamName))
        {
            return teamResources[teamName];
        }
        return 0;
    }

    // 자원 초기화 함수
    public void ResetResource(string teamName)
    {
        if (teamResources.ContainsKey(teamName))
        {
            teamResources[teamName] = 0;
            Debug.Log($"{teamName} 팀 자원 초기화");
        }
    }
}
