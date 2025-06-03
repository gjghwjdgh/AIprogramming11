using UnityEngine;

public class IsEnemyCritAttackDetectedNode : Node
{
    private Animator targetAnimator;
    private string critAttackAnimationStateName; // 확인할 상대방의 '치명타' 애니메이션 상태 이름

    public IsEnemyCritAttackDetectedNode(Animator targetAnimator, string animationName)
    {
        this.targetAnimator = targetAnimator;
        this.critAttackAnimationStateName = animationName;
    }

    public override NodeState Evaluate()
    {
        if (targetAnimator == null) return NodeState.FAILURE;

        // 상대방이 현재 '치명타 공격' 애니메이션을 재생 중인지 확인합니다.
        if (targetAnimator.GetCurrentAnimatorStateInfo(0).IsName(critAttackAnimationStateName))
        {
            return NodeState.SUCCESS;
        }

        return NodeState.FAILURE;
    }
}