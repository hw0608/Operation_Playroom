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
        SoldierIdleAnimServerRpc();
        
    }
    [ServerRpc(RequireOwnership = false)]
    private void SoldierIdleAnimServerRpc()
    {
        Debug.Log("병사 Idle 실행");
        networkAnimator.Animator.SetBool("Idle", true);
        networkAnimator.Animator.SetBool("Walk", false);
        networkAnimator.Animator.SetBool("Die", false);
    }
    public void SoldierWalkAnim()
    {
        SoldierWalkAnimServerRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    private void SoldierWalkAnimServerRpc()
    {
        Debug.Log("병사 Walk 실행");
        if (networkAnimator == null) return;
        networkAnimator.Animator.SetBool("Idle", false);
        networkAnimator.Animator.SetBool("Walk", true);
        networkAnimator.Animator.SetBool("Die", false);
    }
    public void SoldierAttackAnim()
    {
        SoldierAttackAnimServerRpc();
        
    }
    [ServerRpc(RequireOwnership = false)]
    private void SoldierAttackAnimServerRpc()
    {
        if (networkAnimator == null) return;
        networkAnimator.Animator.SetTrigger("Attack");
    }
    public void SoldierHitAnim()
    {
        SoldierCollectAnimServerRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    private void SoldierHitAnimServerRpc()
    {
        if (networkAnimator == null) return;
        networkAnimator.Animator.SetTrigger("Hit");
    }
    public void SoldierCollectAnim()
    {
        
        SoldierCollectAnimServerRpc();
        
    }
    [ServerRpc(RequireOwnership = false)]
    private void SoldierCollectAnimServerRpc()
    {
        if (networkAnimator == null) return;
        networkAnimator.Animator.SetTrigger("Collect");
    }
    public void SoldierDieAnim()
    {
        SoldierDieAnimServerRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    private void SoldierDieAnimServerRpc()
    {
        if (networkAnimator == null) return;
        networkAnimator.Animator.SetBool("Idle", false);
        networkAnimator.Animator.SetBool("Walk", false);
        networkAnimator.Animator.SetBool("Die", true);
    }
}

