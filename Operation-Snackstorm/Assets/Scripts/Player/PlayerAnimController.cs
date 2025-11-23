using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimController : MonoBehaviour
{
    private Animator animator;

    private int _animIDSpeed;
    private int _animIDJump;
    private int _animIDThrow;
    private int _animIDSkate;
    private int _animIDFallDown;
    private int _animIDMop;
    private int _animIDAttack;

    void Awake()
    {
        animator = GetComponent<Animator>();
        AssignAnimationIDs();
    }

    void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDThrow = Animator.StringToHash("isThrow");
        _animIDSkate = Animator.StringToHash("Skate");
        _animIDFallDown = Animator.StringToHash("FallDown");
        _animIDMop = Animator.StringToHash("Mopping");
        _animIDAttack = Animator.StringToHash("Attack");
    }

    public void SetSpeed(float value)
    {
        animator.SetFloat(_animIDSpeed, value);
    }

    public void SetSkate(bool isSkating)
    {
        animator.SetBool(_animIDSkate, isSkating);
    }

    public void SetFallDown(bool isFalling)
    {
        animator.SetBool(_animIDFallDown, isFalling);
    }

    public void SetThrow(bool isThrowing)
    {
        animator.SetBool(_animIDThrow, isThrowing);
    }

    public void SetMop(bool isMop)
    {
        animator.SetBool(_animIDMop, isMop);
    }


    public void SetAttack(bool isAttacking)
    {
        animator.SetBool (_animIDAttack, isAttacking);
    }

    public bool GetFallDown()
    {
        return animator.GetBool(_animIDFallDown);
    }
}
