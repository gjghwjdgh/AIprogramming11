using UnityEngine;

public class IsEnemyWideOpenNode : Node
{
    private Animator targetAnimator;
    private string[] wideOpenAnimationNames; 
    
    public IsEnemyWideOpenNode(Animator targetAnimator, string[] animationNames)
    {
        this.targetAnimator = targetAnimator;
        this.wideOpenAnimationNames = animationNames;
    }

    public override NodeState Evaluate()
    {
        if (targetAnimator == null || wideOpenAnimationNames == null) return NodeState.FAILURE;

        AnimatorStateInfo stateInfo = targetAnimator.GetCurrentAnimatorStateInfo(0);
        foreach (var name in wideOpenAnimationNames)
        {
            if (stateInfo.IsName(name))
            {
                return NodeState.SUCCESS;
            }
        }
        return NodeState.FAILURE;
    }
}