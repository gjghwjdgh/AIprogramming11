// 파일 이름: TimedDefendNode.cs
using UnityEngine;

public class TimedDefendNode : Node
{
    private PaladinActuator actuator;
    private CooldownManager cooldownManager;
    private float duration; // 방어를 유지할 시간

    public TimedDefendNode(Transform agentTransform, float duration)
    {
        this.actuator = agentTransform.GetComponent<PaladinActuator>();
        this.cooldownManager = agentTransform.GetComponent<CooldownManager>();
        this.duration = duration;
    }

    public override NodeState Evaluate()
    {
        if (actuator == null || cooldownManager == null) return NodeState.FAILURE;

        // "N초 동안 방어" 명령 실행
        actuator.ExecuteTimedDefense(duration);

        // 방어 스킬에 쿨타임 적용
        cooldownManager.StartCooldown("Defend", 6f); 

        return NodeState.SUCCESS;
    }
}