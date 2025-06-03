using UnityEngine;

public class MoveAwayNode : Node
{
    private Transform agentTransform;
    private Transform targetTransform;
    private Animator animator;
    private float moveSpeed = 2.0f; // 캐릭터의 후퇴 속도

    public MoveAwayNode(Transform agentTransform, Transform targetTransform)
    {
        this.agentTransform = agentTransform;
        this.targetTransform = targetTransform;
        this.animator = agentTransform.GetComponent<Animator>();
    }

    public override NodeState Evaluate()
    {
        if (targetTransform == null) return NodeState.FAILURE;

        // 목표 반대 방향 계산 (적 -> 나)
        Vector3 direction = agentTransform.position - targetTransform.position;
        direction.y = 0;

        // 캐릭터를 목표 반대 방향으로 이동시키지만, 여전히 적을 바라보게 함
        agentTransform.position += direction.normalized * moveSpeed * Time.deltaTime;
        agentTransform.rotation = Quaternion.LookRotation(-direction); // 적을 바라봄

        // Animator의 MoveSpeed 파라미터에 후퇴 속도 전달 (뒤로 걷는 애니메이션용)
        // 여기서는 앞으로 걷는 것과 구분하기 위해 음수나 다른 값을 사용할 수도 있지만,
        // 일단 1.0f로 설정하여 걷는 애니메이션을 재생합니다.
        animator.SetFloat("MoveSpeed", 1.0f);
        
        return NodeState.SUCCESS;
    }
}