using UnityEngine;

public class RootMotionMover : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = true;  //  �ݵ�� �Ѿ� ��
    }

    void Update()
    {
        float v = 0.0f;

        if (Input.GetKey(KeyCode.W))
            v = 1.0f;
        else if (Input.GetKey(KeyCode.S))
            v = -1.0f;

        else
        {
            v = 0;
        }


        // �ִϸ����Ϳ� v ���� ���� �� �ִϸ��̼� ��ȯ�� ���
        animator.SetFloat("v", v);
    }

    void OnAnimatorMove()
    {
        // Animator�� Root Motion �̵� �����͸� Transform�� ���� ����
        transform.position += animator.deltaPosition;
        transform.rotation *= animator.deltaRotation;
    }
}
