// 파일 이름: BT_Brain.cs
using UnityEngine;

public abstract class BT_Brain : MonoBehaviour, IPaladinParameters
{
    // 디버깅용 현재 액션 이름 (모든 자식 AI들이 이 변수를 상속받음)
    [HideInInspector]
    public string currentActionName = "None";

    // 인터페이스 멤버들도 이 부모 클래스에서 관리
    public abstract float optimalCombatDistanceMin { get; }
    public abstract float optimalCombatDistanceMax { get; }
    public abstract string idleStateName { get; }
}