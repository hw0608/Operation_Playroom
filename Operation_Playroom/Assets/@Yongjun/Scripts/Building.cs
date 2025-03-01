using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class Building : NetworkBehaviour
{
    [SerializeField] NetworkVariable<int> health; // 건물 체력
    [SerializeField] BuildingScriptableObject buildingData; // 건물 데이터 스크립터블 오브젝트
    bool isDestruction = false; // 중복철거 방지


    //[SerializeField] BuildingScriptableObject buildingData;
    [SerializeField] EffectScriptableObject effectData;


    void Start()
    {

        if (IsServer)
        {
            health.Value = buildingData.health;
        }

        if (IsClient)
        {
            StartCoroutine(RaiseBuilding(3f));
        }
    }

    void Update()
    {
        if (health.Value <= 0 && !isDestruction)
        {
            isDestruction = true;
            DestructionBuildingServerRpc();
        }
    }


    IEnumerator RaiseBuilding(float duration)
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

        transform.localPosition = targetPos;

        Destroy(buildEffect);

        GameObject sparkleEffect = Instantiate(effectData.sparkleEffect, transform.position, Quaternion.identity);
        sparkleEffect.transform.SetParent(transform, true);
        sparkleEffect.transform.localPosition = Vector3.zero;
        yield return new WaitForSeconds(0.5f);
        Destroy(sparkleEffect);
    }


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

