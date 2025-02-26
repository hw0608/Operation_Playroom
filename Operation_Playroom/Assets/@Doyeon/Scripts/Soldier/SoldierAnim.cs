using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class SoldierAnim : NetworkBehaviour
{
    private Animator animator;
    private NetworkAnimator networkAnimator;
    public override void OnNetworkSpawn()
    {
        animator = GetComponent<Animator>();
        networkAnimator = GetComponent<NetworkAnimator>();

        if (networkAnimator == null)
            Debug.LogError("네트워크애니메이터 초기화 안됨");
    }
    public void SoldierIdleAnim()
    {
        if (IsOwner)
        {
            SoldierIdleAnimServerRpc();
        }
    }
    [ServerRpc]
    private void SoldierIdleAnimServerRpc()
    {
        networkAnimator.Animator.SetBool("Idle", true);
        networkAnimator.Animator.SetBool("Walk", false);
        networkAnimator.Animator.SetBool("Die", false);
    }
    public void SoldierWalkAnim()
    {
        if (IsOwner)
        {
            SoldierWalkAnimServerRpc();
        }
    }
    [ServerRpc]
    private void SoldierWalkAnimServerRpc()
    {
        if (networkAnimator == null) return;
        networkAnimator.Animator.SetBool("Idle", false);
        networkAnimator.Animator.SetBool("Walk", true);
        networkAnimator.Animator.SetBool("Die", false);
    }
    public void SoldierAttackAnim()
    {
        if (IsOwner)
        {
            SoldierAttackAnimServerRpc();
        }
    }
    [ServerRpc]
    private void SoldierAttackAnimServerRpc()
    {
        if (networkAnimator == null) return;
        networkAnimator.Animator.SetTrigger("Attack");
    }
    public void SoldierHitAnim()
    {
        if (IsOwner)
        {
            SoldierCollectAnimServerRpc();
        }
    }
    [ServerRpc]
    private void SoldierHitAnimServerRpc()
    {
        if (networkAnimator == null) return;
        networkAnimator.Animator.SetTrigger("Hit");
    }
    public void SoldierCollectAnim()
    {
        if (IsOwner)
        {
            SoldierCollectAnimServerRpc();
        }
    }
    [ServerRpc]
    private void SoldierCollectAnimServerRpc()
    {
        if (networkAnimator == null) return;
        networkAnimator.Animator.SetTrigger("Collect");
    }
    public void SoldierDieAnim()
    {
        SoldierCollectAnimServerRpc();
    }
    [ServerRpc]
    private void SoldierDieAnimServerRpc()
    {
        if (networkAnimator == null) return;
        networkAnimator.Animator.SetBool("Idle", false);
        networkAnimator.Animator.SetBool("Walk", false);
        networkAnimator.Animator.SetBool("Die", true);
    }
}

