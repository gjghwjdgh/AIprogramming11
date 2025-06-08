// 파일 이름: SpinAttackNode.cs
using UnityEngine;

public class SpinAttackNode : Node
{
    private PaladinActuator actuator;
    private CooldownManager cooldownManager;
    private string skillName = "SpinAttack";
    private float cooldownDuration = 30f;

    public SpinAttackNode(Transform agentTransform)
    {
        actuator = agentTransform.GetComponent<PaladinActuator>();
        cooldownManager = agentTransform.GetComponent<CooldownManager>();
    }

    public override NodeState Evaluate()
    {
        if (!cooldownManager.IsCooldownFinished(skillName))
        {
            return NodeState.FAILURE;
        }

        actuator.StartAttack(PaladinActuator.AttackType.R_Attack);
        actuator.OnActionStart(); // 행동 시작 알림
        cooldownManager.StartCooldown(skillName, cooldownDuration);

        return NodeState.SUCCESS;
    }
}