// 파일 이름: MaintainDistanceNode.cs (수정 완료 버전)
using UnityEngine;

public class MaintainDistanceNode : Node
{
    // 기존 변수 (이름 유지)
    private Transform agentTransform;
    private Transform targetTransform;
    private PaladinActuator actuator;
    private float idealDistance;
    private float tolerance;
    
    // 새로 추가된 변수
    private float directionUpdateTimer;
    private const float DIRECTION_UPDATE_INTERVAL = 1.0f; // 방향 업데이트 주기 (1초)

    // 생성자는 그대로 유지하여 다른 스크립트의 오류를 해결합니다.
    public MaintainDistanceNode(Transform agent, Transform target, float idealDist, float tol)
    {
        this.agentTransform = agent;
        this.targetTransform = target;
        this.actuator = agent.GetComponent<PaladinActuator>();
        this.idealDistance = idealDist;
        this.tolerance = tol;
    }

    public override NodeState Evaluate()
    {
        if (targetTransform == null || actuator == null) return NodeState.FAILURE;

        float distance = Vector3.Distance(agentTransform.position, targetTransform.position);

        // 1. 목표 거리에 도달했는지 확인
        if (Mathf.Abs(distance - idealDistance) <= tolerance)
        {
            actuator.SetMovement(0); // 목표 도달 시 멈춤
            directionUpdateTimer = DIRECTION_UPDATE_INTERVAL; // 타이머를 초기화하여 다음에 즉시 방향을 보도록 함
            return NodeState.SUCCESS; // 행동 성공 및 종료
        }

        // --- 여기서부터는 목표 거리를 맞추기 위해 움직여야 하는 경우 ---
        
        // 2. 타이머를 업데이트하고, 1초가 지났는지 확인
        directionUpdateTimer += Time.deltaTime;
        if (directionUpdateTimer >= DIRECTION_UPDATE_INTERVAL)
        {
            directionUpdateTimer = 0f; // 타이머 리셋
            
            // 3. 방향 재설정: 타겟을 바라보도록 회전
            Vector3 lookPosition = targetTransform.position - agentTransform.position;
            lookPosition.y = 0; // AI가 위아래로 기울지 않도록 함
            actuator.SetRotation(Quaternion.LookRotation(lookPosition));
        }

        // 4. 움직임 제어: 거리에 따라 전진 또는 후진
        actuator.SetMovement(distance > idealDistance ? 0.75f : -0.75f);
        
        // 5. 행동이 아직 끝나지 않았으므로 '진행 중' 상태 반환
        return NodeState.RUNNING;
    }
}