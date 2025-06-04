using UnityEngine;

public class IsCooldownCompleteNode : Node
{
    private CooldownManager cooldownManager;
    private string skillName;

    public IsCooldownCompleteNode(Transform agentTransform, string skillName)
    {
        cooldownManager = agentTransform.GetComponent<CooldownManager>();
        this.skillName = skillName;
    }

    public override NodeState Evaluate()
    {
        // CooldownManager에게 특정 스킬의 쿨타임이 끝났는지 물어봅니다.
        if (cooldownManager.IsCooldownFinished(skillName))
        {
            return NodeState.SUCCESS; // 끝났으면 성공
        }
        else
        {
            return NodeState.FAILURE; // 아직 안 끝났으면 실패
        }
    }
}