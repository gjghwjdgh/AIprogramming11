using UnityEngine;

// 파일 이름: SpinAttackNode.cs
public class SpinAttackNode : Node
{
    private Animator animator;
    private CooldownManager cooldownManager;
    private string skillName = "SpinAttack";
    private float cooldownDuration = 30f;     // 회전베기 쿨타임 (BT 초안 문서 기준)

    public SpinAttackNode(Transform agentTransform)
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

        animator.SetTrigger("SpinAttack"); // "SpinAttack" 파라미터는 Animator Controller에 정의 필요
        Debug.Log(skillName + " 사용!");

        cooldownManager.StartCooldown(skillName, cooldownDuration);

        return NodeState.SUCCESS;
    }
}