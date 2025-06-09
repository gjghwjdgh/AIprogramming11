// 파일 이름: EvadeNode.cs (최종 완성 버전)
using UnityEngine;
using System.Collections;

public class EvadeNode : Node
{
    private Transform agentTransform;
    private PaladinActuator actuator;
    private CooldownManager cooldownManager;
    private MonoBehaviour coroutineRunner;
    private string direction;

    private bool isEvading = false;
    private float evadeAnimationLength = 1.0f; // 회피 애니메이션의 총 길이
    private float evadeCooldown = 5f; // 회피 후 쿨타임 (5초는 예시)

    public EvadeNode(Transform agentTransform, string direction)
    {
        this.agentTransform = agentTransform;
        this.actuator = agentTransform.GetComponent<PaladinActuator>();
        this.cooldownManager = agentTransform.GetComponent<CooldownManager>();
        this.coroutineRunner = agentTransform.GetComponent<MonoBehaviour>();
        this.direction = direction;
    }

    public override NodeState Evaluate()
    {
        if (isEvading)
        {
            return NodeState.RUNNING;
        }

        // 이제 EvadeNode가 스스로 쿨타임을 확인합니다.
        if (!cooldownManager.IsCooldownFinished("Evade"))
        {
            return NodeState.FAILURE;
        }

        if (coroutineRunner == null) return NodeState.FAILURE;

        isEvading = true;
        // ▼▼▼ 핵심 수정: 회피를 시작할 때 쿨타임을 함께 시작! ▼▼▼
        cooldownManager.StartCooldown("Evade", evadeCooldown);

        if (direction == "Left" || direction == "Right")
        {
            coroutineRunner.StartCoroutine(SidewaysEvadeCoroutine());
        }
        else 
        {
            coroutineRunner.StartCoroutine(BackwardEvadeCoroutine());
        }

        return NodeState.RUNNING;
    }

    private IEnumerator SidewaysEvadeCoroutine()
    {
        Quaternion originalRotation = agentTransform.rotation;
        float angle = (direction == "Left") ? 90f : -90f;
        agentTransform.Rotate(0, angle, 0);
        actuator.Dodge("Backward");
        yield return new WaitForSeconds(evadeAnimationLength);
        agentTransform.rotation = originalRotation;
        isEvading = false;
    }

    private IEnumerator BackwardEvadeCoroutine()
    {
        actuator.Dodge("Backward");
        yield return new WaitForSeconds(evadeAnimationLength);
        isEvading = false;
    }
}