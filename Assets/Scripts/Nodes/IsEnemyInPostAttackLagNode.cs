using UnityEngine;

public class IsEnemyInPostAttackLagNode : Node
{
    private Animator targetAnimator;
    // 후딜레이에 해당하는 모든 애니메이션 상태 이름을 배열로 관리할 수 있습니다.
    private string[] lagAnimationNames = { "Enemy_Attack_Lag", "Enemy_Attack_Recovery" };

    public IsEnemyInPostAttackLagNode(Animator targetAnimator)
    {
        this.targetAnimator = targetAnimator;
    }

    public override NodeState Evaluate()
    {
        if (targetAnimator == null) return NodeState.FAILURE;

        AnimatorStateInfo stateInfo = targetAnimator.GetCurrentAnimatorStateInfo(0);

        foreach (var name in lagAnimationNames)
        {
            if (stateInfo.IsName(name))
            {
                // 적이 후딜레이 상태 중 하나라면 성공 반환
                return NodeState.SUCCESS;
            }
        }
        
        return NodeState.FAILURE;
    }
}