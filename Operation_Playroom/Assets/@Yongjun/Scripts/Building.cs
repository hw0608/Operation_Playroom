using UnityEngine;
using System.Collections;

public class Building : MonoBehaviour
{
    [SerializeField] int health; // �ǹ� ü��
    [SerializeField] BuildingScriptableObject buildingData; // �ǹ� ������ ��ũ���ͺ� ������Ʈ
    bool isDestruction = false; // �ߺ�ö�� ����

    void OnEnable() => StartCoroutine(RaiseBuilding(gameObject, 0.2f, 3f));

    void Start()
    {
        health = buildingData.health;
    }

    void Update()
    {
        if (health <= 0 && !isDestruction)
        {
            isDestruction = true;
            DestructionBuilding();
        }
    }

    IEnumerator RaiseBuilding(GameObject building, float targetPosY, float duration) // �ǹ� ������ �� ȿ��
    {
        float elapsedTime = 0f;
        Vector3 startPos = building.transform.position;
        Vector3 targetPos = new Vector3(startPos.x, targetPosY, startPos.z);

        GameObject buildEffect = Instantiate(buildingData.buildEffect, new Vector3(transform.position.x, 0.2f, transform.position.z), Quaternion.identity);

        while (elapsedTime < duration)
        {
            building.transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        building.transform.position = targetPos;
        
        Destroy(buildEffect);

        GameObject sparkleEffect = Instantiate(buildingData.sparkleEffect, new Vector3(transform.position.x, 0.5f, transform.position.z), Quaternion.identity);
        yield return new WaitForSeconds(0.5f);
        Destroy(sparkleEffect);
    }

    void DestructionBuilding() // �ǹ� �ı�
    {
        GetComponentInParent<OccupySystem>().ResetOwnership();
        gameObject.SetActive(false);
        GameObject destructionEffect = Instantiate(buildingData.destructionEffect, transform.position, Quaternion.identity);
        Destroy(destructionEffect, 3.25f);
        Destroy(gameObject, 3.25f);
    }
}
