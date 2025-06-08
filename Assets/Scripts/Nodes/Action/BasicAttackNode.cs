// 파일 이름: BasicAttackNode.cs
using UnityEngine;

public class BasicAttackNode : Node
{
    private PaladinActuator actuator;
    private CooldownManager cooldownManager;
    private string skillName = "BasicAttack";
    private float cooldownDuration = 6f;

    public BasicAttackNode(Transform agentTransform)
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

        actuator.StartAttack(PaladinActuator.AttackType.Q_Attack);
        actuator.OnActionStart(); // 행동 시작 알림
        cooldownManager.StartCooldown(skillName, cooldownDuration);

        return NodeState.SUCCESS;
    }
}