// 파일 이름: IsEnemyDefendEndedNode.cs
using UnityEngine;

public class IsEnemyDefendEndedNode : Node
{
    private Animator targetAnimator;
    private string defendEndedStateName; // 적의 '방어 종료 후' 애니메이션 상태 이름

    /// <summary>
    /// 적의 애니메이터가 특정 방어 종료 상태에 있는지 확인하는 조건 노드.
    /// </summary>
    /// <param name="animator">적의 Animator 컴포넌트.</param>
    /// <param name="stateName">적이 방어를 마친 후 진입할 것으로 예상되는 애니메이션 상태의 이름.</param>
    public IsEnemyDefendEndedNode(Animator animator, string stateName)
    {
        this.targetAnimator = animator;
        this.defendEndedStateName = stateName;
    }

    public override NodeState Evaluate()
    {
        if (targetAnimator == null)
        {
            // Debug.LogWarning("IsEnemyDefendEndedNode: Target Animator is null!");
            return NodeState.FAILURE;
        }

        AnimatorStateInfo stateInfo = targetAnimator.GetCurrentAnimatorStateInfo(0);

        // 적의 현재 애니메이터 상태가 방어 종료 상태 이름과 일치하는지 확인
        if (stateInfo.IsName(defendEndedStateName))
        {
            // Debug.Log($"IsEnemyDefendEndedNode: SUCCESS! Target is in '{defendEndedStateName}' state (Defend Ended)");
            return NodeState.SUCCESS;
        }
        
        // Debug.Log($"IsEnemyDefendEndedNode: FAILURE. Target is not in '{defendEndedStateName}' state.");
        return NodeState.FAILURE;
    }
}