using UnityEngine;

public class CharacterStatus : MonoBehaviour
{
    // 캐릭터의 최대 체력과 현재 체력
    public float maxHealth = 100f;
    public float currentHealth;
    
    // public bool didJustDefend = false; // <-- 이 줄은 PaladinActuator를 직접 참조하므로 필요 없어져서 삭제합니다.

    private PaladinActuator actuator; // ★★★ PaladinActuator를 참조할 변수를 추가합니다.

    void Awake()
    {
        // 게임이 시작되면 현재 체력을 최대 체력으로 설정
        currentHealth = maxHealth;

        // ★★★ PaladinActuator 컴포넌트를 찾아서 변수에 할당합니다.
        actuator = GetComponent<PaladinActuator>();
    }

    // 데미지를 받는 함수 (다른 스크립트에서 호출할 수 있음)
    public void TakeDamage(float damage)
    {
        // ★★★ 데미지를 받기 전에, 방어 상태인지 먼저 확인합니다. ★★★
        if (actuator != null && actuator.IsCurrentlyDefending)
        {
            // 방어 중이라면 데미지를 무시하고 함수를 종료합니다.
            Debug.Log(gameObject.name + " defended the attack!");
            return; 
        }

        // --- 방어 중이 아닐 때만 아래의 데미지 처리 코드가 실행됩니다. ---
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        // 데미지 받았을 때의 로그 (디버깅용)
        Debug.Log(gameObject.name + " took " + damage + " damage. Current health: " + currentHealth);
    }

    // 체력을 회복하는 함수 (필요 시 사용)
    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
}