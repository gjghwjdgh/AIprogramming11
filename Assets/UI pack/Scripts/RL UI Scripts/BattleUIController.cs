using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUIController : MonoBehaviour
{
    public static BattleUIController Instance { get; private set; }

    [Header("Health")]
    public Image leftHealthFill;
    public Image rightHealthFill;

    [Header("Cooldown")]
    public Image leftAttack1Fill;
    public Image leftAttack2Fill;
    public Image leftAttack3Fill;
    public Image leftDefendFill;
    public Image leftDodgeFill;

    public Image rightAttack1Fill;
    public Image rightAttack2Fill;
    public Image rightAttack3Fill;
    public Image rightDefendFill;
    public Image rightDodgeFill;

    [Header("Cooldown Texts")]
    public TMP_Text leftAttack1Text;
    public TMP_Text leftAttack2Text;
    public TMP_Text leftAttack3Text;
    public TMP_Text leftDefendText;
    public TMP_Text leftDodgeText;

    public TMP_Text rightAttack1Text;
    public TMP_Text rightAttack2Text;
    public TMP_Text rightAttack3Text;
    public TMP_Text rightDefendText;
    public TMP_Text rightDodgeText;

    [Header("Win Message")]
    public TMP_Text winMessageText;

    private readonly string ATTACK1 = "Attack1";
    private readonly string ATTACK2 = "Attack2";
    private readonly string ATTACK3 = "Attack3";
    private readonly string DEFEND = "Defend";
    private readonly string DODGE = "Dodge";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    public void UpdateHealth(bool isLeft, float current, float max)
    {
        float fillAmount = current / max;
        if (isLeft)
            leftHealthFill.fillAmount = fillAmount;
        else
            rightHealthFill.fillAmount = fillAmount;
    }

    public void TriggerCooldown(bool isLeft, string type, float duration)
    {
        Image fill = null;
        TMP_Text text = null;
        string label = "";

        if (type == ATTACK1)
        {
            fill = isLeft ? leftAttack1Fill : rightAttack1Fill;
            text = isLeft ? leftAttack1Text : rightAttack1Text;
            label = ATTACK1;
        }
        else if (type == ATTACK2)
        {
            fill = isLeft ? leftAttack2Fill : rightAttack2Fill;
            text = isLeft ? leftAttack2Text : rightAttack2Text;
            label = ATTACK2;
        }
        else if (type == ATTACK3)
        {
            fill = isLeft ? leftAttack3Fill : rightAttack3Fill;
            text = isLeft ? leftAttack3Text : rightAttack3Text;
            label = ATTACK3;
        }
        else if (type == DEFEND)
        {
            fill = isLeft ? leftDefendFill : rightDefendFill;
            text = isLeft ? leftDefendText : rightDefendText;
            label = DEFEND;
        }
        else if (type == DODGE)
        {
            fill = isLeft ? leftDodgeFill : rightDodgeFill;
            text = isLeft ? leftDodgeText : rightDodgeText;
            label = DODGE;
        }

        if (fill != null && text != null)
        {
            StartCoroutine(AnimateCooldown(fill, text, duration, label));
        }
    }

    private IEnumerator AnimateCooldown(Image fillImage, TMP_Text labelText, float duration, string defaultText)
    {
        float time = duration;
        while (time > 0f)
        {
            time -= Time.deltaTime;
            fillImage.fillAmount = time / duration;
            labelText.text = Mathf.Ceil(time * 10f) / 10f + "s";
            yield return null;
        }

        fillImage.fillAmount = 0f;
        labelText.text = defaultText;
    }

    public void ShowWinMessage(string winner)
    {
        if (winMessageText != null)
        {
            winMessageText.text = $"{winner} ½Â¸®!";
            winMessageText.gameObject.SetActive(true);
        }
    }

    public void HideWinMessage()
    {
        if (winMessageText != null)
        {
            winMessageText.gameObject.SetActive(false);
        }
    }
}
