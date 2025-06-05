using UnityEngine;

// 파일 이름: FeintStepNode.cs
public class FeintStepNode : Node
{
    private Transform agentTransform;
    private Animator animator;
    private CooldownManager cooldownManager;
    private string direction; // "ForwardShort", "LeftStep", "RightStep" 등
    private float stepDistance;

    private string skillName = "FeintStep"; // 간 보기 행동의 쿨타임용 이름
    private float cooldownDuration = 3f;   // 간 보기 행동의 쿨타임

    public FeintStepNode(Transform agentTransform, string direction, float stepDistance)
    {
        this.agentTransform = agentTransform;
        this.animator = agentTransform.GetComponent<Animator>();
        this.cooldownManager = agentTransform.GetComponent<CooldownManager>();
        this.direction = direction.ToLower();
        this.stepDistance = stepDistance;
    }

    public override NodeState Evaluate()
    {
        // 이 노드는 IsCooldownCompleteNode 뒤에 온다고 가정하고 쿨타임 시작만 처리
        // 만약 이 노드 자체적으로 쿨타임 체크를 하려면 IsCooldownFinished를 호출

        // "Feint" 또는 방향에 맞는 애니메이션 트리거 발동
        // 예시: "FeintForward", "FeintLeft" 등 또는 하나의 "Feint" 트리거 후 Blend Tree 사용
        animator.SetTrigger("FeintStepTrigger"); // "FeintStepTrigger" 라는 이름의 Trigger 파라미터 필요
        Debug.Log("FeintStepNode: " + direction + " 방향으로 간 보기 시도");

        Vector3 moveDirection = Vector3.zero;
        switch (direction)
        {
            case "forwardshort":
                moveDirection = agentTransform.forward;
                break;
            case "backwardshort": // 필요하다면 추가
                moveDirection = -agentTransform.forward;
                break;
            case "leftstep":
                moveDirection = -agentTransform.right;
                break;
            case "rightstep":
                moveDirection = agentTransform.right;
                break;
            default:
                moveDirection = agentTransform.forward; // 기본값
                break;
        }

        // transform.position 직접 변경 (Collider 없이 이동)
        agentTransform.position += moveDirection * stepDistance;

        cooldownManager.StartCooldown(skillName, cooldownDuration);
        
        return NodeState.SUCCESS;
    }
}