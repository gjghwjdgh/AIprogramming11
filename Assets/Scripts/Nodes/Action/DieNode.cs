using UnityEngine;

public class DieNode : Node
{
    private Animator animator;

    public DieNode(Transform agentTransform)
    {
        animator = agentTransform.GetComponent<Animator>();
    }

    public override NodeState Evaluate()
    {
        // "Die"라는 이름의 애니메이션 트리거를 발동시킵니다.
        // Animator Controller에 "Die" 트리거 파라미터와 사망 애니메이션 상태가 준비되어 있어야 합니다.
        animator.SetTrigger("Die");
        return NodeState.SUCCESS;
    }
}