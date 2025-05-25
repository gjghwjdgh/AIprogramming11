using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UICoolDown : MonoBehaviour
{
    public Image cooldownImage;           // ���� �������� �̹���
    public TextMeshProUGUI cooldownText;  // �ð� ǥ�� �ؽ�Ʈ
    public float cooldownDuration = 2.5f; // �⺻ ��Ÿ��
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
