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

        // Root Motion 직접 처리할 준비
        animator.applyRootMotion = true;
    }

    void Update()
    {
        // 스페이스: 칼 꺼내기
        //if (Input.GetKeyDown(KeyCode.Space) && !hasDrawnSword)
        //{
           // animator.SetTrigger("DrawSword");
            hasDrawnSword = true;
       // }

        // 칼 꺼낸 이후
        if (hasDrawnSword)
        {
            // W 누르면 걷기 시작
            if (Input.GetKeyDown(KeyCode.W))
            {
              
               
                animator.SetFloat("v", 1.0f); // 애니메이션 먼저 전환
                //playerTr.Translate(0, 0, moveSpeed * Time.deltaTime);
            }

            // W 떼면 멈춤
            //else
            //{
               // animator.SetFloat("v", 0.0f);
            //}
            
        }
    }

    void OnAnimatorMove()
    {
        // Animator의 Root Motion delta를 직접 적용
        if (animator.applyRootMotion)
        {
            // deltaPosition으로 실제 이동 반영
            transform.position += animator.deltaPosition;

            // deltaRotation으로 실제 회전 반영 (필요 시)
            transform.rotation *= animator.deltaRotation;
        }
    }
}
