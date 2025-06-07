using UnityEngine;

public class Character : MonoBehaviour, IDamageable
{
    public enum Side { Left, Right }
    public Side side; // Inspector에서 캐릭터마다 Left/Right로 지정

    public float health = 100f;
    public float maxHealth = 100f;

    public void TakeDamage(float damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth);
        Debug.Log($"{gameObject.name}가 {damage}의 데미지를 입음. 남은 체력: {health}");

        Debug.Log($"=== side 확인: {side} ===");  // 이거 추가!!

        // UI 갱신
        if (side == Side.Left)
            TestUIController.Instance.SetLeftHealth(health, maxHealth);
        else
            TestUIController.Instance.SetRightHealth(health, maxHealth);

        if (health <= 0)
            Die();
    }

    void Die()
    {
        // 사망 처리
    }
}
