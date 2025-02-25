using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class OccupySystem : MonoBehaviour
{
    [SerializeField] int redTeamResourceCount = 0; // ������ �ڿ�
    [SerializeField] int blueTeamResourceCount = 0;
    const int resourceFillCount = 3; // ä���� �� �ڿ�
    Owner currentOwner = Owner.Neutral; // ������ �ʱ� ����
    [SerializeField] Image redTeamResourceCountImage; // �̹��� ��ġ
    [SerializeField] Image blueTeamResourceCountImage;
    [SerializeField] OccupyScriptableObject occupyData; // ������ ������ ��ũ���ͺ� ������Ʈ

    void Update() => DetectResources();

    void DetectResources() // ������ �� �ڿ� ����
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

    void HandleResource(Collider collider) // ������ �ڿ� ����
    {
        Owner owner = collider.gameObject.GetComponent<ResourceData>().CurrentOwner;

        if (owner == Owner.Neutral) return;

        if (owner == Owner.Red) redTeamResourceCount++;
        else if (owner == Owner.Blue) blueTeamResourceCount++;

        Destroy(collider.gameObject);
    }

    void CheckOwnership() // ������ ������ �˻�
    {
        if (currentOwner == Owner.Neutral)
        {
            if (redTeamResourceCount >= resourceFillCount) ChangeOwnership(Owner.Red);
            else if (blueTeamResourceCount >= resourceFillCount) ChangeOwnership(Owner.Blue);
        }
    }

    void ChangeOwnership(Owner newOwner) // �߸� �������� �������� ������ ����
    {
        currentOwner = newOwner;

        GetComponent<Renderer>().material.color = newOwner == Owner.Red ? Color.red : Color.blue;

        InstantiateBuilding(newOwner);
    }

    void InstantiateBuilding(Owner newOwner) // �ǹ� �Ǽ�
    {
        ResetFillAmount();

        GameObject buildingPrefab = newOwner == Owner.Red ? occupyData.buildingPrefabTeamRed : occupyData.buildingPrefabTeamBlue;
        GameObject buildingInstance = Instantiate(buildingPrefab, new Vector3(transform.position.x, -0.3f, transform.position.z), Quaternion.Euler(-90f, 0f, 0f));
        buildingInstance.transform.SetParent(transform);
    }

    void UpdateVisuals() // �ڿ� ���� �ð� ȿ��
    {
        float redTeamFillAmount = Mathf.Clamp((float)redTeamResourceCount / resourceFillCount, 0f, 1f);
        float blueTeamFillAmount = Mathf.Clamp((float)blueTeamResourceCount / resourceFillCount, 0f, 1f);

        redTeamResourceCountImage.fillAmount = redTeamFillAmount;
        blueTeamResourceCountImage.fillAmount = blueTeamFillAmount;
    }

    async void ResetFillAmount() // �ڿ� ���� �ð� ȿ�� �ʱ�ȭ
    {
        await Task.Delay(10);

        redTeamResourceCountImage.fillAmount = 0f;
        blueTeamResourceCountImage.fillAmount = 0f;
    }

    public void ResetOwnership() // �ǹ� �ı� �� ������ �ʱ�ȭ
    {
        ResetFillAmount();

        currentOwner = Owner.Neutral;

        GetComponent<Renderer>().material.color = new Color(0, 0, 0);

        redTeamResourceCount = 0;
        blueTeamResourceCount = 0;
    }
}