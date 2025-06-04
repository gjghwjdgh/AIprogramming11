using System.Collections.Generic;
using UnityEngine;

public class CooldownManager : MonoBehaviour
{
    // 스킬 이름(string)과 쿨타임이 끝나는 시간(float)을 저장하는 딕셔너리
    private Dictionary<string, float> cooldowns = new Dictionary<string, float>();

    // 쿨타임 시작 함수
    public void StartCooldown(string skillName, float cooldownDuration)
    {
        float cooldownEndTime = Time.time + cooldownDuration;
        cooldowns[skillName] = cooldownEndTime;
        // 디버깅용 로그
        // Debug.Log(skillName + " cooldown started. Ends at: " + cooldownEndTime);
    }

    // 쿨타임이 끝났는지 확인하는 함수
    public bool IsCooldownFinished(string skillName)
    {
        // 쿨타임 목록에 스킬이 등록되지 않았거나,
        // 등록은 되었지만 현재 시간이 쿨타임 종료 시간보다 크다면
        if (!cooldowns.ContainsKey(skillName) || Time.time >= cooldowns[skillName])
        {
            return true; // 쿨타임 완료 (스킬 사용 가능)
        }

        // 위 조건에 해당하지 않으면 아직 쿨타임 중
        return false;
    }
}