using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class IKControlRight : MonoBehaviour
{
    protected Animator animator;

    [Header("IK Settings")]
    public bool ikActive = false;
    public Transform rightHandObj = null;
    public Transform lookObj = null;

    [Header("IK Weights")]
    [Range(0, 1)] public float rightHandPositionWeight = 1.0f;
    [Range(0, 1)] public float rightHandRotationWeight = 1.0f;
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
            Debug.LogError("IKControlRight: Ingen Animator-komponent hittades!");
            enabled = false;
            return;
        }

        if (showDebug)
        {
            Debug.Log("IKControlRight: Initierad med IK " + (ikActive ? "aktiv" : "inaktiv"));
        }
    }

    void Update()
    {
        // Kontrollera om IK-status har ändrats
        if (wasIkActive != ikActive)
        {
            if (showDebug)
            {
                Debug.Log("IKControlRight: IK " + (ikActive ? "aktiverad" : "inaktiverad"));
            }
            wasIkActive = ikActive;
        }

        // Kontrollera om handmålet har ändrats
        if (lastHandTarget != rightHandObj)
        {
            if (showDebug && rightHandObj != null)
            {
                Debug.Log("IKControlRight: Nytt handmål satt: " + rightHandObj.name);
            }
            lastHandTarget = rightHandObj;
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
                    Debug.Log("IKControlRight: Tittar på " + lookObj.name);
                }
            }

            // Sätt höger hands målposition och rotation, om ett mål har tilldelats
            if (rightHandObj != null)
            {
                // Animatorn kanske inte är förberedd för IK på varje frame
                try
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandPositionWeight);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandRotationWeight);
                    animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandObj.position);
                    animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandObj.rotation);

                    if (showDebug && Time.frameCount % 60 == 0)
                    {
                        Debug.Log("IKControlRight: Höger hand placerad på " + rightHandObj.name);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("IKControlRight: Fel vid IK-hantering: " + e.Message);
                    ikActive = false;
                }
            }
        }
        // Om IK inte är aktiv, återställ hand och huvuds position och rotation
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            animator.SetLookAtWeight(0);
        }
    }

    // Publika metoder för att aktivera/inaktivera IK från andra skript
    public void SetTarget(Transform target, bool activateIK = true)
    {
        rightHandObj = target;
        ikActive = activateIK;

        if (showDebug)
        {
            Debug.Log("IKControlRight: Mål satt till " + (target != null ? target.name : "null") + ", IK " + (activateIK ? "aktiverad" : "inaktiverad"));
        }
    }

    public void ClearTarget()
    {
        rightHandObj = null;
        ikActive = false;

        if (showDebug)
        {
            Debug.Log("IKControlRight: Mål rensad, IK inaktiverad");
        }
    }

    public void SetLookTarget(Transform target, float weight = 0.5f)
    {
        lookObj = target;
        lookAtWeight = weight;

        if (showDebug)
        {
            Debug.Log("IKControlRight: Look-mål satt till " + (target != null ? target.name : "null") + ", vikt: " + weight);
        }
    }

    // För debugging i editorn
    private void OnDrawGizmos()
    {
        if (!showDebug) return;

        if (rightHandObj != null && ikActive)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(rightHandObj.position, 0.05f);
            Gizmos.DrawLine(transform.position, rightHandObj.position);
        }

        if (lookObj != null && ikActive)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(lookObj.position, 0.05f);
            Gizmos.DrawLine(transform.position + Vector3.up * 1.7f, lookObj.position);
        }
    }
}