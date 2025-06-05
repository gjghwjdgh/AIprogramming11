using UnityEngine;
using System.Collections.Generic; // List 사용을 위해 추가

// 파일 이름: RandomAttackNode.cs
public class RandomAttackNode : Node
{
    private Transform agentTransform;
    private Animator animator;
    private CooldownManager cooldownManager;
    private string[] attackSkillNames; // 실행할 공격 스킬 이름 배열
    // 각 스킬에 대한 쿨타임은 해당 스킬의 ActionNode에 정의되어 있다고 가정
    // 또는 여기에 각 스킬별 쿨타임 정보를 Dictionary 등으로 저장할 수도 있음

    public RandomAttackNode(Transform agentTransform, string[] attackSkillNames)
    {
        this.agentTransform = agentTransform;
        this.animator = agentTransform.GetComponent<Animator>();
        this.cooldownManager = agentTransform.GetComponent<CooldownManager>();
        this.attackSkillNames = attackSkillNames;
    }

    public override NodeState Evaluate()
    {
        if (attackSkillNames == null || attackSkillNames.Length == 0)
        {
            return NodeState.FAILURE; // 공격할 스킬이 없음
        }

        List<string> availableSkills = new List<string>();
        foreach (string skillName in attackSkillNames)
        {
            if (cooldownManager.IsCooldownFinished(skillName))
            {
                availableSkills.Add(skillName);
            }
        }

        if (availableSkills.Count == 0)
        {
            // Debug.Log("RandomAttackNode: 모든 스킬이 쿨타임입니다.");
            return NodeState.FAILURE; // 사용 가능한 스킬이 없음
        }

        // 사용 가능한 스킬 중 하나를 무작위로 선택
        string selectedSkill = availableSkills[Random.Range(0, availableSkills.Count)];

        // 선택된 스킬 실행 (애니메이션 트리거 발동)
        // 각 스킬에 대한 구체적인 애니메이션 트리거 이름이 필요합니다.
        // 여기서는 스킬 이름과 애니메이션 트리거 이름이 같다고 가정합니다.
        animator.SetTrigger(selectedSkill);

        // 해당 스킬의 쿨타임 시작 (쿨타임 값은 각 스킬 노드 또는 CooldownManager에서 관리)
        // 여기서는 각 스킬별 쿨타임 값을 알아야 하므로, 실제로는 BasicAttackNode 등을 직접 호출하거나,
        // CooldownManager에 스킬별 쿨타임 정보를 등록해두고 사용해야 합니다.
        // 이 예제에서는 CooldownManager가 스킬 이름만으로 쿨타임을 시작할 수 있다고 가정하지 않으므로,
        // 이 부분은 해당 스킬의 ActionNode가 실행될 때 처리되도록 하거나,
        // CooldownManager에 스킬별 기본 쿨타임을 등록하고 여기서 StartCooldown을 호출하도록 확장해야 합니다.
        // 지금은 ActionNode에서 쿨타임을 관리하므로, 여기서는 애니메이션만 발동시킵니다.
        // (올바르게 하려면, 이 노드가 BasicAttackNode나 KickAttackNode 자체를 실행해야 함)

        // 더 나은 구현: 이 노드는 선택된 스킬에 해당하는 'ActionNode' 자체를 실행해야 합니다.
        // 예를 들어, selectedSkill이 "BasicAttack"이면 BasicAttackNode를 실행.
        // 하지만 현재 구조에서는 이 노드가 다른 노드를 직접 실행할 수 없으므로,
        // BT_Paladin.cs에서 RandomAttackNode 대신 Selector와 Random Decorator를 쓰는 것이 더 일반적입니다.

        // 현재는 애니메이션 트리거만 발동하고, 쿨타임은 각 공격 노드에서 관리한다고 가정합니다.
        // 만약 이 노드에서 직접 쿨타임을 시작하려면, 스킬별 쿨타임 값을 알아야 합니다.
        // 예를 들어, BT 초안 문서의 값을 사용한다면:
        float skillCooldown = 2f; // 기본값
        if (selectedSkill == "KickAttack") skillCooldown = 5f;
        else if (selectedSkill == "SpinAttack") skillCooldown = 10f; // SpinAttack은 이 랜덤 목록에 없지만 예시로
        // else if (selectedSkill == "BasicAttack") skillCooldown = 2f; // 이미 기본값
        
        cooldownManager.StartCooldown(selectedSkill, skillCooldown); // 스킬 이름과 맞는 쿨타임 적용

        Debug.Log("RandomAttackNode: " + selectedSkill + " 사용!");
        return NodeState.SUCCESS;
    }
}