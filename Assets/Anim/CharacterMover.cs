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

        // Q Ű �� ���� ��� 1 �ֵθ���
        if (Input.GetKeyDown(KeyCode.Q))
        {
            animator.SetTrigger("attackTrigger");
            animator.SetInteger("attackIndex", 0);
            
        }

        // E Ű �� ���� ��� 2 ������
        if (Input.GetKeyDown(KeyCode.E))
        {
            animator.SetTrigger("attackTrigger");
            animator.SetInteger("attackIndex", 1);
            
        }

        // R Ű �� ���� ��� 3 ȸ������
        if (Input.GetKeyDown(KeyCode.R))
        {
            animator.SetTrigger("attackTrigger");
            animator.SetInteger("attackIndex", 2);

        }

    }

    void OnAnimatorMove()
    {
        // deltaPosition�� ����
        Vector3 deltaPos = animator.deltaPosition;
        deltaPos.y = 0.0f;  // ���纻�� Y�� 0����

        // �̵� �ݿ�
        transform.position += deltaPos;
        transform.rotation *= animator.deltaRotation;
    }
}
