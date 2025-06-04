using UnityEngine;

public class DefendNode : Node
{
    private Animator animator;
    private CooldownManager cooldownManager;

    public DefendNode(Transform agentTransform)
    {
        animator = agentTransform.GetComponent<Animator>();
        cooldownManager = agentTransform.GetComponent<CooldownManager>();
    }

    public override NodeState Evaluate()
    {
        // "Defend" 애니메이션 트리거를 발동시킵니다.
        // Animator에 "Defend" 파라미터가 있어야 합니다.
        animator.SetTrigger("Defend");

        // BT 초안 문서에 따라 2초 쿨타임을 적용합니다.
        cooldownManager.StartCooldown("Defend", 2f);
        
        return NodeState.SUCCESS;
    }
}