using UnityEngine;

// 파일 이름: IsTargetNotAttackingOrDefendingNode.cs
public class IsTargetNotAttackingOrDefendingNode : Node
{
    private Animator targetAnimator;
    private string[] activeStateNames; // 상대방의 공격 또는 방어 애니메이션 상태 이름들

    public IsTargetNotAttackingOrDefendingNode(Animator targetAnim, params string[] statesToConsiderActive)
    {
        this.targetAnimator = targetAnim;
        this.activeStateNames = statesToConsiderActive;
    }

    public override NodeState Evaluate()
    {
        if (targetAnimator == null)
        {
            // 타겟 애니메이터가 없으면, 일단 안전하다고 (공격/방어 중이 아니라고) 판단할 수 있음
            // 또는 상황에 따라 FAILURE를 반환할 수도 있음
            return NodeState.SUCCESS;
        }

        AnimatorStateInfo stateInfo = targetAnimator.GetCurrentAnimatorStateInfo(0);

        foreach (string stateName in activeStateNames)
        {
            if (stateInfo.IsName(stateName))
            {
                // 상대가 주어진 활성 상태 (공격, 방어 등) 중 하나라면, 이 조건은 실패
                return NodeState.FAILURE;
            }
        }

        // 상대가 주어진 어떤 활성 상태도 아니라면, 이 조건은 성공
        return NodeState.SUCCESS;
    }
}