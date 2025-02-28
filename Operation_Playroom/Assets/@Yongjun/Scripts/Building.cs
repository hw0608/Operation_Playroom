using UnityEngine;
using System.Collections;

public class Building : MonoBehaviour
{
    [SerializeField] int health; // 건물 체력
    [SerializeField] BuildingScriptableObject buildingData; // 건물 데이터 스크립터블 오브젝트
    bool isDestruction = false; // 중복철거 방지

<<<<<<< HEAD
    void OnEnable() => StartCoroutine(RaiseBuilding(gameObject, 0.2f, 3f));
=======
    [SerializeField] BuildingScriptableObject buildingData;
    [SerializeField] EffectScriptableObject effectData;
>>>>>>> yj

    void Start()
    {
<<<<<<< HEAD
        health = buildingData.health;
=======
        if (IsServer)
        {
            health.Value = buildingData.health;
        }

        if (IsClient)
        {
            StartCoroutine(RaiseBuilding(3f));
        }
>>>>>>> yj
    }

    void Update()
    {
        if (health <= 0 && !isDestruction)
        {
            isDestruction = true;
            DestructionBuilding();
        }
    }

<<<<<<< HEAD
    IEnumerator RaiseBuilding(GameObject building, float targetPosY, float duration) // 건물 생성될 때 효과
=======
    IEnumerator RaiseBuilding(float duration)
>>>>>>> yj
    {
        float elapsedTime = 0f;
        Vector3 startPos = transform.localPosition;
        Vector3 targetPos = new Vector3(0f, 10f, 0f);

        GameObject buildEffect = Instantiate(effectData.buildEffect, transform.position, Quaternion.identity, transform);
        buildEffect.transform.SetParent(transform, true);
        buildEffect.transform.localPosition = new Vector3(0f, 0f, 1.5f);

        while (elapsedTime < duration)
        {
            transform.localPosition = Vector3.Lerp(startPos, targetPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
<<<<<<< HEAD

        building.transform.position = targetPos;
        
=======
        transform.localPosition = targetPos;
>>>>>>> yj
        Destroy(buildEffect);

        GameObject sparkleEffect = Instantiate(effectData.sparkleEffect, transform.position, Quaternion.identity);
        sparkleEffect.transform.SetParent(transform, true);
        sparkleEffect.transform.localPosition = Vector3.zero;
        yield return new WaitForSeconds(0.5f);
        Destroy(sparkleEffect);
    }

<<<<<<< HEAD
    void DestructionBuilding() // 건물 파괴
    {
        GetComponentInParent<OccupySystem>().ResetOwnership();
        gameObject.SetActive(false);
        GameObject destructionEffect = Instantiate(buildingData.destructionEffect, transform.position, Quaternion.identity);
        Destroy(destructionEffect, 3.25f);
        Destroy(gameObject, 3.25f);
    }
}
=======
    [ServerRpc(RequireOwnership = false)]
    void DestructionBuildingServerRpc()
    {
        GetComponent<OccupySystem>().ResetOwnershipServerRpc();

        DestructionBuildingClientRpc();

        NetworkObject networkObject = GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Despawn(true);
        }
    }

    [ClientRpc]
    void DestructionBuildingClientRpc()
    {
        if (IsServer) return;

        GameObject destructionEffect = Instantiate(effectData.destructionEffect, transform.position, Quaternion.identity);
        destructionEffect.transform.SetParent(transform, true);
        destructionEffect.transform.localPosition = Vector3.zero;
        Destroy(destructionEffect, 3.25f);
        gameObject.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        if (health.Value > 0)
        {
            health.Value -= damage;
        }
    }
}
>>>>>>> yj
