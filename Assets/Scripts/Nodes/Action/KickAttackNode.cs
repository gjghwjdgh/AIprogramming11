// 파일 이름: KickAttackNode.cs
using UnityEngine;

public class KickAttackNode : Node
{
    private PaladinActuator actuator;
    private CooldownManager cooldownManager;
    private DataLogger dataLogger; // ★★★ 데이터 로거 참조 추가
    private string skillName = "KickAttack";
    private float cooldownDuration = 10f; // 기존 쿨타임 값 15초 유지

    public KickAttackNode(Transform agentTransform)
    {
        actuator = agentTransform.GetComponent<PaladinActuator>();
        cooldownManager = agentTransform.GetComponent<CooldownManager>();
        dataLogger = agentTransform.GetComponent<DataLogger>(); // ★★★ 데이터 로거 찾아오기
    }

    public override NodeState Evaluate()
    {
        if (!cooldownManager.IsCooldownFinished(skillName))
        {
            return NodeState.FAILURE;
        }

        actuator.StartAttack(PaladinActuator.AttackType.E_Kick);
        actuator.OnActionStart();
        
        // 기존 쿨타임 로직 그대로 유지
        cooldownManager.StartCooldown(skillName, cooldownDuration);

        // ★★★ 공격 성공 시, 로거에 기록 ★★★
        if(dataLogger != null)
        {
            dataLogger.IncrementAttackCount();
        }

        return NodeState.SUCCESS;
    }
}