using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UICoolDown : MonoBehaviour
{
    public Image cooldownImage;               // ���� ������ �̹���
    public TextMeshProUGUI cooldownText;      // �ð� ǥ�� �ؽ�Ʈ
    public float cooldownDuration = 2.5f;     // ��Ÿ�� ���� �ð�

    [SerializeField]
    private string skillName = "Skill";       // Attack / Defend / Dodge ���� �̸�

    private float cooldownTimer = 0f;
    private bool isCoolingDown = false;

    public bool IsAvailable => !isCoolingDown; // �ܺο��� ����� �� �ִ� ���� ������Ƽ

    public void TriggerCooldown()
    {
        if (isCoolingDown) return;            // ��Ÿ�� ���̸� ���� ����

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
