using UnityEngine;

public class Shield : MonoBehaviour
{
    public Collider shieldCollider; // Inspector에서 할당

    [HideInInspector]
    public bool isShieldActive = false;

    void Start()
    {
        if (shieldCollider != null)
            shieldCollider.enabled = false; // 시작 시 비활성화
    }

    void Update()
    {
        // Left Shift 누르면 방패 활성화
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isShieldActive = true;
            if (shieldCollider != null)
                shieldCollider.enabled = true;
        }
        // Left Shift 떼면 방패 비활성화
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isShieldActive = false;
            if (shieldCollider != null)
                shieldCollider.enabled = false;
        }
    }
}
