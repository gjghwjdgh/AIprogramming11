using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UICoolDown : MonoBehaviour
{
    public Image cooldownImage;               // 원형 게이지 이미지
    public TextMeshProUGUI cooldownText;      // 시간 표시 텍스트
    public float cooldownDuration = 2.5f;     // 쿨타임 지속 시간

    [SerializeField]
    private string skillName = "Skill";       // Attack / Defend / Dodge 같은 이름

    private float cooldownTimer = 0f;
    private bool isCoolingDown = false;

    public bool IsAvailable => !isCoolingDown; // 외부에서 사용할 수 있는 상태 프로퍼티

    public void TriggerCooldown()
    {
        if (isCoolingDown) return;            // 쿨타임 중이면 실행 금지

        cooldownTimer = cooldownDuration;
        isCoolingDown = true;
    }

    void Update()
    {
        if (isCoolingDown)
        {
            cooldownTimer -= Time.deltaTime;
            float fill = Mathf.Clamp01(cooldownTimer / cooldownDuration);
            cooldownImage.fillAmount = fill;
            cooldownText.text = $"{cooldownTimer:F1}s";

            if (cooldownTimer <= 0f)
            {
                isCoolingDown = false;
                cooldownText.text = skillName;
                cooldownImage.fillAmount = 0f;
            }
        }
    }
}
