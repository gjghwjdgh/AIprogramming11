using UnityEngine;

// 파일 이름: EvadeNode.cs
public class EvadeNode : Node
{
    private Transform agentTransform;
    private Animator animator;
    private CooldownManager cooldownManager;
    // Rigidbody 관련 변수 제거
    private string direction;
    private float evadeDistance = 20.0f; // 회피 시 한 번에 이동할 거리

    public EvadeNode(Transform agentTransform, string direction = "Backward")
    {
        this.agentTransform = agentTransform;
        this.animator = agentTransform.GetComponent<Animator>();
        this.cooldownManager = agentTransform.GetComponent<CooldownManager>();
        // Rigidbody 컴포넌트 가져오는 부분 제거
        this.direction = direction.ToLower();
    }

    public override NodeState Evaluate()
    {
        animator.SetTrigger("Evade");

        // Rigidbody null 체크 제거

        Vector3 moveDirection = GetDirectionVector();

        // --- transform.position을 직접 변경하여 이동 ---
        // 캐릭터 컨트롤러나 리지드바디를 사용하지 않으므로, 물리적 상호작용은 무시됩니다.
        agentTransform.position += moveDirection * evadeDistance;

        cooldownManager.StartCooldown("Evade", 10f);
        Debug.Log("Evade (" + direction + ") 사용 (Transform)! 쿨타임 시작.");

        return NodeState.SUCCESS;
    }

    private Vector3 GetDirectionVector()
    {
        // 이 부분은 이전과 동일하게 유지하여 방향을 결정합니다.
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
                Debug.LogWarning("EvadeNode: 알 수 없는 방향 값입니다 - " + direction + ". 기본값(뒤로)으로 회피합니다.");
                return -agentTransform.forward;
        }
    }
}