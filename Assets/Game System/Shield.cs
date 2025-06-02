using UnityEngine;

public class Shield : MonoBehaviour
{
    public Collider shieldCollider; // Inspector���� �Ҵ�

    [HideInInspector]
    public bool isShieldActive = false;

    void Start()
    {
        if (shieldCollider != null)
            shieldCollider.enabled = false; // ���� �� ��Ȱ��ȭ
    }

    void Update()
    {
        // Left Shift ������ ���� Ȱ��ȭ
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isShieldActive = true;
            if (shieldCollider != null)
                shieldCollider.enabled = true;
        }
        // Left Shift ���� ���� ��Ȱ��ȭ
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isShieldActive = false;
            if (shieldCollider != null)
                shieldCollider.enabled = false;
        }
    }
}
