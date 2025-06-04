using UnityEngine;

public class IsEnemyCritAttackDetectedNode : Node
{
    private Animator targetAnimator;
    private string critAttackAnimationStateName;

    public IsEnemyCritAttackDetectedNode(Animator targetAnimator, string animationName)
    {
        this.targetAnimator = targetAnimator;
        this.critAttackAnimationStateName = animationName;
    }

    public override NodeState Evaluate()
    {
        if (targetAnimator == null) return NodeState.FAILURE;

        if (targetAnimator.GetCurrentAnimatorStateInfo(0).IsName(critAttackAnimationStateName))
        {
            return NodeState.SUCCESS;
        }
        return NodeState.FAILURE;
    }
}