using UnityEngine;

public class SoldierAnim : MonoBehaviour
{
    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void SoldierIdleAnim()
    {
        animator.SetBool("Idle", true);
        animator.SetBool("Walk", false);
    }
    public void SoldierWalkAnim()
    {
        animator.SetBool("Idle", false);
        animator.SetBool("Walk", true);
    }
    public void SoldierAttackAnim()
    {
        animator.SetTrigger("Attack");
    }
    public void SoldierCollectAnim()
    {
        animator.SetTrigger("Collect");
    }
    public void SoldierDieAnim()
    {
        animator.SetTrigger("Die");
    }
}

