using UnityEngine;
using System.Collections;

// 파일 이름: EvadeNode.cs
public class EvadeNode : Node
{
    private Transform agentTransform;
    private Animator animator;
    private CooldownManager cooldownManager;
    private MonoBehaviour monoBehaviour;
    private string direction;
    private float evadeDistance = 2f;
    private float evadeDuration = 1.633f; // 이 시간은 실제 Evade 애니메이션 길이와 유사해야 함

    private bool isEvading = false; // 현재 회피 동작이 진행 중인지 나타내는 플래그

    public EvadeNode(Transform agentTransform, string direction = "Backward")
    {
        this.agentTransform = agentTransform;
        this.animator = agentTransform.GetComponent<Animator>();
        this.cooldownManager = agentTransform.GetComponent<CooldownManager>();
        this.monoBehaviour = agentTransform.GetComponent<MonoBehaviour>();
        this.direction = direction.ToLower();

        if (this.monoBehaviour == null)
        {
            Debug.LogError("EvadeNode: AI 캐릭터에 MonoBehaviour를 상속받는 스크립트가 필요합니다!");
        }
    }

    public override NodeState Evaluate()
    {
        if (monoBehaviour == null) return NodeState.FAILURE;

        // 이미 회피 코루틴이 실행 중이라면 계속 RUNNING 반환
        if (isEvading)
        {
            return NodeState.RUNNING;
        }

        // (중요) 쿨타임 체크는 BT_Paladin의 IsCooldownCompleteNode에서 이미 수행했다고 가정합니다.
        // 만약 이 노드에서 직접 쿨타임을 체크하고 싶다면 여기에 추가해야 합니다.

        monoBehaviour.StartCoroutine(EvadeCoroutine());
        animator.SetTrigger("Evade");

        // 쿨타임은 회피 동작 시작과 함께 바로 적용
        Debug.Log("Evade (" + direction + ") 코루틴 시작! 쿨타임 시작.");
        cooldownManager.StartCooldown("Evade", 10f);

        isEvading = true; // 회피 시작 플래그 설정
        return NodeState.RUNNING; // 회피 동작이 진행 중임을 알림
    }

    private IEnumerator EvadeCoroutine()
    {
        float elapsedTime = 0f;
        Vector3 startPosition = agentTransform.position;
        Vector3 targetDirection = GetDirectionVector();
        Vector3 targetPosition = startPosition + targetDirection * evadeDistance;

        // 애니메이션 상태가 실제로 "Evade"로 전환될 때까지 잠시 기다릴 수 있습니다. (선택적)
        // while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Evade")) // "Evade"는 실제 Evade 애니메이션 상태 이름
        // {
        //     yield return null;
        // }

        while (elapsedTime < evadeDuration)
        {
            agentTransform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / evadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        agentTransform.position = targetPosition; // 정확한 위치로 보정
        Debug.Log("Evade (" + direction + ") 코루틴 완료. 최종 위치: " + agentTransform.position);
        isEvading = false; // 회피 완료 플래그 해제
    }

    private Vector3 GetDirectionVector()
    {
        switch (direction)
        {
            case "forward":
                return agentTransform.forward;
            case "backward":
                return -agentTransform.forward;
            case "left":
                return -agentTransform.right;
            case "right":
                return agentTransform.right;
            default:
                return -agentTransform.forward;
        }
    }
}