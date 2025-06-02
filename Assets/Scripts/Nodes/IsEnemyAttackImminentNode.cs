using UnityEngine;

public class IsEnemyAttackImminentNode : Node
{
    private Animator targetAnimator;
    private string attackAnimationStateName; // 확인할 상대방의 공격 애니메이션 상태 이름

    public IsEnemyAttackImminentNode(Animator targetAnimator, string animationName)
    {
        this.targetAnimator = targetAnimator;
        this.attackAnimationStateName = animationName;
    }

    public override NodeState Evaluate()
    {
        if (targetAnimator == null) return NodeState.FAILURE;

        // 상대방 Animator의 현재 애니메이션 상태 정보가 우리가 찾는 공격 상태 이름과 일치하는지 확인합니다.
        // GetCurrentAnimatorStateInfo(0)의 '0'은 베이스 레이어를 의미합니다.
        if (targetAnimator.GetCurrentAnimatorStateInfo(0).IsName(attackAnimationStateName))
        {
            // 상대가 현재 그 공격 애니메이션을 재생 중이라면 성공을 반환합니다.
            return NodeState.SUCCESS;
        }

        return NodeState.FAILURE;
    }
}