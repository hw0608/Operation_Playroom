using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class OccupySystem : MonoBehaviour
{
    /// TODO
    /// Ư�� �����κ��� ������ �ǹ��� ü���� ������ �ְ� (�ǹ� �����տ� �� ��ũ��Ʈ �ۼ�)
    /// �� �ǹ��� �ٸ� ������ �ı��� �� ����
    /// �ǹ��� �ı��� �������� �߸����� �����

    // ����� �ڿ� ī��Ʈ
    int redTeamResourceCount = 0;
    int blueTeamResourceCount = 0;

    // ä���� �� �ڿ� �ѷ�
    const int resourceFillCount = 3;
    
    // ������ �ʱ� ����
    Owner currentOwner = Owner.Neutral;

    // �̹���
    [SerializeField] Image redTeamResourceCountImage;
    [SerializeField] Image blueTeamResourceCountImage;

    // �ǹ� ������
    [SerializeField] GameObject redTeamBuildingPrefab;
    [SerializeField] GameObject blueTeamBuildingPrefab;

    void Update() => DetectResources();

    void DetectResources() // ������ �� �ڿ� ���� �Լ�
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

    void HandleResource(Collider collider) // ������ �ڿ� ���� �Լ�
    {
        Owner owner = collider.gameObject.GetComponent<ResourceData>().CurrentOwner;

        if (owner == Owner.Neutral) return;

        if (owner == Owner.Red) redTeamResourceCount++;
        else if (owner == Owner.Blue) blueTeamResourceCount++;

        Debug.Log($"{owner} �� �ڿ��� �������� ����");

        Destroy(collider.gameObject);
    }

    void CheckOwnership() // ������ ������ �˻� �Լ�
    {
        if (currentOwner == Owner.Neutral)
        {
            if (redTeamResourceCount >= resourceFillCount) ChangeOwnership(Owner.Red);
            else if (blueTeamResourceCount >= resourceFillCount) ChangeOwnership(Owner.Blue);
        }
    }

    void ChangeOwnership(Owner newOwner) // �߸� �������� ���縦 �Ϸ��� ���� ���������� �����ϴ� �Լ�
    {
        currentOwner = newOwner;

        Debug.Log($"{newOwner} ���� ������ ���� �Ϸ�");

        GetComponent<Renderer>().material.color = newOwner == Owner.Red ? Color.red : Color.blue;

        InstantiateBuilding(newOwner);
    }

    void InstantiateBuilding(Owner newOwner) // �ǹ� �Ǽ� �Լ�
    {
        ResetFillAmount();

        GameObject buildingPrefab = newOwner == Owner.Red ? redTeamBuildingPrefab : blueTeamBuildingPrefab;
        Instantiate(buildingPrefab, transform.position, Quaternion.Euler(-90f, 0f, 0f));
    }


    void UpdateVisuals() // �ڿ� ���� �ð� ȿ�� ������Ʈ �Լ�
    {
        float redTeamFillAmount = Mathf.Clamp((float)redTeamResourceCount / resourceFillCount, 0f, 1f);
        float blueTeamFillAmount = Mathf.Clamp((float)blueTeamResourceCount / resourceFillCount, 0f, 1f);

        redTeamResourceCountImage.fillAmount = redTeamFillAmount;
        blueTeamResourceCountImage.fillAmount = blueTeamFillAmount;
    }

    async void ResetFillAmount() // ���� �ð� ȿ�� �ʱ�ȭ �Լ�
    {
        await Task.Delay(10);

        redTeamResourceCountImage.fillAmount = 0f;
        blueTeamResourceCountImage.fillAmount = 0f;
    }

    public void ResetOwnership() // �ǹ��� �ı����� �� ������ �ʱ�ȭ �Լ�
    {
        // �ǹ� �ı� ������ ������� �� �ۼ� ����

        // ������ �ʱ�ȭ

        // ���� �ʱ�ȭ 
        
        ResetFillAmount(); // ���� �ð� ȿ�� �ʱ�ȭ
    }
}