using UnityEngine;

// Node 클래스와 NodeState Enum이 정의되어 있어야 합니다.
// (기존 코드에 이미 있을 것이므로 여기서는 생략)

public class EvadeNode : Node
{
    private PaladinActuator actuator;
    private CooldownManager cooldownManager;
    private string direction;

    public EvadeNode(Transform agentTransform, string direction)
    {
        this.actuator = agentTransform.GetComponent<PaladinActuator>();
        this.cooldownManager = agentTransform.GetComponent<CooldownManager>();
        this.direction = direction;
    }

    public override NodeState Evaluate()
    {
        // 다른 행동을 하고 있다면 회피할 수 없음
        if (actuator.IsActionInProgress)
        {
            return NodeState.FAILURE;
        }

        if (actuator == null || cooldownManager == null)
        {
            Debug.LogError("EvadeNode: Actuator 또는 CooldownManager를 찾을 수 없습니다.");
            return NodeState.FAILURE;
        }

        // 1. 행동 시작 잠금
        actuator.OnActionStart();

        // 2. Actuator에 실제 회피 동작 지시
        actuator.Dodge(direction); // PaladinActuator.cs에 이미 있는 Dodge 함수 호출

        // 3. 'Evade' 스킬 쿨타임 적용
        cooldownManager.StartCooldown("Evade", 10f); // 쿨타임 시간은 CooldownManager에서 관리

        // 4. 행동을 성공적으로 '지시'했으므로 SUCCESS 반환
        //    (주의: OnActionEnd는 애니메이션 이벤트로 처리해야 합니다. 2단계 참조)
        return NodeState.SUCCESS;
    }
}