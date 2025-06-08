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
                    currentChildIndex = 0; // 하나라도 실패하면 처음부터 다시 평가하도록 리셋
                    return NodeState.FAILURE;
                case NodeState.SUCCESS:
                    currentChildIndex++; // 성공하면 다음 자식으로 인덱스 이동
                    if (currentChildIndex >= children.Count)
                    {
                        currentChildIndex = 0; // 시퀀스 전체 성공, 다음 평가를 위해 리셋
                        return NodeState.SUCCESS;
                    }
                    return NodeState.RUNNING; // 아직 시퀀스가 진행 중이므로 RUNNING
                case NodeState.RUNNING:
                    return NodeState.RUNNING;
            }
        }
        
        currentChildIndex = 0;
        return NodeState.SUCCESS; // 이 부분은 거의 도달하지 못함
    }
}