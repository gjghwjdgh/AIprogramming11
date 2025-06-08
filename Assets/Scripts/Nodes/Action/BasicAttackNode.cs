// 파일 이름: BasicAttackNode.cs
using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 추가

public class BasicAttackNode : Node
{
    private PaladinActuator actuator;
    private CooldownManager cooldownManager;
    private BT_Aggressive_Paladin btPaladin; // 코루틴 시작을 위해 필요

    private PaladinActuator.AttackType attackType;
    private string skillName;
    private float cooldownDuration;
    private float attackAnimationDuration; // 공격 애니메이션의 실제 길이

    private bool isAttackActive = false; // 공격 행동이 현재 이 노드에 의해 진행 중인지 여부
    private Coroutine attackCoroutine = null; // 실행 중인 코루틴 참조

    // 생성자: 필요한 인자들을 받도록 수정
    public BasicAttackNode(Transform agentTransform, PaladinActuator.AttackType type, string skillName, float cooldownDuration, float animDuration, BT_Aggressive_Paladin btPaladinInstance)
    {
        actuator = agentTransform.GetComponent<PaladinActuator>();
        cooldownManager = agentTransform.GetComponent<CooldownManager>();
        btPaladin = btPaladinInstance;

        this.attackType = type;
        this.skillName = skillName;
        this.cooldownDuration = cooldownDuration;
        this.attackAnimationDuration = animDuration;
    }

    public override NodeState Evaluate()
    {
        if (actuator == null || cooldownManager == null || btPaladin == null) return NodeState.FAILURE;

        // 다른 행동이 이미 진행 중이라면 이 행동을 시작하지 않음 (이 노드 자체가 active가 아닐 때만)
        if (actuator.IsActionInProgress && !isAttackActive)
        {
            // Debug.Log($"BasicAttackNode: Another action ({btPaladin.currentActionName}) is in progress. Returning FAILURE.");
            return NodeState.FAILURE;
        }

        // 이 공격이 이미 활성화되어 진행 중인 경우
        if (isAttackActive && attackCoroutine != null)
        {
            // Debug.Log($"BasicAttackNode: {skillName} - Attack already active. Returning RUNNING.");
            return NodeState.RUNNING; // 계속 RUNNING 반환
        }

        // 쿨타임 체크 (새로 공격을 시작하는 경우)
        if (!cooldownManager.IsCooldownFinished(skillName))
        {
            // Debug.Log($"BasicAttackNode: {skillName} - Cooldown not finished. Returning FAILURE.");
            return NodeState.FAILURE;
        }

        // 새로운 공격 시작
        if (!isAttackActive && attackCoroutine == null)
        {
            actuator.StartAttack(attackType); // 공격 애니메이션 시작
            actuator.OnActionStart(); // IsActionInProgress = true 설정

            cooldownManager.StartCooldown(skillName, cooldownDuration); // 쿨타임 시작

            Debug.Log($"<color=magenta>BasicAttackNode: Initiating {skillName} attack.</color>");

            isAttackActive = true;
            attackCoroutine = btPaladin.StartCoroutine(AttackCoroutineInternal(attackAnimationDuration));
            
            return NodeState.RUNNING; // 공격 시작 및 진행 중
        }

        // 코루틴이 완료되어 공격 행동이 끝난 경우
        if (!isAttackActive && attackCoroutine == null)
        {
            Debug.Log($"<color=green>BasicAttackNode: {skillName} - Attack completed. Returning SUCCESS.</color>");
            return NodeState.SUCCESS; // 공격 행동 완료
        }

        // 예기치 않은 상황
        Debug.LogWarning($"BasicAttackNode: {skillName} - Unexpected state. Returning FAILURE.");
        return NodeState.FAILURE;
    }

    private IEnumerator AttackCoroutineInternal(float duration)
    {
        yield return new WaitForSeconds(duration); // 공격 애니메이션 지속 시간만큼 기다림

        // 공격 종료 처리
        actuator.OnActionEnd(); // IsActionInProgress = false 설정
        Debug.Log($"<color=yellow>BasicAttackNode: {skillName} - Attack duration ended. Setting to inactive.</color>");

        isAttackActive = false;
        attackCoroutine = null; // 코루틴 참조 해제
    }
}