using UnityEngine;

public class CharacterStatus : MonoBehaviour
{
    // 캐릭터의 최대 체력과 현재 체력
    public float maxHealth = 100f;
    public float currentHealth;

    void Awake()
    {
        // 게임이 시작되면 현재 체력을 최대 체력으로 설정
        currentHealth = maxHealth;
    }

    // 데미지를 받는 함수 (다른 스크립트에서 호출할 수 있음)
    public void TakeDamage(float damage)
    {
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