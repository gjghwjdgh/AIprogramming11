using UnityEngine;

// 파일 이름: IsEnemyJustFinishedDefendingNode.cs
public class IsEnemyJustFinishedDefendingNode : Node
{
    private Animator targetAnimator;
    private string defendStateName; // 상대방의 '방어' 애니메이션 상태 이름
    private string idleStateName;   // 상대방의 '기본자세(Idle)' 애니메이션 상태 이름
    // (추가) AI의 CharacterStatus에 'lastTimeEnemyDefended' 같은 변수를 두고 활용 가능

    public IsEnemyJustFinishedDefendingNode(Animator targetAnimator, string defendStateName, string idleStateName)
    {
        this.targetAnimator = targetAnimator;
        this.defendStateName = defendStateName;
        this.idleStateName = idleStateName;
    }

    public override NodeState Evaluate()
    {
        if (targetAnimator == null) return NodeState.FAILURE;

        AnimatorStateInfo currentStateInfo = targetAnimator.GetCurrentAnimatorStateInfo(0);

        // 현재 상대방이 Idle 상태이고, (이전에 방어했다는 추가적인 정보가 있다면 더 좋음)
        // 이전 프레임에 방어 중이었는지 직접 체크는 복잡하므로,
        // 여기서는 "현재 방어 중이 아니고 Idle 상태"인 경우를 빈틈으로 간주하는 간소화된 로직.
        // 좀 더 정확하려면, targetAnimator의 이전 상태를 기억하고 비교해야 합니다.
        // 또는 AI가 "적의 방어"를 감지했을 때 특정 플래그를 잠시 켰다가 끄는 방식도 가능합니다.

        // 현재는 "상대가 현재 방어 상태가 아니고 Idle 상태라면" 빈틈으로 간주
        if (!currentStateInfo.IsName(defendStateName) && currentStateInfo.IsName(idleStateName))
        {
            // 여기에 추가적인 로직: "그리고 바로 직전에 상대가 방어했었는가?"
            // 예를 들어, AI가 최근에 IsEnemyAttackImminentNode(방어 감지)가 아닌,
            // IsEnemyDefendingNode 같은 별도 노드로 상대 방어를 인지했을 때 플래그를 세우는 방식
            // 지금은 단순화하여 이 조건만으로 판단
            return NodeState.SUCCESS;
        }

        return NodeState.FAILURE;
    }
}