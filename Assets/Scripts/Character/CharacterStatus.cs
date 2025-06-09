// CharacterStatus.cs (수정 완료 버전)
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterStatus : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    
    public Slider healthSlider;
    public TextMeshProUGUI healthText;

    private PaladinActuator actuator;
    private DataLogger dataLogger; // ★★★ 데이터 로거 참조 추가 ★★★

    void Awake()
    {
        currentHealth = maxHealth;
        actuator = GetComponent<PaladinActuator>();
        dataLogger = GetComponent<DataLogger>(); // ★★★ 데이터 로거 찾아오기 ★★★
    }

    void Start()
    {
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        if (actuator != null && actuator.IsCurrentlyDefending)
        {
            Debug.Log(gameObject.name + " defended the attack!");
            
            // ★★★ 방어 성공 시, 로거에 기록 ★★★
            if(dataLogger != null)
            {
                dataLogger.IncrementDefenseCount();
            }
            return; 
        }

        currentHealth -= damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        
        UpdateHealthUI();
        Debug.Log(gameObject.name + " took " + damage + " damage. Current health: " + currentHealth);

        if (currentHealth <= 0)
        {
            GameManager.Instance.OnCharacterDied(this);
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth / maxHealth;
        }
        if (healthText != null)
        {
            healthText.text = $"{Mathf.RoundToInt(currentHealth)} / {maxHealth}";
        }
    }
}