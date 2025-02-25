using System.Collections.Generic;
using UnityEngine;

public class TestItemCollection : MonoBehaviour
{
    public static TestItemCollection Instance;

    private Dictionary<string, int> resources = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // �ڿ� �߰�
    public void AddResource(string resourceName, int amount)
    {
        if (resources.ContainsKey(resourceName))
        {
            resources[resourceName] += amount;
        }
        else
        {
            resources.Add(resourceName, amount);
        }
        Debug.Log($"[ItemCollection] {resourceName} �߰�: {amount}, �ѷ�: {resources[resourceName]}");
    }

    // �ڿ� ���� Ȯ��
    public int GetResourceAmount(string resourceName)
    {
        if (resources.ContainsKey(resourceName))
        {
            return resources[resourceName];
        }
        return 0;
    }
}


