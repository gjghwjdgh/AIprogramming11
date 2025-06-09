// 파일 이름: IPaladinParameters.cs
public interface IPaladinParameters
{
    float optimalCombatDistanceMin { get; }
    float optimalCombatDistanceMax { get; }
    string idleStateName { get; }
    // 필요한 다른 공통 파라미터들 추가
}