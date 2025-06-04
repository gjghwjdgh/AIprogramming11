using UnityEngine;

// 파일 이름: MaintainDistanceNode.cs
public class MaintainDistanceNode : Node
{
    private Transform agentTransform;
    private Transform targetTransform;
    private Animator animator;
    private float idealDistance;
    private float tolerance = 0.5f; // 이상적 거리의 허용 오차
    private float moveSpeed = 2.0f;

    public MaintainDistanceNode(Transform agentTransform, Transform target, float idealDistance)
    {
        this.agentTransform = agentTransform;
        this.targetTransform = target;
        this.animator = agentTransform.GetComponent<Animator>();
        this.idealDistance = idealDistance;
    }

    public override NodeState Evaluate()
    {
        if (targetTransform == null) return NodeState.FAILURE;

        float currentDistance = Vector3.Distance(agentTransform.position, targetTransform.position);
        
        // 이상적 거리보다 멀리 떨어져 있으면
        if (currentDistance > idealDistance + tolerance)
        {
            // 타겟을 향해 이동
            Vector3 direction = targetTransform.position - agentTransform.position;
            direction.y = 0;
            agentTransform.position += direction.normalized * moveSpeed * Time.deltaTime;
            agentTransform.rotation = Quaternion.LookRotation(direction);
            animator.SetFloat("MoveSpeed", 1.0f);
            return NodeState.RUNNING; // 아직 이동 중이므로 RUNNING 반환
        }
        // 이상적 거리보다 너무 가까우면
        else if (currentDistance < idealDistance - tolerance)
        {
            // 타겟으로부터 후퇴
            Vector3 direction = agentTransform.position - targetTransform.position;
            direction.y = 0;
            agentTransform.position += direction.normalized * moveSpeed * Time.deltaTime;
            agentTransform.rotation = Quaternion.LookRotation(-direction);
            animator.SetFloat("MoveSpeed", 1.0f);
            return NodeState.RUNNING; // 아직 이동 중이므로 RUNNING 반환
        }
        // 이상적인 거리 범위 안에 있으면
        else
        {
            // 이동을 멈춤
            animator.SetFloat("MoveSpeed", 0f);
            return NodeState.SUCCESS; // 목표 달성! 성공 반환
        }
    }
}