// IsEnemyDefendingNode.cs (새로 만들어야 할 수도 있는 파일)
using UnityEngine;
public class IsEnemyDefendingNode : Node
{
    private Animator targetAnimator;
    private string defendStateName;
    public IsEnemyDefendingNode(Animator anim, string name) { this.targetAnimator = anim; this.defendStateName = name; }
    public override NodeState Evaluate()
    {
        if (targetAnimator.GetCurrentAnimatorStateInfo(0).IsName(defendStateName)) return NodeState.SUCCESS;
        return NodeState.FAILURE;
    }
}