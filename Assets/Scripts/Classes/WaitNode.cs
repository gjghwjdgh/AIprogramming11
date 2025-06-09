// 파일 이름: WaitNode.cs
using UnityEngine;

public class WaitNode : Node
{
    private float waitTime;
    private float startTime;
    private bool isWaiting;

    public WaitNode(float waitTime)
    {
        this.waitTime = waitTime;
        isWaiting = false;
    }

    public override NodeState Evaluate()
    {
        if (!isWaiting)
        {
            // 기다리기 시작
            isWaiting = true;
            startTime = Time.time;
            Debug.Log($"<color=orange>Wait 시작 ({waitTime}초)</color>");
        }

        // 아직 기다리는 중인가?
        if (Time.time - startTime < waitTime)
        {
            // 그렇다면 "나는 아직 바쁘다(RUNNING)"라고 보고
            return NodeState.RUNNING;
        }
        else
        {
            // 기다리기가 끝났다면 "임무 성공(SUCCESS)"을 보고
            isWaiting = false;
            Debug.Log("<color=green>Wait 종료</color>");
            return NodeState.SUCCESS;
        }
    }
}