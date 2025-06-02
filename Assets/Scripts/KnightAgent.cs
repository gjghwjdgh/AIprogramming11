using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class KnightAgent : Agent
{
    // 나이트의 이동 속도
    public float moveSpeed = 5f;
    // 목표 지점의 Transform
    public Transform targetTransform;

    private Rigidbody rb;

    /// <summary>
    /// 에이전트 초기화 시 호출됩니다.
    /// </summary>
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// 에피소드(학습 단위)가 시작될 때마다 호출됩니다.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        // 나이트의 위치와 속도를 초기화합니다.
        transform.localPosition = new Vector3(Random.Range(-4f, 4f), 0.5f, Random.Range(-4f, 4f));
        rb.linearVelocity = Vector3.zero;

        // 타겟의 위치를 무작위로 재설정합니다.
        targetTransform.localPosition = new Vector3(Random.Range(-4f, 4f), 0.5f, Random.Range(-4f, 4f));
    }

    /// <summary>
    /// 에이전트가 주변 환경을 관찰하고 정보를 수집합니다.
    /// </summary>
    /// <param name="sensor">정보를 추가할 센서</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // 1. 나이트 자신의 위치 정보
        sensor.AddObservation(transform.localPosition);

        // 2. 목표 지점의 위치 정보
        sensor.AddObservation(targetTransform.localPosition);

        // 3. 나이트의 속도 정보
        sensor.AddObservation(rb.linearVelocity.x);
        sensor.AddObservation(rb.linearVelocity.z);
    }

    /// <summary>
    /// 정책(신경망)으로부터 행동 값을 전달받아 실행합니다.
    /// </summary>
    /// <param name="actionBuffers">전달받은 행동 값</param>
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // 행동 값 (x축, z축 이동량)
        float moveX = actionBuffers.ContinuousActions[0];
        float moveZ = actionBuffers.ContinuousActions[1];

        // 행동 값을 이용해 에이전트를 이동시킴
        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;

        // 작은 패널티를 주어 불필요한 움직임을 줄이고 빨리 목표를 찾도록 유도
        AddReward(-0.001f);
    }

    /// <summary>
    /// 개발자가 직접 에이전트를 조작하여 테스트할 때 사용됩니다.
    /// (Behavior Parameters 컴포넌트의 Behavior Type을 Heuristic Only로 설정해야 함)
    /// </summary>
    /// <param name="actionsOut">키보드 입력에 따른 행동 값</param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal"); // 좌우 방향키
        continuousActionsOut[1] = Input.GetAxis("Vertical");   // 상하 방향키
    }

    /// <summary>
    /// 다른 Collider와 충돌했을 때 호출됩니다.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // 목표 지점에 도달했을 경우
        if (other.gameObject.CompareTag("Target"))
        {
            // 큰 보상을 주고 에피소드를 종료 (성공)
            AddReward(1.0f);
            EndEpisode();
        }
        // 벽이나 장애물에 부딪혔을 경우
        else if (other.gameObject.CompareTag("Wall"))
        {
            // 패널티를 주고 에피소드를 종료 (실패)
            AddReward(-1.0f);
            EndEpisode();
        }
    }
}