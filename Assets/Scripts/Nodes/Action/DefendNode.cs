using UnityEngine;

// 파일 이름: DefendNode.cs
public class DefendNode : Node
{
    private Animator animator;
    private CooldownManager cooldownManager;
    private string skillName = "Defend";
    private float cooldownDuration = 6f; // 방어 쿨타임 (BT 초안 문서 기준) [cite: 26]

    public DefendNode(Transform agentTransform)
    {
        animator = agentTransform.GetComponent<Animator>();
        cooldownManager = agentTransform.GetComponent<CooldownManager>();
    }

    public override NodeState Evaluate()
    {
        if (!cooldownManager.IsCooldownFinished(skillName))
        {
            // Debug.Log(skillName + " 쿨타임 중...");
            return NodeState.FAILURE;
        }

        // "Defend" 애니메이션 트리거 또는 Bool 파라미터 설정
        // 여기서는 트리거로 가정. 만약 방어 상태를 유지해야 한다면 Bool 파라미터를 사용하고
        // 별도의 조건으로 방어 해제 노드를 만들어야 합니다.
        animator.SetTrigger("Defend");
        Debug.Log(skillName + " 사용!");

        cooldownManager.StartCooldown(skillName, cooldownDuration);

        return NodeState.SUCCESS; // 방어 자세를 취하는 행동은 일단 성공으로 처리
    }
}