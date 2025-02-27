using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class Building : NetworkBehaviour
{
    [SerializeField]
    NetworkVariable<int> health = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] BuildingScriptableObject buildingData;

    bool isDestruction = false; // 중복 파괴 방지

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            health.Value = buildingData.health;
        }

        if (IsClient)
        {
            StartCoroutine(RaiseBuilding(gameObject, 0.2f, 3f));
        }
    }

    void Update()
    {
        if (!IsServer) return;

        if (health.Value <= 0 && !isDestruction)
        {
            isDestruction = true;
            DestructionBuildingServerRpc();
        }
    }

    IEnumerator RaiseBuilding(GameObject building, float targetPosY, float duration)
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

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        if (health.Value > 0)
        {
            health.Value -= damage;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void DestructionBuildingServerRpc()
    {
        GetComponentInParent<OccupySystem>().ResetOwnershipServerRpc();

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

        GameObject destructionEffect = Instantiate(buildingData.destructionEffect, transform.position, Quaternion.identity);
        Destroy(destructionEffect, 3.25f);
        gameObject.SetActive(false);
    }
}
