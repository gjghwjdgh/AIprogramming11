// 파일 이름: EvadeNode.cs
using UnityEngine;

public class EvadeNode : Node
{
    private PaladinActuator actuator;
    private CooldownManager cooldownManager;
    private string direction;

    public EvadeNode(Transform agentTransform, string direction = "Backward")
    {
        this.actuator = agentTransform.GetComponent<PaladinActuator>();
        this.cooldownManager = agentTransform.GetComponent<CooldownManager>();
        this.direction = direction;
    }

    public override NodeState Evaluate()
    {
        actuator.Dodge(direction);
        cooldownManager.StartCooldown("Evade", 10f);
        return NodeState.SUCCESS;
    }
}