using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 필요

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI winMessageText; // 인스펙터에서 연결할 TextMeshPro UI 오브젝트

    void Start()
    {
        // 게임 시작 시 승리 메시지 텍스트가 할당되어 있다면 비활성화
        if (winMessageText != null)
        {
            winMessageText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("UIManager: WinMessageText가 할당되지 않았습니다!");
        }
    }

    // 승리 메시지를 표시하는 함수
    public void DisplayWinMessage(string winnerName)
    {
        if (winMessageText != null)
        {
            winMessageText.text = winnerName + " Win!"; // 예: "Player 1 Win!"
            winMessageText.gameObject.SetActive(true);  // 텍스트 오브젝트 활성화

            // (선택 사항) 게임 시간을 멈추고 싶다면 아래 주석 해제
            // Time.timeScale = 0f;
        }
    }

    // (선택 사항) 게임 재시작이나 다음 레벨로 갈 때 호출하여 메시지를 다시 숨기는 함수
    public void HideWinMessage()
    {
        if (winMessageText != null)
        {
            winMessageText.gameObject.SetActive(false);
            // (선택 사항) 게임 시간을 원래대로 돌리고 싶다면
            // Time.timeScale = 1f;
        }
    }
}