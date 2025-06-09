// 파일 이름: StartDefendNode.cs
using UnityEngine;

public class StartDefendNode : Node
{
    private PaladinActuator actuator;
    private CooldownManager cooldownManager;

    public StartDefendNode(Transform agentTransform)
    {
        this.actuator = agentTransform.GetComponent<PaladinActuator>();
        this.cooldownManager = agentTransform.GetComponent<CooldownManager>();
    }

    public override NodeState Evaluate()
    {
        actuator.StartDefense();
        cooldownManager.StartCooldown("Defend", 6f); // 방어 후 6초 쿨타임 (값은 조절 가능)
        return NodeState.SUCCESS;
    }
}