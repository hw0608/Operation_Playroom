using System.Collections;
using Unity.Netcode;
using UnityEngine;

public enum Owner { Red, Blue, Neutral }

public class ResourceData : NetworkBehaviour
{
    [SerializeField] Owner currentOwner = Owner.Neutral;
    Transform originalTransform;
    public NetworkVariable<bool> isColliderEnable = new NetworkVariable<bool>();
    public Collider resourceCollider;
    Coroutine delayColliderEnable;
    Coroutine delayPushObject;
    public override void OnNetworkSpawn()
    {
        resourceCollider = GetComponent<Collider>();
        if (IsServer)
        {
            originalTransform = GameObject.Find("@Resources").transform;
            isColliderEnable.Value = resourceCollider.enabled;
        }
        isColliderEnable.OnValueChanged -= ChangeColliderEnable;
        isColliderEnable.OnValueChanged += ChangeColliderEnable;
    }

    public override void OnNetworkDespawn()
    {
        isColliderEnable.OnValueChanged -= ChangeColliderEnable;
    }

    void ChangeColliderEnable(bool oldVal, bool newVal)
    {
        if (newVal)
        {
            if (delayColliderEnable != null)
            {
                StopCoroutine(delayColliderEnable);
                delayColliderEnable = null;
            }
            delayColliderEnable = StartCoroutine(DelayColliderEnable(newVal));
        }
        else
        {
            resourceCollider.enabled = newVal;
        }
    }
    public Owner CurrentOwner
    {
        get { return currentOwner; }
        set { currentOwner = value; }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetParentOwnerserverRpc(ulong targetId, bool isPickUp)
    {
        if (isPickUp)
        {
            NetworkObject newParent = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetId];
            GetComponent<NetworkObject>().TrySetParent(newParent);
            transform.localPosition = new Vector3(0, 1f, 0);
            isColliderEnable.Value = false;
        }
        else
        {
            NetworkObject go = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetId];
            GetComponent<NetworkObject>().TrySetParent(originalTransform);
            Vector3 newPos = go.transform.position + go.transform.forward * 0.4f;
            transform.position = new Vector3(newPos.x, 0, newPos.z); // 앞에 내려놓기
            isColliderEnable.Value = true;
        }
    }



    IEnumerator DelayColliderEnable(bool value)
    {
        yield return new WaitForSeconds(0.3f);
        resourceCollider.enabled = value;
    }

}
