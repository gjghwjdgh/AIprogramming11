// 파일 이름: FeintStepNode.cs
using UnityEngine;

public class FeintStepNode : Node
{
    private PaladinActuator actuator;
    private CooldownManager cooldownManager;
    private string direction;
    private string skillName = "FeintStep";
    private float cooldownDuration = 3f;

    public FeintStepNode(Transform agentTransform, string direction)
    {
        this.actuator = agentTransform.GetComponent<PaladinActuator>();
        this.cooldownManager = agentTransform.GetComponent<CooldownManager>();
        this.direction = direction;
    }

    public override NodeState Evaluate()
    {
        actuator.FeintStep(direction);
        actuator.OnActionStart(); // 행동 시작 알림
        cooldownManager.StartCooldown(skillName, cooldownDuration);
        return NodeState.SUCCESS;
    }
}