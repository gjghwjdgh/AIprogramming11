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
        // PaladinActuator와 CooldownManager 컴포넌트를 가져옵니다.
        actuator = agentTransform.GetComponent<PaladinActuator>();
        cooldownManager = agentTransform.GetComponent<CooldownManager>();
    }

    public override NodeState Evaluate()
    {
        // 쿨다운이 완료되었는지 확인합니다.
        if (!cooldownManager.IsCooldownFinished(skillName))
        {
            return NodeState.FAILURE; // 쿨다운 중이면 실패를 반환합니다.
        }

        // PaladinActuator를 통해 회전 공격(R_Attack)을 실행합니다.
        actuator.StartAttack(PaladinActuator.AttackType.R_Attack);
        actuator.OnActionStart(); // 행동 시작을 알립니다.
        
        // 쿨다운을 시작합니다.
        cooldownManager.StartCooldown(skillName, cooldownDuration);

        return NodeState.SUCCESS; // 성공을 반환합니다.
    }
}