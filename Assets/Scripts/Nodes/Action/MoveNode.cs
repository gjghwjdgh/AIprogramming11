using UnityEngine;

// 파일 이름: MoveNode.cs
public class MoveNode : Node
{
    private Transform agentTransform;
    private Transform targetTransform;
    private Animator animator;
    private float moveSpeed = 2.0f; // 캐릭터의 기본 이동 속도

    public MoveNode(Transform agentTransform, Transform targetTransform)
    {
        this.agentTransform = agentTransform;
        this.targetTransform = targetTransform;
        this.animator = agentTransform.GetComponent<Animator>();
    }

    public override NodeState Evaluate()
    {
        if (targetTransform == null)
        {
            animator.SetFloat("MoveSpeed", 0f);
            return NodeState.FAILURE; // 타겟이 없으면 이동 불가
        }

        // 목표 방향 계산 (Y축은 무시)
        Vector3 direction = targetTransform.position - agentTransform.position;
        direction.y = 0;

        // 너무 가까우면 이동을 멈춤 (제자리에서 공격 등을 하기 위함)
        // 이 거리는 다른 노드(예: IsEnemyInDistanceNode)의 조건과 연계하여 조절
        float attackRange = 1.5f; // 예시 공격 범위
        if (direction.magnitude <= attackRange)
        {
            animator.SetFloat("MoveSpeed", 0f); // 이동 멈춤 애니메이션
            // 제자리에 도달했으므로 SUCCESS로 볼 수도 있지만,
            // 지속적으로 거리를 판단하며 움직여야 할 수도 있으므로 RUNNING을 고려할 수도 있습니다.
            // 여기서는 일단 멈추면 SUCCESS로 처리합니다.
            return NodeState.SUCCESS;
        }

        // 캐릭터를 목표 방향으로 이동 및 회전
        agentTransform.position += direction.normalized * moveSpeed * Time.deltaTime;
        agentTransform.rotation = Quaternion.LookRotation(direction);

        animator.SetFloat("MoveSpeed", direction.magnitude > 0.1f ? 1.0f : 0.0f);
        
        return NodeState.RUNNING; // 목표에 도달할 때까지 계속 이동 중
    }
}