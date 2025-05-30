using UnityEngine;
using System.Collections;
using System.Collections.Generic;






public class CharacterControl : MonoBehaviour
{
    private Animator animator;
    private Transform playerTr;

    public float moveSpeed = 2.0f;
    private bool hasDrawnSword = false;
    private bool isWalking = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerTr = GetComponent<Transform>();

        // Root Motion ���� ó���� �غ�
        animator.applyRootMotion = true;
    }

    void Update()
    {
        // �����̽�: Į ������
        //if (Input.GetKeyDown(KeyCode.Space) && !hasDrawnSword)
        //{
           // animator.SetTrigger("DrawSword");
            hasDrawnSword = true;
       // }

        // Į ���� ����
        if (hasDrawnSword)
        {
            // W ������ �ȱ� ����
            if (Input.GetKeyDown(KeyCode.W))
            {
              
               
                animator.SetFloat("v", 1.0f); // �ִϸ��̼� ���� ��ȯ
                //playerTr.Translate(0, 0, moveSpeed * Time.deltaTime);
            }

            // W ���� ����
            //else
            //{
               // animator.SetFloat("v", 0.0f);
            //}
            
        }
    }

    void OnAnimatorMove()
    {
        // Animator�� Root Motion delta�� ���� ����
        if (animator.applyRootMotion)
        {
            // deltaPosition���� ���� �̵� �ݿ�
            transform.position += animator.deltaPosition;

            // deltaRotation���� ���� ȸ�� �ݿ� (�ʿ� ��)
            transform.rotation *= animator.deltaRotation;
        }
    }
}
