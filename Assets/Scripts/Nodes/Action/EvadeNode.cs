// 파일 이름: EvadeNode.cs
using UnityEngine;

public class EvadeNode : Node
{
    private Transform agentTransform; // EvadeNode는 target을 직접 알 필요는 없음
    private PaladinActuator actuator;
    private CooldownManager cooldownManager;
    private string direction;

    // 생성자는 그대로 유지
    public EvadeNode(Transform agentTransform, string direction = "Backward")
    {
        this.agentTransform = agentTransform;
        this.actuator = agentTransform.GetComponent<PaladinActuator>();
        this.cooldownManager = agentTransform.GetComponent<CooldownManager>();
        this.direction = direction;
    }

    public override NodeState Evaluate()
    {
        // --- 핵심 수정 부분 ---
        // 만약 뒤로 회피하는 경우, 먼저 타겟을 바라보게 함
        if (direction == "Backward")
        {
            // BT_Aggressive_Paladin 컴포넌트에서 target 정보를 가져옴
            BT_Aggressive_Paladin brain = agentTransform.GetComponent<BT_Aggressive_Paladin>();
            if (brain != null && brain.target != null)
            {
                Vector3 directionToTarget = brain.target.position - agentTransform.position;
                directionToTarget.y = 0;
                actuator.SetRotation(Quaternion.LookRotation(directionToTarget));
            }
        }
        // --- 수정 끝 ---

        actuator.Dodge(direction);
        actuator.OnActionStart(); // 회피는 액션 잠금 사용
        cooldownManager.StartCooldown("Evade", 10f);
        return NodeState.SUCCESS;
    }
}