using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAnimationController : MonoBehaviour
{
    private Animator animator;

    private int _animIDSpeed;

    void Awake()
    {
        animator = GetComponent<Animator>();
        AssignAnimationIDs();
    }

    void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
    }

    public void SetSpeed(float value)
    {
        animator.SetFloat(_animIDSpeed, value);
    }
}
