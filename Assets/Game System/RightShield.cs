using UnityEngine;

public class SimpleShield : MonoBehaviour
{
    public Collider shieldCollider; // Inspector에서 Shield의 Collider를 할당

    void Start()
    {
        if (shieldCollider != null)
            shieldCollider.enabled = false; // 시작 시 비활성화
    }

    void Update()
    {
        // Right Shift 누르면 방패 활성화
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            if (shieldCollider != null)
                shieldCollider.enabled = true;
        }
        // Right Shift 떼면 방패 비활성화
        if (Input.GetKeyUp(KeyCode.RightShift))
        {
            if (shieldCollider != null)
                shieldCollider.enabled = false;
        }
    }
}
