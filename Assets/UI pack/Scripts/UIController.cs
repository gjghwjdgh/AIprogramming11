// using Unity.VisualScripting; // 비주얼 스크립팅을 사용하지 않는다면 이 줄은 삭제해도 됩니다.
using UnityEngine;
using UnityEngine.UI;
// using TMPro; // UIManager에서 TextMeshPro를 사용하므로, TestUIController에서는 직접 필요 없을 수 있습니다.

public class TestUIController : MonoBehaviour
{
    public static TestUIController Instance { get; private set; } //KIM

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

    // --- 승리 메시지 관련 추가 ---
    public UIManager uiManager; // 인스펙터에서 UIManager 오브젝트를 연결해주세요.
    public string leftPlayerName = "Left Player";  // 승리 메시지에 표시될 왼쪽 플레이어 이름
    public string rightPlayerName = "Right Player"; // 승리 메시지에 표시될 오른쪽 플레이어 이름
    private bool isGameOver = false; // 게임 종료 상태를 관리하는 플래그
    // ---------------------------

    void Start() // Start 함수 추가
    {
        // 게임 시작 시 체력을 최대로 설정하고 UI에 반영
        leftHealth = maxHealth;
        rightHealth = maxHealth;
        UpdateHealthUI(); // 체력 UI 업데이트 함수 호출

        // UIManager 연결 확인
        if (uiManager == null)
        {
            Debug.LogError("TestUIController: UIManager가 연결되지 않았습니다! 인스펙터에서 할당해주세요.");
            // 필요하다면 자동으로 찾아보는 로직 추가 가능
            // uiManager = FindObjectOfType<UIManager>();
        }
    }

    private void Awake() //KIM
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        // 게임이 종료되었다면 더 이상 아래 로직들을 실행하지 않음
        if (isGameOver)
        {
            return;
        }

        // 테스트용 쿨타임 발동
        if (Input.GetKeyDown(KeyCode.Alpha1)) leftAttack.TriggerCooldown();
        if (Input.GetKeyDown(KeyCode.Alpha2)) leftDefend.TriggerCooldown();
        if (Input.GetKeyDown(KeyCode.Alpha3)) leftDodge.TriggerCooldown();
        if (Input.GetKeyDown(KeyCode.Alpha8)) rightAttack.TriggerCooldown();
        if (Input.GetKeyDown(KeyCode.Alpha9)) rightDefend.TriggerCooldown();
        if (Input.GetKeyDown(KeyCode.Alpha0)) rightDodge.TriggerCooldown();

        // 체력 감소 테스트
        if (Input.GetKeyDown(KeyCode.N)) // 왼쪽 플레이어(Left) 체력 감소
        {
            HandleDamage(true, 25f); // 왼쪽 플레이어에게 25 데미지
        }

        if (Input.GetKeyDown(KeyCode.M)) // 오른쪽 플레이어(Right) 체력 감소
        {
            HandleDamage(false, 25f); // 오른쪽 플레이어에게 25 데미지
        }
    }

    // 데미지 처리 및 승리 조건 확인을 위한 함수
    void HandleDamage(bool isLeftPlayerTarget, float damageAmount)
    {
        if (isGameOver) return; // 이미 게임이 끝났다면 데미지 처리 안 함

        if (isLeftPlayerTarget) // 왼쪽 플레이어가 데미지를 받는 경우
        {
            leftHealth -= damageAmount;
            leftHealth = Mathf.Max(0, leftHealth); // 체력이 0 밑으로 내려가지 않도록
            
            if (leftHealth <= 0)
            {
                // 왼쪽 플레이어 체력이 0 이하 -> 오른쪽 플레이어 승리
                if (uiManager != null)
                {
                    uiManager.DisplayWinMessage(rightPlayerName); // 오른쪽 플레이어 이름으로 승리 메시지 표시
                }
                isGameOver = true; // 게임 종료 상태로 변경
                Debug.Log("게임 종료! 승자: " + rightPlayerName);
            }
        }
        else // 오른쪽 플레이어가 데미지를 받는 경우
        {
            rightHealth -= damageAmount;
            rightHealth = Mathf.Max(0, rightHealth); // 체력이 0 밑으로 내려가지 않도록

            if (rightHealth <= 0)
            {
                // 오른쪽 플레이어 체력이 0 이하 -> 왼쪽 플레이어 승리
                if (uiManager != null)
                {
                    uiManager.DisplayWinMessage(leftPlayerName); // 왼쪽 플레이어 이름으로 승리 메시지 표시
                }
                isGameOver = true; // 게임 종료 상태로 변경
                Debug.Log("게임 종료! 승자: " + leftPlayerName);
            }
        }
        UpdateHealthUI(); // 체력 변경 후 UI 업데이트
    }

    // 체력 바 UI를 업데이트하는 함수
    void UpdateHealthUI()
    {
        if (leftHealthFill != null)
        {
            leftHealthFill.fillAmount = leftHealth / maxHealth;
        }
        if (rightHealthFill != null)
        {
            rightHealthFill.fillAmount = rightHealth / maxHealth;
        }
    }

    // 체력 UI를 외부에서 갱신하는 함수 추가 KIM
    public void SetLeftHealth(float current, float max)
    {
        leftHealth = current;
        maxHealth = max;
        leftHealthFill.fillAmount = leftHealth / maxHealth;
    }
    public void SetRightHealth(float current, float max)
    {
        rightHealth = current;
        maxHealth = max;
        rightHealthFill.fillAmount = rightHealth / maxHealth;
    }
}
