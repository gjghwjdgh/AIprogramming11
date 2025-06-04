using UnityEngine;

public class KickAttackNode : Node
{
    private Animator animator;
    private CooldownManager cooldownManager;

    public KickAttackNode(Transform agentTransform)
    {
        animator = agentTransform.GetComponent<Animator>();
        cooldownManager = agentTransform.GetComponent<CooldownManager>();
    }

    public override NodeState Evaluate()
    {
        // "KickAttack" 스킬의 쿨타임이 완료되었는지 확인
        if (!cooldownManager.IsCooldownFinished("KickAttack"))
        {
            return NodeState.FAILURE; // 쿨타임이면 실패
        }

        // "KickAttack" 애니메이션 트리거 발동
        animator.SetTrigger("KickAttack");
        // 5초 쿨타임 시작
        cooldownManager.StartCooldown("KickAttack", 5f);
        return NodeState.SUCCESS;
    }
}