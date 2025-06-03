using UnityEngine;

public class IsEnemyShowingSmallOpeningNode : Node
{
    private Animator targetAnimator;
    private string[] smallOpeningAnimationNames = { "Enemy_Attack_Recovery_Short" };

    public IsEnemyShowingSmallOpeningNode(Animator targetAnimator)
    {
        this.targetAnimator = targetAnimator;
    }

    public override NodeState Evaluate()
    {
        if (targetAnimator == null) return NodeState.FAILURE;

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