// GameManager.cs (수정 완료 버전)
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Objects")]
    public CharacterStatus player1Status;
    public CharacterStatus player2Status;

    [Header("UI Elements")]
    public GameObject victoryPanel;
    public TextMeshProUGUI winnerText;

    [Header("Data Loggers")] // ★★★ 데이터 로거 참조 변수 추가 ★★★
    public DataLogger player1Logger;
    public DataLogger player2Logger;

    private bool isGameOver = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnCharacterDied(CharacterStatus loser)
    {
        if (isGameOver) return;
        isGameOver = true;

        CharacterStatus winner = (loser == player1Status) ? player2Status : player1Status;
        DataLogger winnerLogger = (loser == player1Status) ? player2Logger : player1Logger;
        DataLogger loserLogger = (loser == player1Status) ? player1Logger : player2Logger;

        // ★★★ 승패 결과 기록 ★★★
        if(winnerLogger != null) winnerLogger.RecordMatchResult("Win");
        if(loserLogger != null) loserLogger.RecordMatchResult("Loss");

        // ★★★ 각 로거에게 CSV 파일 저장을 명령 ★★★
        if(winnerLogger != null) winnerLogger.WriteDataToCSV();
        if(loserLogger != null) loserLogger.WriteDataToCSV();

        // --- 기존 UI 및 시간 정지 로직 ---
        if (victoryPanel != null && winnerText != null)
        {
            victoryPanel.SetActive(true);
            winnerText.text = winner.gameObject.name + " WINS!";
        }
        Time.timeScale = 0f;
    }
}