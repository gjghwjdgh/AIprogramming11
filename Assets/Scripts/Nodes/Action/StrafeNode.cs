// 파일 이름: StrafeNode.cs (기존 내용을 지우고 아래 코드로 덮어쓰세요)
using UnityEngine;

public class StrafeNode : Node
{
    private PaladinActuator actuator;
    private bool isStrafing; // 현재 스트레이프 동작이 진행 중인지 기억하는 플래그

    public StrafeNode(Transform agentTransform)
    {
        this.actuator = agentTransform.GetComponent<PaladinActuator>();
        this.isStrafing = false; // 처음에는 쉬는 상태
    }

    public override NodeState Evaluate()
    {
        // 만약 이미 스트레이프 중이라면, 계속 RUNNING을 반환해서 다른 행동을 막음
        if (isStrafing)
        {
            return NodeState.RUNNING;
        }

        if (actuator == null) return NodeState.FAILURE;

        // 스트레이프 시작
        isStrafing = true; // "나 이제 바쁘다!" 플래그 켜기
        string direction = Random.value > 0.5f ? "Left" : "Right";
        
        // PaladinActuator에게 스트레이프를 시키면서, 끝나면 OnStrafeComplete 함수를 실행해달라고 부탁
        actuator.Strafe(direction, OnStrafeComplete);
        
        // 스트레이프가 시작되었으니, "진행 중"이라고 보고
        return NodeState.RUNNING;
    }

    // 스트레이프 동작이 모두 끝났을 때 PaladinActuator가 호출해줄 함수
    private void OnStrafeComplete()
    {
        isStrafing = false; // "이제 일 끝났다!" 플래그 끄기
    }
}