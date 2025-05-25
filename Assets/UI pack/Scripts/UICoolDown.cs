using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UICoolDown : MonoBehaviour
{
    public Image cooldownImage;           // 원형 게이지용 이미지
    public TextMeshProUGUI cooldownText;  // 시간 표시 텍스트
    public float cooldownDuration = 2.5f; // 기본 쿨타임
    private float cooldownTimer = 0f;
    private bool isCoolingDown = false;

    public void TriggerCooldown()
    {
        cooldownTimer = cooldownDuration;
        isCoolingDown = true;
    }

    void Update()
    {
        if (!isCoolingDown) return;

        cooldownTimer -= Time.deltaTime;

        float fill = Mathf.Clamp01(cooldownTimer / cooldownDuration);
        cooldownImage.fillAmount = fill;
        cooldownText.text = cooldownTimer > 0f ? $"{cooldownTimer:F1}s" : "Ready";

        if (cooldownTimer <= 0f)
        {
            isCoolingDown = false;
        }
    }
}
