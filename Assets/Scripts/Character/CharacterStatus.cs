// CharacterStatus.cs (수정된 최종 버전)
using UnityEngine;
using UnityEngine.UI; // ★★★ UI 네임스페이스를 사용하기 위해 반드시 추가! ★★★

public class CharacterStatus : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    
    public Slider healthSlider; // ★★★ 체력 바 UI 슬라이더를 연결할 변수 추가 ★★★

    private PaladinActuator actuator;

    void Awake()
    {
        currentHealth = maxHealth;
        actuator = GetComponent<PaladinActuator>();
    }

    // ★★★ Start 함수를 추가하여 게임 시작 시 UI를 한번 업데이트합니다. ★★★
    void Start()
    {
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        if (actuator != null && actuator.IsCurrentlyDefending)
        {
            Debug.Log(gameObject.name + " defended the attack!");
            return; 
        }

        currentHealth -= damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        Debug.Log(gameObject.name + " took " + damage + " damage. Current health: " + currentHealth);
        
        UpdateHealthUI(); // ★★★ 데미지를 받을 때마다 UI 업데이트 함수 호출 ★★★
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        UpdateHealthUI(); // ★★★ 회복할 때마다 UI 업데이트 함수 호출 ★★★
    }

    // ★★★ 체력 UI를 업데이트하는 함수 ★★★
    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            // 슬라이더의 값(0~1)을 현재 체력 비율에 맞게 계산하여 업데이트합니다.
            healthSlider.value = currentHealth / maxHealth;
        }
    }
}