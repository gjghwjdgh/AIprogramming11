using UnityEngine;

// 파일 이름: IsSafeToAttackNode.cs
public class IsSafeToAttackNode : Node
{
    private CharacterStatus status;
    private Animator targetAnimator;
    private float healthThreshold;
    private string[] enemyAttackingStates = { "Enemy_Attack", "Enemy_Critical_Strike" }; // 적의 공격 애니메이션 상태 이름들

    public IsSafeToAttackNode(Transform agentTransform, Animator targetAnimator, float healthThreshold)
    {
        this.status = agentTransform.GetComponent<CharacterStatus>();
        this.targetAnimator = targetAnimator;
        this.healthThreshold = healthThreshold;
    }

    public override NodeState Evaluate()
    {
        // 1. 내 체력이 설정한 기준치 이하이면 안전하지 않음 (실패)
        if (status.currentHealth < healthThreshold)
        {
            return NodeState.FAILURE;
        }

        // 2. 상대방이 현재 공격 중이라면 안전하지 않음 (실패)
        if (targetAnimator != null)
        {
            AnimatorStateInfo stateInfo = targetAnimator.GetCurrentAnimatorStateInfo(0);
            foreach (var stateName in enemyAttackingStates)
            {
                if (stateInfo.IsName(stateName))
                {
                    return NodeState.FAILURE;
                }
            }
        }
        
        // 모든 안전 조건을 통과하면 성공
        return NodeState.SUCCESS;
    }
}