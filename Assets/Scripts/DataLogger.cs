// DataLogger.cs
using UnityEngine;
using System.IO; // 파일 입출력을 위해 추가
using System.Text; // StringBuilder를 위해 추가

public class DataLogger : MonoBehaviour
{
    // --- 기록할 데이터 ---
    public string agentName; // 이 로거가 누구의 것인지 식별
    public int successfulAttacks = 0;
    public int successfulDefenses = 0;
    public string matchResult = "N/A"; // "Win", "Loss", "Draw"

    // --- 데이터 수집을 위한 공개 함수 ---
    public void IncrementAttackCount()
    {
        successfulAttacks++;
    }

    public void IncrementDefenseCount()
    {
        successfulDefenses++;
    }

    public void RecordMatchResult(string result)
    {
        matchResult = result;
    }

    // --- CSV 파일 저장을 위한 함수 ---
    public void WriteDataToCSV()
    {
        // 파일 경로는 Unity 프로젝트의 Assets 폴더 바로 안쪽입니다.
        string filePath = Path.Combine(Application.dataPath, "simulation_results.csv");
        StringBuilder sb = new StringBuilder();

        // 파일이 존재하지 않으면 헤더(첫 줄)를 추가합니다.
        if (!File.Exists(filePath))
        {
            sb.AppendLine("AgentName,SuccessfulAttacks,SuccessfulDefenses,MatchResult");
        }

        // 현재 데이터를 한 줄로 만듭니다.
        string dataLine = $"{agentName},{successfulAttacks},{successfulDefenses},{matchResult}";
        sb.AppendLine(dataLine);

        // 파일에 데이터를 추가합니다. (기존 내용을 덮어쓰지 않고 이어쓰기)
        File.AppendAllText(filePath, sb.ToString());

        Debug.Log(agentName + "의 데이터가 " + filePath + " 에 저장되었습니다.");
    }
}