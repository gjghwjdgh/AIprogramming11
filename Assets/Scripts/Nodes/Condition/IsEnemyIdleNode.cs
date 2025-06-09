using UnityEngine;

// 파일 이름: IsEnemyIdleNode.cs
public class IsEnemyIdleNode : Node
{
    private Animator targetAnimator;
    private string idleStateName;

    public IsEnemyIdleNode(Animator targetAnimator, string idleStateName)
    {
        this.targetAnimator = targetAnimator;
        this.idleStateName = idleStateName;
    }

    public override NodeState Evaluate()
    {
        if (targetAnimator == null) return NodeState.FAILURE;

        if (targetAnimator.GetCurrentAnimatorStateInfo(0).IsName(idleStateName))
        {
            return NodeState.SUCCESS;
        }

        return NodeState.FAILURE;
    }
}