using System;
using System.Collections;
using System.Collections.Generic;
using Script.Enemy;
using UnityEngine;

public class PlayerAttacking : MonoBehaviour
{
    private enum AttackState
    {
        Idle,
        Attack1,
        Attack2
    }
    
    [SerializeField] private PlayerAttackArea attackArea;
    [SerializeField] private AttackState currentAttackState = AttackState.Idle;
    [SerializeField] private Animator armAnimator; 
    
    [SerializeField] private Transform attack1Plane;
    [SerializeField] private Transform attack2Plane;
    

    // --- NEW: Timing Delays ---
    [Header("Impact Timings")]
    [Tooltip("Time in seconds before the Attack 1 hitbox is active")]
    [SerializeField] private float attack1HitDelay = 0.2f; 
    [Tooltip("Time in seconds before the Attack 2 hitbox is active")]
    [SerializeField] private float attack2HitDelay = 0.25f;

    private bool nextAttackQueued = false;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (currentAttackState == AttackState.Idle)
            {
                StartCombo();
            }
            else if (currentAttackState == AttackState.Attack1)
            {
                nextAttackQueued = true;
            }
        }
    }

    private void StartCombo()
    {
        currentAttackState = AttackState.Attack1;
        nextAttackQueued = false;

        armAnimator.Play("Attack1");
        
        // Fire the delayed attack execution instead of instantaneous
        StartCoroutine(DelayedExecuteAttack(attack1Plane, attack1HitDelay));
        
        StartCoroutine(WaitAndTransition(CheckCombo));
    }

    private void CheckCombo()
    {
        if (nextAttackQueued)
        {
            Time.timeScale =  1f;
            currentAttackState = AttackState.Attack2;
            nextAttackQueued = false;

            armAnimator.Play("Attack2"); 

            // Fire the delayed attack execution for the second swing
            StartCoroutine(DelayedExecuteAttack(attack2Plane, attack2HitDelay));
            
            StartCoroutine(WaitAndTransition(BackToIdle));
        }
        else
        {
            Time.timeScale = 1;
            BackToIdle();
        }
    }

    private void BackToIdle()
    {
        currentAttackState = AttackState.Idle;
        nextAttackQueued = false;
        
        armAnimator.Play("Idle");
    }

    private IEnumerator WaitAndTransition(Action nextStateMethod)
    {
        yield return null; 
        
        float currentAnimLength = armAnimator.GetCurrentAnimatorStateInfo(0).length;
        
        yield return new WaitForSeconds(currentAnimLength);
        
        nextStateMethod.Invoke();
    }
    
    // --- UPDATED: Continuous Scanning Coroutine ---
    private IEnumerator DelayedExecuteAttack(Transform activePlane, float delayTime)
    {
        float timer = 0f;
        bool slowMoTriggered = false;

        // Loop every frame until our visual wind-up delay is reached
        while (timer < delayTime)
        {
            // If we haven't triggered slow-mo yet, keep scanning the hitbox!
            if (!slowMoTriggered && attackArea != null)
            {
                GameObject[] earlyTargets = attackArea.GetTargetsInSwing();
                
                // The moment an enemy gets pulled into the hitbox during the wind-up, drop time!
                if (earlyTargets != null && earlyTargets.Length > 0)
                {
                    Time.timeScale = 0.5f; 
                    slowMoTriggered = true;
                }
            }

            // Because we use Time.deltaTime, the moment timeScale drops to 0.35, 
            // this timer slows down with it. This ensures the code delay 
            // stretches perfectly alongside your slow-mo Animator!
            timer += Time.deltaTime;
            
            // Wait for the next frame
            yield return null; 
        }
        
        // The wind-up is over, and the sword has visually connected. Fire the cut!
        ExecuteHitboxLogic(activePlane);
        yield return new WaitForSeconds(0.02f);
        Time.timeScale = 1f;
    }
    private void ExecuteHitboxLogic(Transform activePlane)
    {
        // Safety check for the attack area script itself
        if (attackArea == null) return;

        // Fire the instantaneous OverlapBox cast to get our targets right NOW
        GameObject[] targetsToProcess = attackArea.GetTargetsInSwing();

        // If the enemy dodged or the player turned the camera away during the delay,
        // snap time back to normal and abort the hit logic.
        if (targetsToProcess == null || targetsToProcess.Length == 0)
        {
            Time.timeScale = 1f;
            return;
        }

        int parriableLayer = LayerMask.NameToLayer("Parriable");
        LayerMask splitAndDamagable = LayerMask.GetMask("SplittableObject") | GlobalReference.Instance.EnemyLayer;

        // We absolutely KEEP this HashSet! It is still crucial for preventing multiple 
        // child colliders on the same enemy from triggering SplitDeath multiple times.
        HashSet<IDamagable> hitTargets = new();
        
        foreach (GameObject obj in targetsToProcess)
        {
            // GetTargetsInSwing() already filters out nulls, but keeping this is a good defensive habit
            if (obj == null) continue; 
    
            if (((1 << obj.layer) & splitAndDamagable) != 0)
            {
                var damagable = obj.GetComponentInParent<IDamagable>();

                if (damagable != null && hitTargets.Add(damagable))
                {
                    damagable.SplitDeath(activePlane);
                    // REMOVED: Time.timeScale = 0.35f; 
                    // (It is now handled smoothly in the wind-up!)
                }
            }
            else if (((1 << obj.layer) & parriableLayer) != 0)
            {
                if (obj.TryGetComponent(out IParriable parriable))
                {
                    parriable.Parried();
                }
            }
        }
    }
}