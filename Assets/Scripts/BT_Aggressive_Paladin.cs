using UnityEngine;
using System.Collections.Generic;

public class BT_Aggressive_Paladin : MonoBehaviour, IPaladinParameters
{
    [Header("Debugging")]
    public bool enableDebugLog = true; // 디버그 로그 활성화 여부
    public float debugLogInterval = 0.5f; // 로그 출력 간격 (0.5초)
    private float debugTimer; // 마지막 로그 출력 후 지난 시간을 저장

    // AI가 적으로 인식할 대상 (인스펙터에서 할당)
    public Transform target;
    public Animator targetAnimator;

    // 행동 트리의 최상위 루트 노드
    private Node root;

    // AI의 성향 및 판단 기준 값
    [Header("AI Behavior Parameters")]
    public float criticalHealthThreshold = 20f;
    public float lowHealthThreshold = 35f;
    public float engageDistance = 12f;          // 이 거리 안으로 들어오면 교전 고려 시작
    public float optimalCombatDistanceMin = 1.0f; // 선호하는 최소 교전 거리 (공격 시작 가능 거리)
    public float optimalCombatDistanceMax = 3.0f; // 선호하는 최대 교전 거리 (이 이상 벌어지면 접근 시도)
    public float tooCloseDistance = 0.8f;         // 너무 가까워서 뒤로 물러나야 하는 거리

    [Header("Attack Range Parameters")]
    public float basicAttackRange = 2.0f;     // 기본 공격 유효 사거리
    public float kickAttackRange = 1.8f;      // 발차기 공격 유효 사거리
    public float spinAttackRange = 2.5f;      // 회전베기 공격 유효 사거리

    // 상대방(플레이어)의 애니메이션 상태 이름
    [Header("Opponent Animation State Names")]
    public string normalAttackStateName = "공격0";
    public string criticalAttackStateName = "공격1";
    public string defendStateName = "방어";
    public string opponentIdleStateName = "기본자세"; // 인터페이스 호환용
    public string[] postAttackLagStateNames = { "기본자세" };
    public string[] wideOpenStateNames = { "Stunned", "KnockedDown" };


    // --- IPaladinParameters 인터페이스 멤버 구현 ---
    // 이 프로퍼티들은 ShouldFeintNode 등에서 공통적으로 사용될 수 있습니다.
    // 현재 BT_Aggressive_Paladin 내부 로직에서는 위에 선언된 public 변수들을 직접 사용합니다.
    // 만약 인터페이스를 통해 접근해야 하는 노드가 있다면, 이 프로퍼티들이 해당 public 변수를 반환하도록 합니다.
    float IPaladinParameters.optimalCombatDistanceMin { get { return this.optimalCombatDistanceMin; } }
    float IPaladinParameters.optimalCombatDistanceMax { get { return this.optimalCombatDistanceMax; } }
    string IPaladinParameters.idleStateName { get { return this.opponentIdleStateName; } }
    // --- 인터페이스 멤버 구현 끝 ---

    private PaladinActuator actuator;
    
    private CooldownManager cooldownManager; // 최적화를 위해 Start에서 한 번만 가져옴

    void Awake()
    {
        cooldownManager = GetComponent<CooldownManager>();
        if (cooldownManager == null)
        {
            Debug.LogError("BT_Aggressive_Paladin: CooldownManager 컴포넌트가 없습니다!");
        }

        actuator = GetComponent<PaladinActuator>();
        if (actuator == null)
        {
            Debug.LogError("BT_Aggressive_Paladin: PaladinActuator 컴포넌트가 없습니다!");
        }
    }

