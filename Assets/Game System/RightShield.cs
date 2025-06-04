using UnityEngine;

public class SimpleShield : MonoBehaviour
{
    public Collider shieldCollider; // Inspector���� Shield�� Collider�� �Ҵ�

    void Start()
    {
        if (shieldCollider != null)
            shieldCollider.enabled = false; // ���� �� ��Ȱ��ȭ
    }

    void Update()
    {
        // Right Shift ������ ���� Ȱ��ȭ
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            if (shieldCollider != null)
                shieldCollider.enabled = true;
        }
        // Right Shift ���� ���� ��Ȱ��ȭ
        if (Input.GetKeyUp(KeyCode.RightShift))
        {
            if (shieldCollider != null)
                shieldCollider.enabled = false;
        }
    }
}
