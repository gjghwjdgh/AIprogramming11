// 파일 이름: DefendNode.cs
using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 추가

public class DefendNode : Node
{
    private PaladinActuator actuator;
    private CooldownManager cooldownManager;
    private BT_Aggressive_Paladin btPaladin; // BT_Aggressive_Paladin 참조 추가 (코루틴 시작용)

    private string skillName = "Defend";
    private float cooldownDuration = 6f; // 방어 스킬 자체의 쿨타임
    private float defendActiveDuration = 3f; // 방어 행동이 실제로 활성화되는 시간 (3초로 통일)

    private bool isDefendingActive = false; // 방어 행동이 현재 이 노드에 의해 진행 중인지 여부
    private Coroutine defendCoroutine = null; // 실행 중인 코루틴 참조

    public DefendNode(Transform agentTransform, BT_Aggressive_Paladin btPaladinInstance)
    {
        actuator = agentTransform.GetComponent<PaladinActuator>();
        cooldownManager = agentTransform.GetComponent<CooldownManager>();
        btPaladin = btPaladinInstance;
    }

    public override NodeState Evaluate()
    {
        if (actuator == null || cooldownManager == null || btPaladin == null)
        {
            Debug.LogError("DefendNode: Missing component(s) or BT_Aggressive_Paladin reference.");
            return NodeState.FAILURE;
        }

        // 다른 행동이 이미 진행 중이라면 이 행동을 시작하지 않음 (이 노드 자체가 active가 아닐 때만)
        if (actuator.IsActionInProgress && !isDefendingActive)
        {
            // Debug.Log($"DefendNode: Another action ({btPaladin.currentActionName}) is in progress. Returning FAILURE.");
            return NodeState.FAILURE;
        }

        // 1. 방어 쿨타임 체크
        if (!cooldownManager.IsCooldownFinished(skillName))
        {
            // Debug.Log("DefendNode: Cooldown is not finished. Returning FAILURE.");
            return NodeState.FAILURE;
        }

        // 2. 방어 행동이 이미 활성화되어 진행 중인 경우 (이 노드가 이미 시작시킨 방어 행동)
        if (isDefendingActive && defendCoroutine != null)
        {
            // Debug.Log("DefendNode: Defend action already active. Returning RUNNING.");
            return NodeState.RUNNING; // 계속 RUNNING 반환
        }

        // 3. 새로운 방어 행동 시작
        if (!isDefendingActive && defendCoroutine == null)
        {
            cooldownManager.StartCooldown(skillName, cooldownDuration); // 쿨타임 시작
            actuator.SetDefend(true); // Animator의 "isDefending" bool 파라미터를 true로 설정
            actuator.OnActionStart(); // 행동 시작 알림 (IsActionInProgress = true)

            Debug.Log($"<color=cyan>DefendNode: Initiating Defend for {defendActiveDuration} seconds.</color>");

            isDefendingActive = true;
            defendCoroutine = btPaladin.StartCoroutine(DefendCoroutineInternal());

            return NodeState.RUNNING; // 방어 행동이 시작되었고 진행 중
        }
        
        // 4. 코루틴이 완료되어 방어 행동이 끝난 경우 (Evaluate가 다시 호출될 때)
        if (!isDefendingActive && defendCoroutine == null)
        {
            Debug.Log("<color=green>DefendNode: Defend action completed. Returning SUCCESS.</color>");
            return NodeState.SUCCESS; // 방어 행동 완료
        }

        // 예기치 않은 상황
        Debug.LogWarning("DefendNode: Unexpected state. Returning FAILURE.");
        return NodeState.FAILURE;
    }

    private IEnumerator DefendCoroutineInternal()
    {
        yield return new WaitForSeconds(defendActiveDuration); // 지정된 시간 동안 기다림

        actuator.SetDefend(false); // Animator의 "isDefending" bool 파라미터를 false로 설정
        actuator.OnActionEnd(); // 행동 종료 알림 (IsActionInProgress = false)

        Debug.Log("<color=yellow>DefendCoroutine: Defend duration ended. Setting to inactive.</color>");
        
        isDefendingActive = false;
        defendCoroutine = null; // 코루틴 참조 해제
    }
}