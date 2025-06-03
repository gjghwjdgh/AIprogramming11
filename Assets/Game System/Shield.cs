using UnityEngine;

public class Shield : MonoBehaviour
{
    [Header("공통 설정")]
    public Collider shieldCollider;

    [HideInInspector]
    public bool isShieldActive = false;

    [Header("AI 전용 설정")]
    public CharacterStatus myStatus;
    public Animator myAnimator;
    public string defendAnimationStateName = "Defend";
    public string enemyWeaponTag = "EnemyWeapon";

    [Header("플레이어 전용 설정")]
    public bool isPlayerControlled = false;

    void Start()
    {
        if (shieldCollider != null)
            shieldCollider.enabled = false;
    }

    void Update()
    {
        if (isPlayerControlled)
        {
            HandlePlayerInput();
        }
    }

    void HandlePlayerInput()
    {
        // Left Shift 키를 누르면 방패 활성화
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isShieldActive = true; // isShieldActive 값을 true로 설정
            if (shieldCollider != null) shieldCollider.enabled = true;
            if (myAnimator != null) myAnimator.SetBool("IsDefending", true);
        }
        // Left Shift 키를 떼면 방패 비활성화
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isShieldActive = false; // isShieldActive 값을 false로 설정
            if (shieldCollider != null) shieldCollider.enabled = false;
            if (myAnimator != null) myAnimator.SetBool("IsDefending", false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (myStatus == null || myAnimator == null) return;

        if (other.CompareTag(enemyWeaponTag))
        {
            if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName(defendAnimationStateName))
            {
                myStatus.didJustDefend = true;
                Debug.Log(gameObject.name + " 방어 성공!");
            }
        }
    }
}