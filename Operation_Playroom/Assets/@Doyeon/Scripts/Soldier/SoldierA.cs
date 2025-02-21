using UnityEngine;

public class SoldierA : MonoBehaviour
{
    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void SodierIdleAnim()
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
}
