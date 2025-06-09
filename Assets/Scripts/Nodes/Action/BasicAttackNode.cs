// 파일 이름: BasicAttackNode.cs
using UnityEngine;

public class BasicAttackNode : Node
{
    private PaladinActuator actuator;
    private CooldownManager cooldownManager;
    private DataLogger dataLogger; // ★★★ 데이터 로거 참조 추가
    private string skillName = "BasicAttack";
    private float cooldownDuration = 6f; // 기존 쿨타임 값 6초 유지

    public BasicAttackNode(Transform agentTransform)
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

        actuator.StartAttack(PaladinActuator.AttackType.Q_Attack);
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