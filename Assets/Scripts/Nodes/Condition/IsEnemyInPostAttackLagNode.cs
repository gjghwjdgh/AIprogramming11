using UnityEngine;

public class IsEnemyInPostAttackLagNode : Node
{
    private Animator targetAnimator;
    private string[] lagAnimationNames;

    public IsEnemyInPostAttackLagNode(Animator targetAnimator, string[] animationNames)
    {
        this.targetAnimator = targetAnimator;
        this.lagAnimationNames = animationNames;
    }

    public override NodeState Evaluate()
    {
        if (targetAnimator == null || lagAnimationNames == null) return NodeState.FAILURE;

        AnimatorStateInfo stateInfo = targetAnimator.GetCurrentAnimatorStateInfo(0);
        foreach (var name in lagAnimationNames)
        {
            if (stateInfo.IsName(name))
            {
                return NodeState.SUCCESS;
            }
        }
        return NodeState.FAILURE;
    }
}