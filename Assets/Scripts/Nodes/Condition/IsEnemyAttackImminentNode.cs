using UnityEngine;

public class IsEnemyAttackImminentNode : Node
{
    private Animator targetAnimator;
    private string attackAnimationStateName;

    public IsEnemyAttackImminentNode(Animator targetAnimator, string animationName)
    {
        this.targetAnimator = targetAnimator;
        this.attackAnimationStateName = animationName;
    }

    public override NodeState Evaluate()
    {
        if (targetAnimator == null) return NodeState.FAILURE;

        if (targetAnimator.GetCurrentAnimatorStateInfo(0).IsName(attackAnimationStateName))
        {
            return NodeState.SUCCESS;
        }
        return NodeState.FAILURE;
    }
}