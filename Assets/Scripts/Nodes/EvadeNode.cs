using UnityEngine;

public class EvadeNode : Node
{
    private Animator animator;
    private CooldownManager cooldownManager;

    public EvadeNode(Transform agentTransform)
    {
        animator = agentTransform.GetComponent<Animator>();
        cooldownManager = agentTransform.GetComponent<CooldownManager>();
    }

    public override NodeState Evaluate()
    {
        // EvadeNode는 IsCooldownCompleteNode를 통과한 후에 실행되므로
        // 여기서 또 쿨타임을 체크할 필요는 없습니다. 바로 행동을 실행합니다.

        animator.SetTrigger("Evade"); // "Evade" 애니메이션 트리거 발동
        cooldownManager.StartCooldown("Evade", 10f); // BT 초안에 명시된 10초 쿨타임 시작 [cite: 8, 9]
        return NodeState.SUCCESS;
    }
}