using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Building : NetworkBehaviour
{
    // 건물 체력
    [SerializeField] NetworkVariable<int> health = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // 스크립터블 오브젝트
    [SerializeField] BuildingScriptableObject buildingData;

    // 중복 철거 방지
    bool isDestruction = false;

    public override void OnNetworkSpawn()
    {
        Debug.Log("BuildingSpawn!");
        health.OnValueChanged -= OnHealthChange;
        health.OnValueChanged += OnHealthChange;
    }
    public override void OnNetworkDespawn()
    {
        health.OnValueChanged -= OnHealthChange;
    }
    public void OnHealthChange(int oldVal, int newVal)
    {
        if (newVal <= 0 && !isDestruction)
        {
            isDestruction = true;
            if (IsServer)
            {
                DestructionBuilding();
            }
        }
    }

    public void BuildingInit()
    {
        Debug.Log("Building Init!");
        health.Value = buildingData.health;
        Debug.Log(health.Value);

        StartCoroutine(RaiseBuilding(3f));
    }


    void Update()
    {
        if (!IsServer) return;

        if (Input.GetKeyDown(KeyCode.K))
        {
            health.Value = 0;
        }

    }

    IEnumerator RaiseBuilding(float duration)
    {
        float elapsedTime = 0f;
        Vector3 startPos = new Vector3(0, -25f, 0);
        Vector3 targetPos = new Vector3(0f, 5f, 0f);

        GameObject buildEffect = Managers.Resource.Instantiate("BuildingSmokeEffect", null, true);
        ActiveNetworkObjectClientRpc(buildEffect.GetComponent<NetworkObject>().NetworkObjectId, true);
        if (buildEffect.GetComponent<NetworkObject>().TrySetParent(transform.parent, true))
        {
            buildEffect.transform.localPosition = Vector3.zero;
        }

        while (elapsedTime < duration)
        {
            transform.localPosition = Vector3.Lerp(startPos, targetPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = targetPos;

        Managers.Pool.Push(buildEffect);
        ActiveNetworkObjectClientRpc(buildEffect.GetComponent<NetworkObject>().NetworkObjectId, false);

        GameObject sparkleEffect = Managers.Resource.Instantiate("BuildCompleteEffect");
        ActiveNetworkObjectClientRpc(sparkleEffect.GetComponent<NetworkObject>().NetworkObjectId, true);
        if (sparkleEffect.GetComponent<NetworkObject>().TrySetParent(transform, true))
        {
            sparkleEffect.transform.localPosition = Vector3.zero;
            yield return new WaitForSeconds(0.5f);
            Managers.Pool.Push(sparkleEffect);
            ActiveNetworkObjectClientRpc(sparkleEffect.GetComponent<NetworkObject>().NetworkObjectId, false);
        };
        isDestruction = false;
    }

    void DestructionBuilding()
    {
        GetComponentInParent<OccupySystem>().ResetOwnership();
        GameObject destructionEffect = Managers.Resource.Instantiate("BuildingDestroyEffect", null, true);
        ActiveNetworkObjectClientRpc(destructionEffect.GetComponent<NetworkObject>().NetworkObjectId, true);
        if (destructionEffect.GetComponent<NetworkObject>().TrySetParent(transform.parent, true))
        {
            destructionEffect.transform.localPosition = Vector3.zero;
            StartCoroutine(DelayPushEffect(destructionEffect, 3.25f));
            transform.localPosition = new Vector3(0, -25f, 0);
        }
    }
    IEnumerator DelayPushEffect(GameObject effect, float time)
    {
        yield return new WaitForSeconds(time);
        Managers.Pool.Push(effect);
        ActiveNetworkObjectClientRpc(effect.GetComponent<NetworkObject>().NetworkObjectId, false);
        Managers.Pool.Push(gameObject);
        ActiveNetworkObjectClientRpc(GetComponent<NetworkObject>().NetworkObjectId, false);
    }
    [ClientRpc]
    void ActiveNetworkObjectClientRpc(ulong networkObjectId, bool isActive)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
        {
            networkObject.gameObject.SetActive(isActive);
        }
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
