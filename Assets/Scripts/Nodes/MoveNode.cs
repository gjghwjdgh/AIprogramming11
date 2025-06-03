using UnityEngine;

public class MoveNode : Node
{
    private Transform agentTransform;
    private Transform targetTransform;
    private Animator animator;
    // private float moveSpeed = 2.0f; // 캐릭터의 이동 속도
    private float moveSpeed = 0.2f; // 캐릭터의 이동 속도

    public MoveNode(Transform agentTransform, Transform targetTransform)
    {
        this.agentTransform = agentTransform;
        this.targetTransform = targetTransform;
        this.animator = agentTransform.GetComponent<Animator>();
    }

    public override NodeState Evaluate()
    {
        if (targetTransform == null) return NodeState.FAILURE;

        // 목표 방향 계산 (Y축은 무시)
        Vector3 direction = targetTransform.position - agentTransform.position;
        direction.y = 0;

        // 캐릭터를 목표 방향으로 이동 및 회전
        agentTransform.position += direction.normalized * moveSpeed * Time.deltaTime;
        agentTransform.rotation = Quaternion.LookRotation(direction);

        // ★★★ 핵심: Animator의 MoveSpeed 파라미터에 현재 속도를 전달 ★★★
        // direction.magnitude가 0보다 크면 걷는 것이고, 0이면 멈춘 것입니다.
        // 여기서는 단순화하여 1.0f (걷는 중)으로 설정합니다.
        animator.SetFloat("MoveSpeed", direction.magnitude > 0.1f ? 1.0f : 0.0f);

        // 이동은 계속 진행되는 행동이므로 RUNNING을 반환할 수 있습니다.
        // 여기서는 단순화를 위해 SUCCESS로 처리합니다.
        return NodeState.SUCCESS;
    }
}