using UnityEngine;

public class OccupySystem : MonoBehaviour
{
    /// TODO
    /// Ư�� �����κ��� ������ �ǹ��� ü���� ������ �ְ� (�ǹ� �����տ� �� ��ũ��Ʈ �ۼ�)
    /// �� �ǹ��� �ٸ� ������ �ı��� �� ����
    /// �ǹ��� �ı��� �������� �߸����� �����

    // �� ���� �������� ������ �ڿ� ī��Ʈ
    int redTeamResourceCount = 0;
    int blueTeamResourceCount = 0;

    // ä���� �� �ڿ� �ѷ�
    const int resourceFillCount = 3;
    
    // ������ �ʱ� ����
    Owner currentOwner = Owner.Neutral;

    // �ǹ� ������
    [SerializeField] GameObject redTeamBuildingPrefab;
    [SerializeField] GameObject blueTeamBuildingPrefab;

    void Update() => DetectResources();

    /// <summary>
    /// �ڿ� ���� �Լ� (������ �� �ڿ��� ����)
    /// </summary>
    void DetectResources()
    {
        // �߸� �������� �ƴϸ� �Լ� Ż��
        if (currentOwner != Owner.Neutral) return;

        // ������ ���� �� �ݶ��̴� ����
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
    /// �ڿ� ó�� �Լ� (������ �ڿ��� ����)
    /// </summary>
    void HandleResource(Collider collider)
    {
        Owner owner = collider.gameObject.GetComponent<ResourceData>().CurrentOwner;

        // �߸� �ڿ��̶�� �Լ� Ż��
        if (owner == Owner.Neutral) return;

        // ���� ����
        if (owner == Owner.Red) redTeamResourceCount++;
        else if (owner == Owner.Blue) blueTeamResourceCount++;
        Debug.Log($"{owner} �� �ڿ��� �������� ����");

        // ������ �ڿ��� �ܼ� �ı� (���� �������� �κ��� ���X)
        Destroy(collider.gameObject);
    }

    /// <summary>
    /// ������ ������ �˻� �Լ� (�߸� �������� �´��� Ȯ��)
    /// </summary>
    void CheckOwnership()
    {
        // �߸��� ���� ������ ����
        if (currentOwner == Owner.Neutral)
        {
            // �ڿ� ä����?
            if (redTeamResourceCount >= resourceFillCount) ChangeOwnership(Owner.Red);
            else if (blueTeamResourceCount >= resourceFillCount) ChangeOwnership(Owner.Blue);
        }
    }

    /// <summary>
    /// �߸� �������� ������ ���� �Լ� (���縦 ���� ���� �������� ����)
    /// </summary>
    void ChangeOwnership(Owner newOwner)
    {
        // ������ ���� �������� ����
        currentOwner = newOwner;
        Debug.Log($"{newOwner} ���� ������ ���� �Ϸ�");

        // ������ ������ �� ������ ����
        GetComponent<Renderer>().material.color = newOwner == Owner.Red ? Color.red : Color.blue;

        // �ǹ� ����
        GameObject buildingPrefab = newOwner == Owner.Red ? redTeamBuildingPrefab : blueTeamBuildingPrefab;
        Instantiate(buildingPrefab, transform.position, Quaternion.Euler(-90f, 0f, 0f));
    }

    /// <summary>
    /// ������ �ʱ�ȭ (�ǹ��� �ı����� �� �۵�)
    /// </summary>
    public void ResetOwnership()
    {
        // ������ �� ���� �ʱ�ȭ
        // �ǹ� �ı� ������ ������� �� �ۼ� ����
    }
}