// 파일 이름: Selector.cs (최종 수정 버전)
using System.Collections.Generic;

public class Selector : Node
{
    protected List<Node> children = new List<Node>();
    private int currentChildIndex = 0; // 현재 실행 중인 자식의 인덱스를 기억

    public Selector(List<Node> children)
    {
        this.children = children;
    }

    public override NodeState Evaluate()
    {
        if (currentChildIndex < children.Count)
        {
            switch (children[currentChildIndex].Evaluate())
            {
                case NodeState.FAILURE:
                    currentChildIndex++; // 실패하면 다음 자식으로
                    return NodeState.RUNNING; // 아직 전체 Selector는 끝나지 않았으므로 RUNNING
                case NodeState.SUCCESS:
                    currentChildIndex = 0; // 성공했으면 처음부터 다시 평가하도록 리셋
                    return NodeState.SUCCESS;
                case NodeState.RUNNING:
                    return NodeState.RUNNING; // 자식이 아직 실행 중이면 나도 실행 중
            }
        }
        
        // 모든 자식을 다 확인했는데 성공을 못 찾았다면 실패
        currentChildIndex = 0; // 다음 평가를 위해 리셋
        return NodeState.FAILURE;
    }
}