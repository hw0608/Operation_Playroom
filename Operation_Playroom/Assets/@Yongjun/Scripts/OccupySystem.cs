using UnityEngine;

public class OccupySystem : MonoBehaviour
{
    /// <summary>
    /// ���� �� �÷��̾� ���� �� �ش� �÷��̾��� �ڿ� ������ �������� �� ������ ���� (�ִ� 3��)
    /// �� �������� �ε����� �ϳ��� ���� �ش� �������� �ڿ� ������ �� �������� �ڿ��� 3���� ���� �ǹ� �Ǽ�
    /// ��, �� �������� �������� ������� ���� �ڿ��� ���� ����
    /// (ex. 1�� �������� �������� �ڿ� 2��, ������� �ڿ� 3���� ������, ������� �ǹ��� ������)
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
                Debug.Log($"{collider.name} ����");

                // �÷��̾��� �� ���� ��������
                string whatTeam = collider.gameObject.GetComponent<TestTeam>().team;
                
                if (ResourceManagerYJ.Instance.GetResource(whatTeam) >= 3)
                {
                    Build(whatTeam);
                }
                else
                {
                    Debug.Log("�ڿ� ����");
                }
            }
        }
    }

    // �ǹ� �Ǽ� �Լ�
    void Build(string teamName)
    {
        Debug.Log($"{teamName} ���� �ǹ��� �Ǽ��Ǿ����ϴ�");
        Debug.Log($"���� �ڿ� : {ResourceManagerYJ.Instance.GetResource(teamName)}��");

        // �ǹ� ���� ���� �߰�
        ResourceManagerYJ.Instance.ResetResource(teamName);
        Debug.Log("�ڿ� �ʱ�ȭ");
    }
}