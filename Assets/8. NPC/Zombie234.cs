using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Zombie234 : MonoBehaviour
{
    [Header("Zombie Settings")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 4.0f;
    public float detectionRange = 15.0f;
    public float attackRange = 2.0f;
    public float attackDamage = 10f;
    public float attackCooldown = 2.0f;

    [Header("References")]
    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    private CharacterController controller;

    private bool playerDetected = false;
    private float lastAttackTime;
    private bool isDead = false;

    void Start()
    {
        // Hitta komponenter
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        // Hitta spelare
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("Ingen spelare hittad!");
        }

        // Konfigurera NavMeshAgent
        if (agent != null)
        {
            agent.speed = walkSpeed;
            agent.stoppingDistance = attackRange;
        }
    }

    void Update()
    {
        if (isDead || player == null) return;

        // Beräkna avstånd till spelare
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Detektera spelare
        playerDetected = distanceToPlayer <= detectionRange;

        if (playerDetected)
        {
            // Jaga eller attackera spelare
            if (distanceToPlayer > attackRange)
            {
                ChasePlayer();
            }
            else
            {
                AttackPlayer();
            }
        }
        else
        {
            // Idle eller vandra
            StopChasing();
        }

        UpdateAnimations();
    }

    void ChasePlayer()
    {
        if (agent != null)
        {
            agent.SetDestination(player.position);
            agent.speed = runSpeed;
        }

        if (agent != null)
        {
            Debug.Log($"Agent status: " +
                $"Enabled: {agent.enabled}, " +
                $"Path Pending: {agent.pathPending}, " +
                $"Remaining Distance: {agent.remainingDistance}, " +
                $"Distance to Player: {Vector3.Distance(transform.position, player.position)}");
        }

        // Rotera mot spelare
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direction),
            5f * Time.deltaTime
        );
    }

    void AttackPlayer()
    {
        // Stoppa rörelse
        if (agent != null)
        {
            agent.isStopped = true;
        }

        // Rotera mot spelare
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);

        // Attackera med cooldown
        if (Time.time > lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;

            // Trigger attack animation
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            // Skada spelare
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    void StopChasing()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            agent.speed = walkSpeed;
        }
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        // Uppdatera animationer baserat på rörelse
        bool isMoving = playerDetected &&
            Vector3.Distance(transform.position, player.position) > attackRange;

        animator.SetFloat("VelocityY", isMoving ? 1 : 0);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // Trigger skadad animation
        if (animator != null)
        {
            animator.SetTrigger("TakeDamage");
        }

        // Logik för att dö
        if (damage >= 100) // Eller din specifika hälsologik
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        // Stoppa all rörelse
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Dödsanimation
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Förstör efter en fördröjning
        Destroy(gameObject, 3f);
    }
}