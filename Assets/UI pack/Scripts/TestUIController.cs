using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TestUIController : MonoBehaviour
{
    public UICoolDown leftAttack;
    public UICoolDown leftDefend;
    public UICoolDown leftDodge;
    public UICoolDown rightAttack;
    public UICoolDown rightDefend;
    public UICoolDown rightDodge;

    public Image leftHealthFill;
    public Image rightHealthFill;

    private float leftHealth = 100f;
    private float rightHealth = 100f;
    private float maxHealth = 100f;

    void Update()
    {
        // 테스트용 쿨타임 발동
        if (Input.GetKeyDown(KeyCode.Alpha1)) leftAttack.TriggerCooldown();
        if (Input.GetKeyDown(KeyCode.Alpha2)) leftDefend.TriggerCooldown();
        if (Input.GetKeyDown(KeyCode.Alpha3)) leftDodge.TriggerCooldown();
        if (Input.GetKeyDown(KeyCode.Alpha8)) rightAttack.TriggerCooldown();
        if (Input.GetKeyDown(KeyCode.Alpha9)) rightDefend.TriggerCooldown();
        if (Input.GetKeyDown(KeyCode.Alpha0)) rightDodge.TriggerCooldown();

        // 체력 조절 테스트
        if (Input.GetKeyDown(KeyCode.L))
        {
            leftHealth = Mathf.Max(0, leftHealth - 10f);
            leftHealthFill.fillAmount = leftHealth / maxHealth;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            rightHealth = Mathf.Max(0, rightHealth - 10f);
            rightHealthFill.fillAmount = rightHealth / maxHealth;
        }
    }
}
