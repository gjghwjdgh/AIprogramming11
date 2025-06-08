// 파일 이름: MaintainDistanceNode.cs
using UnityEngine;

public class MaintainDistanceNode : Node
{
    private Transform agentTransform;
    private Transform targetTransform;
    private PaladinActuator actuator;
    private float idealDistance;
    private float tolerance;

    public MaintainDistanceNode(Transform agent, Transform target, float idealDist, float tol)
    {
        this.agentTransform = agent;
        this.targetTransform = target;
        this.actuator = agent.GetComponent<PaladinActuator>();
        this.idealDistance = idealDist;
        this.tolerance = tol;
    }

    public override NodeState Evaluate()
    {
        if (targetTransform == null || actuator == null) return NodeState.FAILURE;

        float distance = Vector3.Distance(agentTransform.position, targetTransform.position);

        // Debug.Log($"MaintainDistanceNode: Current Distance = {distance:F2}, Ideal = {idealDistance:F2}, Tolerance = {tolerance:F2}");

        // 목표 거리에 도달했거나 오차 범위 내에 있다면 성공
        if (Mathf.Abs(distance - idealDistance) <= tolerance)
        {
            actuator.SetMovement(0); // 움직임 멈춤
            // Debug.Log("MaintainDistanceNode: SUCCESS - Ideal distance achieved.");
            return NodeState.SUCCESS;
        }
        else // 목표 거리에 도달하지 못했다면 이동 계속
        {
            // 위아래를 쳐다보지 않고 방향만 바라보도록 회전 로직 적용
            Vector3 directionToTarget = targetTransform.position - agentTransform.position;
            directionToTarget.y = 0; // Y축 성분을 0으로 만들어서 위아래 바라보지 않음

            if (directionToTarget != Vector3.zero)
            {
                actuator.SetRotation(Quaternion.LookRotation(directionToTarget));
            }

            // 목표 거리에 따라 전진 또는 후진 (속도 0.75f로 통일)
            // Debug.Log($"MaintainDistanceNode: RUNNING - Moving. Distance > Ideal? {distance > idealDistance}");
            actuator.SetMovement(distance > idealDistance ? 0.75f : -0.75f); 
            
            return NodeState.RUNNING; // 목표 거리에 도달할 때까지 계속 실행
        }
    }
}