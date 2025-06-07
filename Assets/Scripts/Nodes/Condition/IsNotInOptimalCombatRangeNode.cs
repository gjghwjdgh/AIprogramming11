// 파일 이름: IsNotInOptimalCombatRangeNode.cs (디버깅 버전)
using UnityEngine;

public class IsNotInOptimalCombatRangeNode : Node
{
    private Transform agentTransform;
    private Transform targetTransform;
    private float minRange;
    private float maxRange;

    public IsNotInOptimalCombatRangeNode(Transform agentTransform, Transform target, float minRange, float maxRange)
    {
        this.agentTransform = agentTransform;
        this.targetTransform = target;
        this.minRange = minRange;
        this.maxRange = maxRange;
    }

    public override NodeState Evaluate()
    {
        if (targetTransform == null) return NodeState.FAILURE;

        float distance = Vector3.Distance(agentTransform.position, targetTransform.position);

        // --- 디버그 로그 추가 ---
        string logMessage = $"[접근 판단] 현재 거리: {distance:F2}m. 최적 범위: {minRange:F1}m ~ {maxRange:F1}m. ";

        // 거리가 최적 범위를 벗어났는지 확인
        if (distance < minRange || distance > maxRange)
        {
            logMessage += "<color=green>판단: 접근해야 함 (SUCCESS)</color>";
            Debug.Log(logMessage);
            return NodeState.SUCCESS;
        }
        else
        {
            logMessage += "<color=red>판단: 접근할 필요 없음 (FAILURE)</color>";
            Debug.Log(logMessage);
            return NodeState.FAILURE;
        }
    }
}