using UnityEngine;

// 파일 이름: KickAttackNode.cs
public class KickAttackNode : Node
{
    private Animator animator;
    private CooldownManager cooldownManager;
    private string skillName = "KickAttack"; // 이 노드가 관리할 스킬의 이름
    private float cooldownDuration = 15f;      // 발차기 쿨타임 (BT 초안 문서 기준)

    public KickAttackNode(Transform agentTransform)
    {
        animator = agentTransform.GetComponent<Animator>();
        cooldownManager = agentTransform.GetComponent<CooldownManager>();
    }

    public override NodeState Evaluate()
    {
        // 1. 스킬 사용 전 쿨타임 확인
        if (!cooldownManager.IsCooldownFinished(skillName))
        {
            // Debug.Log(skillName + " 쿨타임 중...");
            return NodeState.FAILURE;
        }

        // 2. 스킬 실행 (애니메이션 재생 등)
        animator.SetTrigger("KickAttack"); // "KickAttack" 파라미터는 Animator Controller에 정의 필요
        Debug.Log(skillName + " 사용!");

        // 3. 스킬 사용 후 쿨타임 시작
        cooldownManager.StartCooldown(skillName, cooldownDuration);

        return NodeState.SUCCESS;
    }
}