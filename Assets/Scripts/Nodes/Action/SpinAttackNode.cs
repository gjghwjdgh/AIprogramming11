using UnityEngine;

public class SpinAttackNode : Node
{
    private Animator animator;
    private CooldownManager cooldownManager;

    public SpinAttackNode(Transform agentTransform)
    {
        animator = agentTransform.GetComponent<Animator>();
        cooldownManager = agentTransform.GetComponent<CooldownManager>();
    }

    public override NodeState Evaluate()
    {
        // "SpinAttack" 스킬의 쿨타임이 완료되었는지 확인
        if (!cooldownManager.IsCooldownFinished("SpinAttack"))
        {
            return NodeState.FAILURE; // 쿨타임이면 실패
        }

        // "SpinAttack" 애니메이션 트리거 발동
        animator.SetTrigger("SpinAttack");
        // 10초 쿨타임 시작
        cooldownManager.StartCooldown("SpinAttack", 10f);
        return NodeState.SUCCESS;
    }
}