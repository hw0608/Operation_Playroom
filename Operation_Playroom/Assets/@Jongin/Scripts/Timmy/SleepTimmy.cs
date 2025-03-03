using Unity.Netcode;
using UnityEngine;

public class SleepTimmy : NetworkBehaviour
{
    public NetworkVariable<bool> timmyActive = new NetworkVariable<bool>(true);
    public Animator animator;
    void Start()
    {
        timmyActive.OnValueChanged += OnSetActiveSelf;
    }
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        base.OnNetworkSpawn();
        animator = GetComponent<Animator>();
        timmyActive.Value = true;
    }
    public void OnSetActiveSelf(bool oldValue, bool newValue)
    {
        gameObject.SetActive(newValue);
    }
}
