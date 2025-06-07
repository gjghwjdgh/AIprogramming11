// 파일 이름: KickAttackNode.cs
using UnityEngine;

public class KickAttackNode : Node
{
    private PaladinActuator actuator;
    private CooldownManager cooldownManager;
    private string skillName = "KickAttack";
    private float cooldownDuration = 15f;

    public KickAttackNode(Transform agentTransform)
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

        actuator.StartAttack(PaladinActuator.AttackType.E_Kick);
        actuator.OnActionStart(); // 행동 시작 알림
        cooldownManager.StartCooldown(skillName, cooldownDuration);

        return NodeState.SUCCESS;
    }
}