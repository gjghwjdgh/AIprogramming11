// CooldownUIManager.cs (수정된 버전)
using UnityEngine;
using UnityEngine.UI;

public class CooldownUIManager : MonoBehaviour
{
    public CooldownManager cooldownManager;

    [Header("Basic Attack UI")]
    public Image basicAttackOverlay;
    public Text basicAttackText;

    [Header("Kick Attack UI")]
    public Image kickAttackOverlay;
    public Text kickAttackText;

    [Header("Spin Attack UI")]
    public Image spinAttackOverlay;
    public Text spinAttackText;
    
    // ★★★ 방어와 회피 UI를 위한 변수 추가 ★★★
    [Header("Defend UI")]
    public Image defendOverlay;
    public Text defendText;

    [Header("Evade UI")]
    public Image evadeOverlay;
    public Text evadeText;

    void Update()
    {
        if (cooldownManager == null) return;

        // 각 스킬에 대해 UI 업데이트 함수를 호출합니다.
        UpdateSkillUI("BasicAttack", basicAttackOverlay, basicAttackText);
        UpdateSkillUI("KickAttack", kickAttackOverlay, kickAttackText);
        UpdateSkillUI("SpinAttack", spinAttackOverlay, spinAttackText);
        
        // ★★★ 방어와 회피 UI 업데이트 호출 추가 ★★★
        UpdateSkillUI("Defend", defendOverlay, defendText);
        UpdateSkillUI("Evade", evadeOverlay, evadeText);
    }

    void UpdateSkillUI(string skillName, Image overlay, Text text)
    {
        if (overlay == null || text == null) return;

        float remainingCooldown = cooldownManager.GetRemainingCooldown(skillName);
        float totalCooldown = cooldownManager.GetTotalCooldown(skillName);

        if (remainingCooldown > 0)
        {
            overlay.gameObject.SetActive(true);
            text.gameObject.SetActive(true);
            overlay.fillAmount = remainingCooldown / totalCooldown;
            text.text = remainingCooldown.ToString("F1");
        }
        else
        {
            overlay.gameObject.SetActive(false);
            text.gameObject.SetActive(false);
        }
    }
}