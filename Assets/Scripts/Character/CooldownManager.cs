using System.Collections.Generic;
using UnityEngine;

public class CooldownManager : MonoBehaviour
{
    // 스킬 이름(string)과 해당 스킬의 쿨타임이 끝나는 게임 시간(float)을 저장
    private Dictionary<string, float> skillCooldownEndTime = new Dictionary<string, float>();

    /// <summary>
    /// 특정 스킬의 쿨타임을 시작시킵니다.
    /// </summary>
    /// <param name="skillName">쿨타임을 적용할 스킬의 고유한 이름</param>
    /// <param name="cooldownDuration">쿨타임 지속 시간 (초 단위)</param>
    public void StartCooldown(string skillName, float cooldownDuration)
    {
        // 현재 게임 시간 + 쿨타임 지속 시간 = 쿨타임이 끝나는 시간
        float endTime = Time.time + cooldownDuration;
        skillCooldownEndTime[skillName] = endTime;
        // Debug.Log(skillName + " 쿨타임 시작. 종료 시간: " + endTime); // 디버깅용
    }

    /// <summary>
    /// 특정 스킬의 쿨타임이 완료되었는지 확인합니다.
    /// </summary>
    /// <param name="skillName">확인할 스킬의 이름</param>
    /// <returns>쿨타임이 완료되었으면 true, 아니면 false</returns>
    public bool IsCooldownFinished(string skillName)
    {
        // 만약 skillCooldownEndTime 딕셔너리에 해당 스킬이 등록되어 있지 않다면,
        // 아직 한 번도 사용하지 않았거나 쿨타임이 없는 스킬로 간주하여 true 반환
        if (!skillCooldownEndTime.ContainsKey(skillName))
        {
            return true;
        }

        // 해당 스킬의 쿨타임 종료 시간 <= 현재 게임 시간 이라면 쿨타임 완료
        if (skillCooldownEndTime[skillName] <= Time.time)
        {
            return true;
        }

        // 그 외의 경우는 아직 쿨타임 진행 중
        return false;
    }



    // CooldownManager.cs 에 추가할 함수 예시
    public float GetRemainingCooldown(string skillName)
    {
        if (skillCooldownEndTime.ContainsKey(skillName))
        {
            float remaining = skillCooldownEndTime[skillName] - Time.time;
            return remaining > 0 ? remaining : 0;
        }
        return 0;
    }

    public float GetTotalCooldown(string skillName)
    {
        // 이 부분은 각 스킬의 전체 쿨타임 값을 저장해두고 반환해야 합니다.
        // 예를 들어, 딕셔너리를 하나 더 만들거나 switch-case 문을 사용할 수 있습니다.
        switch(skillName)
        {
            case "BasicAttack": return 6f;
            case "KickAttack": return 10f;
            case "SpinAttack": return 20f;
            case "Defend": return 6f;
            case "Evade": return 10f;
            default: return 0f;
        }
    }
}