using UnityEngine;

public class IsEnemyShowingSmallOpeningNode : Node
{
    private Animator targetAnimator;
    private string[] smallOpeningAnimationNames;

    public IsEnemyShowingSmallOpeningNode(Animator targetAnimator, string[] animationNames)
    {
        this.targetAnimator = targetAnimator;
        this.smallOpeningAnimationNames = animationNames;
    }

    public override NodeState Evaluate()
    {
        if (targetAnimator == null || smallOpeningAnimationNames == null) return NodeState.FAILURE;

        AnimatorStateInfo stateInfo = targetAnimator.GetCurrentAnimatorStateInfo(0);
        foreach (var name in smallOpeningAnimationNames)
        {
            if (stateInfo.IsName(name))
            {
                return NodeState.SUCCESS;
            }
        }
        return NodeState.FAILURE;
    }
}