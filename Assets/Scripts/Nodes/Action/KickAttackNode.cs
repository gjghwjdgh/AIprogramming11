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

        // PaladinActuator를 통해 발차기 공격(E_Kick)을 실행합니다.
        actuator.StartAttack(PaladinActuator.AttackType.E_Kick);
        actuator.OnActionStart(); // 행동 시작을 알립니다.
        
        // 쿨다운을 시작합니다.
        cooldownManager.StartCooldown(skillName, cooldownDuration);

        return NodeState.SUCCESS; // 성공을 반환합니다.
    }
}