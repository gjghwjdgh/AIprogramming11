// 파일 이름: MoveAwayNode.cs
using UnityEngine;

public class MoveAwayNode : Node
{
    private Transform agentTransform;
    private Transform targetTransform; // 타겟 정보가 다시 필요해짐
    private PaladinActuator actuator;
    private CooldownManager cooldownManager;
    private string cooldownName = "Reposition";
    private float cooldownDuration = 2f;

    // 생성자가 이제 Transform 2개를 받도록 수정
    public MoveAwayNode(Transform agentTransform, Transform targetTransform)
    {
        this.agentTransform = agentTransform;
        this.targetTransform = targetTransform; // 타겟 정보 저장
        this.actuator = agentTransform.GetComponent<PaladinActuator>();
        this.cooldownManager = agentTransform.GetComponent<CooldownManager>();
    }

    public override NodeState Evaluate()
    {
        if (targetTransform == null || actuator == null || cooldownManager == null) return NodeState.FAILURE;

        if (!cooldownManager.IsCooldownFinished(cooldownName))
        {
            return NodeState.FAILURE;
        }

        // --- 핵심 수정 부분 ---
        // 1. 타겟을 향하는 방향을 계산합니다.
        Vector3 directionToTarget = targetTransform.position - agentTransform.position;
        directionToTarget.y = 0; // 높이는 무시

        // 2. 그 방향을 바라보도록 회전 명령을 내립니다.
        actuator.SetRotation(Quaternion.LookRotation(directionToTarget));

        // 3. 뒤로 걷기 명령을 내립니다.
        actuator.SetMovement(-1.0f);
        // --- 수정 끝 ---
        
        cooldownManager.StartCooldown(cooldownName, cooldownDuration);

        return NodeState.SUCCESS;
    }
}