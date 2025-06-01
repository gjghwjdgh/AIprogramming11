using UnityEngine;

public class DynamicFightCamera : MonoBehaviour
{
    public Transform player1; // 주 플레이어
    public Transform player2; // 상대
    
    [Header("카메라 기본 오프셋 및 방향")]
    public Vector3 offset = new Vector3(0f, 5f, -10f); // 카메라의 기본 방향과 비율을 결정하는 오프셋

    [Header("거리 기반 카메라 후퇴 설정")]
    // 이 계수는 플레이어 간 거리에 따라 카메라가 얼마나 더 뒤로 물러날지를 결정합니다.
    public float distanceMultiplier = 1.5f; 
    // 계산에 사용될 플레이어 간의 최소/최대 거리를 제한합니다.
    // 이 값들은 실제 플레이어 사이의 거리를 클램핑하는 데 사용됩니다.
    public float minPlayerDistance = 5f; 
    public float maxPlayerDistance = 20f;

    [Header("카메라 이동 부드러움")]
    // 카메라가 목표 위치로 이동하는 데 걸리는 대략적인 시간입니다. (값이 작을수록 빠르게 반응)
    public float smoothTime = 0.2f; 

    private Vector3 currentVelocity; // SmoothDamp 함수 내부에서 사용하는 참조 변수

    void LateUpdate()
    {
        if (player1 == null || player2 == null)
        {
            // 플레이어가 할당되지 않았으면 아무것도 하지 않음
            return;
        }

        // 1. 두 플레이어의 중간 지점 계산
        Vector3 centerPoint = (player1.position + player2.position) / 2f;

        // 2. 두 캐릭터 사이의 실제 거리 계산
        float actualPlayerDistance = Vector3.Distance(player1.position, player2.position);
        
        // 3. 카메라 거리 계산에 사용할 플레이어 간 거리 제한 (너무 가깝거나 멀리 있을 때의 반응 제한)
        float clampedPlayerDistance = Mathf.Clamp(actualPlayerDistance, minPlayerDistance, maxPlayerDistance);

        // 4. 카메라의 목표 위치 계산
        //    offset.normalized: 카메라가 centerPoint로부터 어느 방향에 있을지 결정 (단위 벡터)
        //    clampedPlayerDistance * distanceMultiplier: 해당 방향으로 얼마나 멀리 떨어질지 결정
        //    이 값은 centerPoint를 기준으로 한 상대적인 위치가 됩니다.
        Vector3 desiredRelativePosition = offset.normalized * (clampedPlayerDistance * distanceMultiplier);
        Vector3 desiredPosition = centerPoint + desiredRelativePosition;
        
        // 5. 카메라를 목표 위치로 부드럽게 이동
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);

        // 6. 카메라가 항상 두 플레이어의 중간 지점을 바라보도록 함
        transform.LookAt(centerPoint);
    }
}