using UnityEngine;

// 파일 이름: MaintainDistanceNode.cs
public class MaintainDistanceNode : Node
{
    private Transform agentTransform;
    private Transform targetTransform;
    private Animator animator;
    private float idealDistance;
    private float tolerance; // 이상적 거리의 허용 오차
    private float moveSpeed = 2.0f;

    public MaintainDistanceNode(Transform agentTransform, Transform target, float idealDistance, float tolerance = 0.5f) // tolerance 기본값 설정
    {
        this.agentTransform = agentTransform;
        this.targetTransform = target;
        this.animator = agentTransform.GetComponent<Animator>();
        this.idealDistance = idealDistance;
        this.tolerance = tolerance;
    }

    public override NodeState Evaluate()
    {
        if (targetTransform == null)
        {
            if(animator != null) animator.SetFloat("MoveSpeed", 0f);
            return NodeState.FAILURE;
        }

        float currentDistance = Vector3.Distance(agentTransform.position, targetTransform.position);
        
        if (currentDistance > idealDistance + tolerance)
        {
            Vector3 direction = targetTransform.position - agentTransform.position;
            direction.y = 0;
            agentTransform.position += direction.normalized * moveSpeed * Time.deltaTime;
            agentTransform.rotation = Quaternion.LookRotation(direction);
            if(animator != null) animator.SetFloat("MoveSpeed", 1.0f);
            return NodeState.RUNNING;
        }
        else if (currentDistance < idealDistance - tolerance)
        {
            Vector3 direction = agentTransform.position - targetTransform.position;
            direction.y = 0;
            agentTransform.position += direction.normalized * moveSpeed * Time.deltaTime;
            agentTransform.rotation = Quaternion.LookRotation(-direction); // 후퇴 시에는 적을 바라보도록 수정 가능
            if(animator != null) animator.SetFloat("MoveSpeed", 1.0f);
            return NodeState.RUNNING;
        }
        else
        {
            if(animator != null) animator.SetFloat("MoveSpeed", 0f);
            return NodeState.SUCCESS;
        }
    }
}