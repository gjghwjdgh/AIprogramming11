using UnityEngine;

public class IsEnemyWideOpenNode : Node
{
    private Animator targetAnimator;
    private string[] wideOpenAnimationNames = { "KnockedDown", "Stunned" };

    public IsEnemyWideOpenNode(Animator targetAnimator)
    {
        this.targetAnimator = targetAnimator;
    }

    public override NodeState Evaluate()
    {
        if (targetAnimator == null) return NodeState.FAILURE;

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