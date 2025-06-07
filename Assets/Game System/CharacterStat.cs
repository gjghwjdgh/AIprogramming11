using UnityEngine;

public class Character : MonoBehaviour, IDamageable
{
    public enum Side { Left, Right }
    public Side side; // Inspector���� ĳ���͸��� Left/Right�� ����

    public float health = 100f;
    public float maxHealth = 100f;

    public void TakeDamage(float damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);
        Debug.Log($"{gameObject.name}�� {damage}�� �������� ����. ���� ü��: {health}");

        Debug.Log($"=== side Ȯ��: {side} ===");  // �̰� �߰�!!

        // UI ����
        if (side == Side.Left)
            TestUIController.Instance.SetLeftHealth(health, maxHealth);
        else
            TestUIController.Instance.SetRightHealth(health, maxHealth);

        if (health <= 0)
            Die();
    }

    void Die()
    {
        // ��� ó��
    }
}
