// 파일 이름: DefendNode.cs
using UnityEngine;

public class DefendNode : Node
{
    private PaladinActuator actuator;
    private CooldownManager cooldownManager;
    private string skillName = "Defend";
    private float cooldownDuration = 6f;

    public DefendNode(Transform agentTransform)
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

        actuator.SetDefend(true);
        actuator.OnActionStart(); // 행동 시작 알림
        cooldownManager.StartCooldown(skillName, cooldownDuration);

        return NodeState.SUCCESS;
    }
}