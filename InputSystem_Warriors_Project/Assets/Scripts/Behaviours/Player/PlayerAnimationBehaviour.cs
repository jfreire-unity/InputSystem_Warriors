using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationBehaviour : MonoBehaviour
{
    [Header("Component References")]
    public Animator playerAnimator;

    //Animation String IDs
    private int playerMovementAnimationID;
    private int playerAttackAnimationID;
    
    [NonSerialized]
    public bool attackInProgress;

    public void SetupBehaviour()
    {
        SetupAnimationIDs();
    }

    void SetupAnimationIDs()
    {
        playerMovementAnimationID = Animator.StringToHash("Movement");
        playerAttackAnimationID = Animator.StringToHash("Attack");
    }

    public void UpdateMovementAnimation(float movementBlendValue)
    {
        playerAnimator.SetFloat(playerMovementAnimationID, movementBlendValue);
    }

    public void PlayAttackAnimation()
    {
        playerAnimator.SetTrigger(playerAttackAnimationID);
    }
    
    void AttackInProgress(int value)
    {
        attackInProgress = value == 1;
        Debug.Log("Attack in progress: " + attackInProgress);
    }
}