    void Start()
    {
        // 최상위 노드: 여러 행동 중 하나를 선택하는 Selector
        root = new Selector(new List<Node>
        {
            // --- 최우선 순위 1: 생존 및 긴급 회피 ---
            new Selector(new List<Node>
            {
                // 1-1. 사망 처리
                new Sequence(new List<Node>
                {
                    new IsHealthLowNode(transform, 0f), // 체력이 0 이하인가?
                    new DieNode(transform)              // 그렇다면 사망
                }),

                // 1-2. 적의 치명적인 공격에 대한 긴급 회피
                new Sequence(new List<Node>
                {
                    new IsEnemyCritAttackDetectedNode(targetAnimator, criticalAttackStateName), // 적이 치명타를 쓰나?
                    new IsCooldownCompleteNode(transform, "Evade"),                            // 회피 쿨타임은?
                    new EvadeNode(transform, "Backward")                                         // 그렇다면 뒤로 회피
                })
            }),

            // --- 우선 순위 2: 기회 포착 및 강력한 공격 ---
            new Sequence(new List<Node>
            {
                new IsEnemyWideOpenNode(targetAnimator, wideOpenStateNames), // 적이 큰 빈틈(스턴 등)을 보였나?
                new IsEnemyInDistanceNode(transform, target, spinAttackRange), // 스핀 공격 사거리인가?
                new IsCooldownCompleteNode(transform, "SpinAttack"),         // 스핀 공격 쿨타임은?
                new SpinAttackNode(transform)                                // 그렇다면 필살기!
            }),
            
            // --- 우선 순위 3: 일반 위협에 대한 방어 ---
            new Sequence(new List<Node>
            {
                new IsEnemyAttackImminentNode(targetAnimator, normalAttackStateName), // 적이 일반 공격을 하나?
                new IsHealthHighEnoughToDefendNode(transform, lowHealthThreshold),    // 방어할 체력은 되나?
                new IsCooldownCompleteNode(transform, "Defend"),                      // 방어 쿨타임은?
                new IsEnemyInDistanceNode(transform, target, basicAttackRange),       // 공격이 닿을 거리인가?
                new DefendNode(transform)                                             // 그렇다면 방어
            }),

            // --- 우선 순위 4: 위치 선정 ---
            new Selector(new List<Node>
            {
                // 4-1. 너무 가까우면 거리 벌리기
                new Sequence(new List<Node>
                {
                    new IsEnemyInDistanceNode(transform, target, tooCloseDistance), // 너무 가까운가?
                    new EvadeNode(transform, "Backward")                            // 뒤로 회피해서 거리 벌리기
                }),

                // 4-2. 너무 멀면 거리 좁히기
                new Sequence(new List<Node>
                {
                    new IsNotInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMin, optimalCombatDistanceMax), // 최적 교전 거리를 벗어났는가?
                    new MaintainDistanceNode(transform, target, (optimalCombatDistanceMin + optimalCombatDistanceMax) / 2f, 0.5f) // 최적 거리로 이동
                })
            }),

            // --- 우선 순위 5: 최적 거리에서의 공격적인 운영 ---
            // 이 모든 행동은 '최적 교전 거리 안'에 있을 때만 고려함
            new Sequence(new List<Node>
            {
                new IsInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMin, optimalCombatDistanceMax),
                new Selector(new List<Node> // 다양한 공격 패턴 중 하나를 선택
                {
                    // 5-1. 발차기로 압박
                    new Sequence(new List<Node>
                    {
                        new IsEnemyInDistanceNode(transform, target, kickAttackRange),
                        new IsSafeToAttackNode(transform, targetAnimator, lowHealthThreshold),
                        new IsCooldownCompleteNode(transform, "KickAttack"),
                        new KickAttackNode(transform)
                    }),

                    // 5-2. 주도적인 기본 공격
                    new Sequence(new List<Node>
                    {
                        new IsEnemyInDistanceNode(transform, target, basicAttackRange),
                        new IsSafeToAttackNode(transform, targetAnimator, lowHealthThreshold),
                        new IsCooldownCompleteNode(transform, "BasicAttack"),
                        new BasicAttackNode(transform)
                    })
                })
            }),

            // --- 최후 순위: 능동적인 대기 (멍때리기 방지) ---
            // 위의 모든 행동을 할 수 없을 때, 가만히 있지 않고 계속 움직임
            new Selector(new List<Node>
            {
                // 6-1. 60% 확률로 좌우로 움직이며 간 보기
                new Sequence(new List<Node>
                {
                    new RandomChanceNode(0.6f),
                    new IsCooldownCompleteNode(transform, "Strafe"), // 좌우 움직임에도 짧은 쿨타임
                    // new StrafeNode(transform) // "좌우로 움직여라"는 새 액션 노드 (필요 시 제작)
                    // StrafeNode가 없다면 아래의 FeintStep으로 대체 가능
                    new FeintStepNode(transform, Random.value > 0.5f ? "LeftStep" : "RightStep")
                }),

                // 6-2. 나머지 확률로 그냥 대기
                new IdleNode(transform)
            })
        });
    }

    void Update()
    {
        if (root != null && actuator != null)
        {
            actuator.SetDefend(false);
            actuator.SetMovement(0);

            root.Evaluate();
        }
        
        if (enableDebugLog && target != null)
        {
            debugTimer += Time.deltaTime; // 매 프레임 시간 추가
            if (debugTimer >= debugLogInterval) // 설정된 간격이 되면
            {
                PrintDebugStatus(); // 로그 출력
                debugTimer = 0f; // 타이머 초기화
            }
        }
    }

        void PrintDebugStatus()
    {
        // 현재 상태를 문자열로 조합
        string status = $"--- AI DEBUG STATUS ({gameObject.name}) ---\n";

        // 1. 거리 정보
        float distance = Vector3.Distance(transform.position, target.position);
        status += $"Distance to Target: {distance:F2} m\n";

        // 2. 타겟의 애니메이터 상태 정보
        if (targetAnimator != null)
        {
            // 주요 상태들의 활성화 여부를 확인하여 현재 상태 추정
            string targetState = "Unknown";
            if (targetAnimator.GetCurrentAnimatorStateInfo(0).IsName(normalAttackStateName)) targetState = "Normal Attack";
            else if (targetAnimator.GetCurrentAnimatorStateInfo(0).IsName(criticalAttackStateName)) targetState = "Critical Attack";
            else if (targetAnimator.GetCurrentAnimatorStateInfo(0).IsName(defendStateName)) targetState = "Defending";
            else if (targetAnimator.GetCurrentAnimatorStateInfo(0).IsName(opponentIdleStateName)) targetState = "Idle";
            status += $"Target Animator State: {targetState}\n";
        }

        // 3. 주요 스킬 쿨타임 정보
        if (cooldownManager != null)
        {
            status += "-- Cooldowns --\n";
            status += $"Basic Attack: {(cooldownManager.IsCooldownFinished("BasicAttack") ? "Ready" : "On Cooldown")}\n";
            status += $"Kick Attack: {(cooldownManager.IsCooldownFinished("KickAttack") ? "Ready" : "On Cooldown")}\n";
            status += $"Spin Attack: {(cooldownManager.IsCooldownFinished("SpinAttack") ? "Ready" : "On Cooldown")}\n";
            status += $"Evade: {(cooldownManager.IsCooldownFinished("Evade") ? "Ready" : "On Cooldown")}\n";
        }
        
        status += "------------------------------------------------";

        // 최종적으로 조합된 문자열을 로그로 출력
        Debug.Log(status);
    }
}