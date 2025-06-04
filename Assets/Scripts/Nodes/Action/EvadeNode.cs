using UnityEngine;

// 파일 이름: EvadeNode.cs
public class EvadeNode : Node
{
    private Transform agentTransform;
    private Animator animator;
    private CooldownManager cooldownManager;
    private string direction;
    private float evadeDistance = 2f; // 회피 거리

    // 생성자에 방향(direction)을 받는 부분이 추가되었습니다.
    public EvadeNode(Transform agentTransform, string direction = "Backward")
    {
        this.agentTransform = agentTransform;
        this.animator = agentTransform.GetComponent<Animator>();
        this.cooldownManager = agentTransform.GetComponent<CooldownManager>();
        this.direction = direction;
    }

    public override NodeState Evaluate()
    {
        // EvadeNode는 보통 IsCooldownCompleteNode 뒤에 오므로,
        // 여기서는 쿨타임 체크를 생략하고 행동과 쿨타임 시작에 집중합니다.
        
        // "Evade" 애니메이션 트리거를 발동시킵니다.
        // (세부 구현: Animator에서 Blend Tree 등을 사용해 실제 방향에 맞는 애니메이션을 재생할 수 있습니다)
        animator.SetTrigger("Evade");

        // 물리적으로 위치 이동 (간단한 예시)
        Vector3 evadeVector = GetDirectionVector();
        agentTransform.position += evadeVector * evadeDistance;

        // 10초 쿨타임을 시작합니다.
        cooldownManager.StartCooldown("Evade", 10f);

        return NodeState.SUCCESS;
    }

    private Vector3 GetDirectionVector()
    {
        switch (direction.ToLower())
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
                return -agentTransform.forward; // 기본값은 뒤로 회피
        }
    }
}