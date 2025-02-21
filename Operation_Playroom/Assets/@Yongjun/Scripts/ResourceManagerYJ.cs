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

    // �ڿ� �߰� �Լ�
    public void AddResource(string teamName, int amount)
    {
        if (teamResources.ContainsKey(teamName))
        {
            teamResources[teamName] += amount;
            Debug.Log($"{teamName} �� �ڿ�: {teamResources[teamName]}��");
        }
    }

    // �ڿ� ���� ��������
    public int GetResource(string teamName)
    {
        if (teamResources.ContainsKey(teamName))
        {
            return teamResources[teamName];
        }
        return 0;
    }

    // �ڿ� �ʱ�ȭ �Լ�
    public void ResetResource(string teamName)
    {
        if (teamResources.ContainsKey(teamName))
        {
            teamResources[teamName] = 0;
            Debug.Log($"{teamName} �� �ڿ� �ʱ�ȭ");
        }
    }
}
