using UnityEngine;

public class BasicAttackNode : Node
{
    private Animator animator;
    private CooldownManager cooldownManager;

    public BasicAttackNode(Transform agentTransform)
    {
        this.animator = agentTransform.GetComponent<Animator>();
        this.cooldownManager = agentTransform.GetComponent<CooldownManager>();
    }

    public override NodeState Evaluate()
    {
        if (!cooldownManager.IsCooldownFinished("BasicAttack"))
        {
            return NodeState.FAILURE; // 쿨타임이면 실패
        }

        animator.SetTrigger("Attack");
        cooldownManager.StartCooldown("BasicAttack", 2.0f); // [cite: 14]
        return NodeState.SUCCESS; // 공격 실행 성공
    }
}