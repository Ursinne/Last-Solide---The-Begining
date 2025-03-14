using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class IKControlLeft : MonoBehaviour
{
    protected Animator animator;

    [Header("IK Settings")]
    public bool ikActive = false;
    public Transform leftHandObj = null;
    public Transform lookObj = null;

    [Header("IK Weights")]
    [Range(0, 1)] public float leftHandPositionWeight = 1.0f;
    [Range(0, 1)] public float leftHandRotationWeight = 1.0f;
    [Range(0, 1)] public float lookAtWeight = 0.5f;

    [Header("Debug")]
    public bool showDebug = false;

    private bool wasIkActive = false;
    private Transform lastHandTarget = null;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("IKControlLeft: Ingen Animator-komponent hittades!");
            enabled = false;
            return;
        }

        if (showDebug)
        {
            Debug.Log("IKControlLeft: Initierad med IK " + (ikActive ? "aktiv" : "inaktiv"));
        }
    }

    void Update()
    {
        // Kontrollera om IK-status har ändrats
        if (wasIkActive != ikActive)
        {
            if (showDebug)
            {
                Debug.Log("IKControlLeft: IK " + (ikActive ? "aktiverad" : "inaktiverad"));
            }
            wasIkActive = ikActive;
        }

        // Kontrollera om handmålet har ändrats
        if (lastHandTarget != leftHandObj)
        {
            if (showDebug && leftHandObj != null)
            {
                Debug.Log("IKControlLeft: Nytt handmål satt: " + leftHandObj.name);
            }
            lastHandTarget = leftHandObj;
        }
    }

    // En callback för att beräkna IK
    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        // Om IK är aktiv, sätt position och rotation direkt till målet
        if (ikActive)
        {
            // Sätt look target positionen, om en sådan har tilldelats
            if (lookObj != null)
            {
                animator.SetLookAtWeight(lookAtWeight);
                animator.SetLookAtPosition(lookObj.position);

                if (showDebug && Time.frameCount % 60 == 0) // Logga var 60:e frame för att inte spamma
                {
                    Debug.Log("IKControlLeft: Tittar på " + lookObj.name);
                }
            }

            // Sätt vänster hands målposition och rotation, om ett mål har tilldelats
            if (leftHandObj != null)
            {
                // Animatorn kanske inte är förberedd för IK på varje frame
                try
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandPositionWeight);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandRotationWeight);
                    animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandObj.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandObj.rotation);

                    if (showDebug && Time.frameCount % 60 == 0)
                    {
                        Debug.Log("IKControlLeft: Vänster hand placerad på " + leftHandObj.name);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("IKControlLeft: Fel vid IK-hantering: " + e.Message);
                    ikActive = false;
                }
            }
        }
        // Om IK inte är aktiv, återställ hand och huvuds position och rotation
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
            animator.SetLookAtWeight(0);
        }
    }

    // Publika metoder för att aktivera/inaktivera IK från andra skript
    public void SetTarget(Transform target, bool activateIK = true)
    {
        leftHandObj = target;
        ikActive = activateIK;

        if (showDebug)
        {
            Debug.Log("IKControlLeft: Mål satt till " + (target != null ? target.name : "null") + ", IK " + (activateIK ? "aktiverad" : "inaktiverad"));
        }
    }

    public void ClearTarget()
    {
        leftHandObj = null;
        ikActive = false;

        if (showDebug)
        {
            Debug.Log("IKControlLeft: Mål rensad, IK inaktiverad");
        }
    }

    public void SetLookTarget(Transform target, float weight = 0.5f)
    {
        lookObj = target;
        lookAtWeight = weight;

        if (showDebug)
        {
            Debug.Log("IKControlLeft: Look-mål satt till " + (target != null ? target.name : "null") + ", vikt: " + weight);
        }
    }

    // För debugging i editorn
    private void OnDrawGizmos()
    {
        if (!showDebug) return;

        if (leftHandObj != null && ikActive)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(leftHandObj.position, 0.05f);
            Gizmos.DrawLine(transform.position, leftHandObj.position);
        }

        if (lookObj != null && ikActive)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(lookObj.position, 0.05f);
            Gizmos.DrawLine(transform.position + Vector3.up * 1.7f, lookObj.position);
        }
    }
}