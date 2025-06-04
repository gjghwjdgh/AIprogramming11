using UnityEngine;

public class BasicAttackNode : Node
{
    private Animator animator;
    private CooldownManager cooldownManager;
    private string skillName = "BasicAttack"; // 이 노드가 관리할 스킬의 이름
    private float cooldownDuration = 6f;      // 이 스킬의 쿨타임 시간 [cite: 14, 38]

    public BasicAttackNode(Transform agentTransform)
    {
        animator = agentTransform.GetComponent<Animator>();
        cooldownManager = agentTransform.GetComponent<CooldownManager>(); // CooldownManager 컴포넌트 가져오기
    }

    public override NodeState Evaluate()
    {
        // 1. 스킬 사용 전 쿨타임 확인
        if (!cooldownManager.IsCooldownFinished(skillName))
        {
            // Debug.Log(skillName + " 쿨타임 중..."); // 디버깅용
            return NodeState.FAILURE; // 쿨타임 중이면 실패 반환
        }

        // 2. 스킬 실행 (애니메이션 재생 등)
        animator.SetTrigger(skillName); // "BasicAttack" 파라미터는 Animator Controller에 정의되어 있어야 함
        Debug.Log(skillName + " 사용!");

        // 3. 스킬 사용 후 쿨타임 시작
        cooldownManager.StartCooldown(skillName, cooldownDuration);

        return NodeState.SUCCESS; // 스킬 사용 성공
    }
}