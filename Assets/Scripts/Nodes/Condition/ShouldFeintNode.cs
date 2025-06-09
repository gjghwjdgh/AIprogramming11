using UnityEngine;

// 파일 이름: ShouldFeintNode.cs
public class ShouldFeintNode : Node
{
    private Transform agentTransform;
    private Transform targetTransform;
    private Animator targetAnimator;
    private CooldownManager cooldownManager;
    private string feintType; // "Aggressive", "Defensive" 등 간 보기 타입
    private string enemyIdleStateName; // 타겟의 Idle 상태 이름
    private IPaladinParameters aiParameters; // 인터페이스 타입으로 변경


    public ShouldFeintNode(Transform agentTransform, Transform target, string feintType)
    {
        this.agentTransform = agentTransform;
        this.targetTransform = target;
        this.targetAnimator = target.GetComponent<Animator>(); // 타겟의 Animator 직접 참조
        this.cooldownManager = agentTransform.GetComponent<CooldownManager>();
        this.feintType = feintType;

        this.aiParameters = agentTransform.GetComponent<IPaladinParameters>();

        if (this.aiParameters != null)
        {
            this.enemyIdleStateName = this.aiParameters.idleStateName;
        }
        else
        {
            this.enemyIdleStateName = "기본자세"; // 기본값
            Debug.LogError("ShouldFeintNode: IPaladinParameters 컴포넌트를 찾을 수 없어 enemyIdleStateName 기본값을 사용합니다.");
        }
    }

    public override NodeState Evaluate()
    {
        // ▼▼▼ aiParameters null 체크 필수! ▼▼▼
        if (targetTransform == null || targetAnimator == null || cooldownManager == null || aiParameters == null)
        {
            return NodeState.FAILURE;
        }

        // 예시: 공격적인 간 보기 조건 (Aggressive Feint)
        if (feintType == "Aggressive")
        {
            // 1. 상대방이 현재 특별한 행동(공격, 방어 등)을 하지 않고 Idle 상태인가?
            bool enemyIsIdle = targetAnimator.GetCurrentAnimatorStateInfo(0).IsName(enemyIdleStateName);
            bool attackReady = false;
            // (primaryAttackSkills 정의 필요 또는 생성자에서 받아오기)
            string[] primaryAttackSkills = { "BasicAttack", "KickAttack" };
            foreach (string skill in primaryAttackSkills)
            {
                if (cooldownManager.IsCooldownFinished(skill))
                {
                    attackReady = true;
                    break;
                }
            }


            float distance = Vector3.Distance(agentTransform.position, targetTransform.position);
            // ▼▼▼ 인터페이스를 통해 파라미터 접근 ▼▼▼
            bool inFeintRange = distance < aiParameters.optimalCombatDistanceMax + 1.0f && distance > aiParameters.optimalCombatDistanceMin - 0.5f;

            if (enemyIsIdle && attackReady && inFeintRange)
            {
                return NodeState.SUCCESS;
            }
        }
        return NodeState.FAILURE;
    }
}