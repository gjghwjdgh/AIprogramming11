using UnityEngine;

// 파일 이름: IdleNode.cs
public class IdleNode : Node
{
    private Animator animator;

    // 생성자: AI 캐릭터의 Transform을 받아와 Animator 컴포넌트를 저장합니다.
    public IdleNode(Transform agentTransform)
    {
        // GetComponent는 비용이 있으므로 생성자에서 한 번만 찾아두는 것이 효율적입니다.
        animator = agentTransform.GetComponent<Animator>();
    }

    // 행동 평가 함수
    public override NodeState Evaluate()
    {
        // Animator의 MoveSpeed 파라미터를 0으로 설정하여
        // 이동 애니메이션을 멈추고 Idle 애니메이션으로 전환하도록 합니다.
        if (animator != null)
        {
            animator.SetFloat("MoveSpeed", 0f);
        }

        // '대기' 행동은 항상 즉시 성공한 것으로 처리합니다.
        return NodeState.SUCCESS;
    }
}