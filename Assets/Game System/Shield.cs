using UnityEngine;

public class Shield : MonoBehaviour
{
    //[Header("공통 설정")]
    //public Collider shieldCollider;

    //[HideInInspector]
    //public bool isShieldActive = false;

    //[Header("AI 전용 설정")]
    //public CharacterStatus myStatus;
    //public Animator myAnimator;
    //public string defendAnimationStateName = "Defend";
    //public string enemyWeaponTag = "EnemyWeapon";

    //[Header("플레이어 전용 설정")]
    //public bool isPlayerControlled = false;

    //void Start()
    //{
    //    if (shieldCollider != null)
    //        shieldCollider.enabled = false;
    //}

    //void Update()
    //{
    //    if (isPlayerControlled)
    //    {
    //        HandlePlayerInput();
    //    }
    //}

    //void HandlePlayerInput()
    //{
    //    // Left Shift 키를 누르면 방패 활성화
    //    if (Input.GetKeyDown(KeyCode.LeftShift))
    //    {
    //        isShieldActive = true; // isShieldActive 값을 true로 설정
    //        if (shieldCollider != null) shieldCollider.enabled = true;
    //        if (myAnimator != null) myAnimator.SetBool("IsDefending", true);
    //    }
    //    // Left Shift 키를 떼면 방패 비활성화
    //    if (Input.GetKeyUp(KeyCode.LeftShift))
    //    {
    //        isShieldActive = false; // isShieldActive 값을 false로 설정
    //        if (shieldCollider != null) shieldCollider.enabled = false;
    //        if (myAnimator != null) myAnimator.SetBool("IsDefending", false);
    //    }
    //}

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (myStatus == null || myAnimator == null) return;

    //    if (other.CompareTag(enemyWeaponTag))
    //    {
    //        if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName(defendAnimationStateName))
    //        {
    //            myStatus.didJustDefend = true;
    //            Debug.Log(gameObject.name + " 방어 성공!");
    //        }
    //    }


    //    Sword sword = other.GetComponent<Sword>();
    //    if (sword != null)
    //    {
    //        if (isShieldActive)
    //        {
    //            // 공격 칼의 가속도 기반으로 보상 처리
    //            float swordAccelMagnitude = sword.acceleration.magnitude;

    //            // 예: 가속도가 5 이상이면 "강력한 공격 방어 성공"
    //            if (swordAccelMagnitude > 5f)
    //            {
    //                Debug.Log("강력한 공격 방어 성공! 보상 UP");

    //                // 에이전트의 방어 보상 함수 호출
    //                MLtest agent = GetComponentInParent<MLtest>();
    //                if (agent != null)
    //                {
    //                    agent.OnDefendSuccess(swordAccelMagnitude);
    //                }
    //            }
    //            else
    //            {
    //                Debug.Log("약한 공격 방어 성공 (보상 X)");
    //            }
    //        }
    //    }
    //}

    public Collider shieldCollider;

    public bool isShieldActive = false;
    public MLtest mlAgent; // MLtest 스크립트 연결



    void OnTriggerEnter(Collider other)
    {
        if (isShieldActive && other.CompareTag("Sword"))
        {
            Rigidbody swordRb = other.GetComponent<Rigidbody>();
            float attackAccel = swordRb != null ? swordRb.linearVelocity.magnitude : 0f;

            // ML-Agent에게 방어 성공 보상!
            mlAgent.OnDefendSuccess(attackAccel);
            Debug.Log("방어 성공! 가속도: " + attackAccel);
        }
    }



    // 방어 상태를 켜거나 끄는 함수
    public void ActivateShield()
    {
        isShieldActive = true;
    }

    public void DeactivateShield()
    {
        isShieldActive = false;
    }
}
