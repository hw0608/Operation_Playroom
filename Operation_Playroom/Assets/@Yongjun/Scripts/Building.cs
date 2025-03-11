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

    // 건물 메쉬
    MeshFilter meshFilter;

    // 건물 체력 상태
    int currentState = 3; // 3: 기본, 2: 손상, 1: 많이 손상

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

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
        //// ~
        int damage = oldVal - newVal;
        if (damage > 0)
        {
            if (IsServer)
            {
                StartCoroutine(PlayDamageEffect());
            }
        }
        UpdateBuildingMesh(newVal);
        //// ~

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

        StartCoroutine(RaiseBuilding(2f));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TakeDamageServerRpc(10);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TakeDamageServerRpc(100);
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
    public void TakeDamageServerRpc(int damage) // 테스트용도 서버한테 건물 체력감소 요청. 나중에 지워도 됩니다
    {
        if (health.Value > 0)
        {
            health.Value -= damage;
        }
    }

    public void TakeDamage(int damage)
    {
        if (health.Value > 0)
        {
            health.Value -= damage;
        }
    }

    void UpdateBuildingMesh(int health)
    {
        float healthPer = (float)health / buildingData.health;
        int state = (healthPer > 0.6f) ? 3 : (healthPer > 0.2f) ? 2 : 1; // 건물체력이 60%이상일때 보통, 20~60%일때 손상, 20%미만일때 많이손상

        if (currentState == state) return;

        currentState = state;

        switch (state) 
        {
            case 3:
                meshFilter.mesh = buildingData.defaultMesh;
                break;

            case 2:
                meshFilter.mesh = buildingData.brokenMesh;
                break;

            case 1:
                meshFilter.mesh = buildingData.hardBrokenMesh;
                break;
        }
    }

    IEnumerator PlayDamageEffect()
    {
        // 건물 피해 입을 때 이펙트
        GameObject damageEffect = Managers.Resource.Instantiate("BuildingDamageEffect", null, true);
        ActiveNetworkObjectClientRpc(damageEffect.GetComponent<NetworkObject>().NetworkObjectId, true);
        if (damageEffect.GetComponent<NetworkObject>().TrySetParent(transform.parent, true))
        {
            damageEffect.transform.localPosition = Vector3.zero;
            yield return new WaitForSeconds(1f);
            Managers.Pool.Push(damageEffect);
            ActiveNetworkObjectClientRpc(damageEffect.GetComponent<NetworkObject>().NetworkObjectId, false);
        }
    }
} 