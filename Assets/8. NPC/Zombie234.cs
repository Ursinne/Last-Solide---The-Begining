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
    public float health = 100f;
    public float maxHealth = 100f;
    
    [Header("Debug Settings")]
    public bool ignorePlayer = false; // Ändrat standardvärde till false

    [Header("Wandering Settings")]
    public float wanderRadius = 10f;
    public float minWanderTime = 5f;
    public float maxWanderTime = 15f;
    private float wanderTimer;
    private Vector3 wanderTarget;
    private bool isWandering = false;

    [Header("References")]
    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    private CharacterController controller;

    // Status-variabler
    private bool playerDetected = false;
    private float lastAttackTime;
    private Vector3 startPosition;
    private bool isDead = false;

    void Start()
    {
        // Spara startposition
        startPosition = transform.position;
        
        // Initiera hälsa
        health = maxHealth;

        // Hitta komponenter
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        // Hitta spelare oavsett om vi ignorerar eller inte
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            Debug.Log("Spelare hittad: " + player.name);
        }
        else
        {
            Debug.LogWarning("Ingen spelare med taggen 'Player' hittades!");
        }

        // Konfigurera NavMeshAgent
        if (agent != null)
        {
            agent.speed = walkSpeed;
            agent.stoppingDistance = attackRange;
            agent.isStopped = false;
            Debug.Log($"NavMeshAgent konfigurerad: isStopped: {agent.isStopped}, speed: {agent.speed}");
        }
        else
        {
            Debug.LogError("NavMeshAgent saknas på zombien! Vandring kommer inte fungera korrekt.");
        }

        // Starta vandringsruttinen direkt
        isWandering = true;
        StartWandering();
        Debug.Log("Zombie startar: börjar vandra");
    }

    void Update()
    {
        if (isDead) return;

        // Om vi ignorerar spelaren, koncentrera oss bara på vandring
        if (ignorePlayer)
        {
            HandleWandering();
            UpdateAnimations();
            return;
        }

        // Normal logik när vi inte ignorerar spelaren
        if (player != null)
        {
            // Beräkna avstånd till spelare
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            // Debugging
            Debug.Log($"Avstånd till spelare: {distanceToPlayer}, Detektionsräckvidd: {detectionRange}");

            // Detektera spelare
            playerDetected = distanceToPlayer <= detectionRange;

            if (playerDetected)
            {
                // Jaga eller attackera spelare
                if (distanceToPlayer > attackRange)
                {
                    Debug.Log("Spelaren upptäckt! Jagar...");
                    ChasePlayer();
                    
                    // Inte längre vandrande när spelaren är upptäckt
                    isWandering = false;
                }
                else
                {
                    Debug.Log("Spelaren inom attackräckvidd!");
                    AttackPlayer();
                    isWandering = false;
                }
            }
            else
            {
                // Hantera vandringsbeteende när spelaren inte är upptäckt
                Debug.Log("Spelaren ej upptäckt, fortsätter vandra");
                HandleWandering();
            }
        }
        else
        {
            // Ingen spelare hittad, bara vandra
            HandleWandering();
        }

        // Uppdatera animationer
        UpdateAnimations();
    }

    void HandleWandering()
    {
        // Minska vandringstimern
        wanderTimer -= Time.deltaTime;

        // Om timern är ute eller vi inte vandrar just nu, välj en ny destination
        if (wanderTimer <= 0 || !isWandering)
        {
            // 30% chans att stå stilla
            if (Random.value < 0.3f)
            {
                StopWandering();
                Debug.Log("Zombie stannar tillfälligt");
                wanderTimer = Random.Range(minWanderTime * 0.5f, maxWanderTime * 0.5f); // Kortare paus
            }
            else
            {
                StartWandering();
            }
        }

        // Om vi vandrar och har en agent, kontrollera om vi nått målet
        if (isWandering && agent != null && !agent.pathPending && agent.enabled)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                Debug.Log("Zombie har nått sitt mål, väljer en ny destination");
                StartWandering();
            }
        }
    }

    void StartWandering()
    {
        // Skapa en slumpmässig vandringsriktning
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection.y = 0;
        wanderTarget = startPosition + randomDirection;
        
        isWandering = true;
        wanderTimer = Random.Range(minWanderTime, maxWanderTime);
        
        Debug.Log($"Ny vandringsmål: {wanderTarget}, avstånd: {Vector3.Distance(transform.position, wanderTarget)}");
        
        // Om vi har en agent, använd NavMesh
        if (agent != null && agent.enabled)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(wanderTarget, out hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.speed = walkSpeed;
                agent.isStopped = false;
                agent.SetDestination(hit.position);
                Debug.Log($"NavMeshAgent destination: {hit.position}");
            }
            else
            {
                Debug.LogWarning("Kunde inte hitta en valid NavMesh-position. Försöker med en närmare position.");
                // Försök med en position närmare zombien
                if (NavMesh.SamplePosition(transform.position + Random.insideUnitSphere * (wanderRadius * 0.5f), 
                                         out hit, wanderRadius * 0.5f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                    Debug.Log($"Alternativ NavMeshAgent destination: {hit.position}");
                }
                else
                {
                    Debug.LogError("Kunde fortfarande inte hitta en valid NavMesh-position!");
                }
            }
        }
        else
        {
            Debug.LogWarning("NavMeshAgent saknas eller är inaktiverad");
            // Enklare direkt rörelse för fall utan NavMeshAgent
            if (controller != null)
            {
                StartCoroutine(SimpleWandering());
            }
        }
    }
    
    // En enkel wandringsrutin som inte kräver NavMeshAgent
    private IEnumerator SimpleWandering()
    {
        Vector3 targetPos = transform.position + Random.insideUnitSphere * wanderRadius;
        targetPos.y = transform.position.y;
        
        Vector3 direction = (targetPos - transform.position).normalized;
        float duration = Random.Range(3f, 8f);
        float timer = 0f;
        
        while (timer < duration && isWandering)
        {
            timer += Time.deltaTime;
            
            if (controller != null && controller.enabled)
            {
                controller.SimpleMove(direction * walkSpeed);
            }
            else
            {
                transform.position += direction * walkSpeed * Time.deltaTime;
            }
            
            // Rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, 
                                                Quaternion.LookRotation(direction), 
                                                2f * Time.deltaTime);
                                                
            yield return null;
        }
        
        // Välj en ny riktning
        wanderTimer = 0.1f;
    }

    void StopWandering()
    {
        if (agent != null)
        {
            agent.isStopped = true;
        }
        isWandering = false;
    }

    void ChasePlayer()
    {
        if (agent != null && agent.enabled)
        {
            // Stoppa eventuell vandring och ställ in förföljning
            StopWandering();
            
            // Sätt spelarens position som mål
            agent.isStopped = false;
            agent.SetDestination(player.position);
            agent.speed = runSpeed;
            
            Debug.Log($"Jagar spelare: speed={runSpeed}, path={agent.pathStatus}, remaining={agent.remainingDistance}");
        }
        else
        {
            // Manuell förföljning om NavMeshAgent inte fungerar
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            
            // Använd CharacterController om möjligt
            if (controller != null && controller.enabled)
            {
                controller.SimpleMove(direction * runSpeed);
                Debug.Log($"Jagar spelare med CharacterController: direction={direction}, speed={runSpeed}");
            }
            else
            {
                // Fallback: Direkt positionsförändring
                transform.position += direction * runSpeed * Time.deltaTime;
                Debug.Log($"Jagar spelare med direkt positionering: direction={direction}, speed={runSpeed}");
            }
            
            // Rotera mot spelaren
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                5f * Time.deltaTime
            );
        }
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
                Debug.Log("Spelar attackanimation");
            }

            // Skada spelare
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"Zombie attackerade spelaren och orsakade {attackDamage} skada!");
            }
            else
            {
                Debug.LogWarning("Spelaren har ingen PlayerHealth-komponent!");
            }
        }
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        float distanceToPlayer = player ? Vector3.Distance(transform.position, player.position) : float.MaxValue;
        
        // Uppdaterade villkor - separera beräkningar från debugging
        bool shouldAttack = !ignorePlayer && playerDetected && distanceToPlayer <= attackRange;
        bool shouldChase = !ignorePlayer && playerDetected && distanceToPlayer > attackRange;
        
        // Sätt animationsparametrar tydligt
        animator.SetBool("isWalking", isWandering);
        animator.SetBool("isRunning", shouldChase);
        
        // Om animator har dessa parametrar
        if (shouldAttack)
        {
            // Använd inte en bool för attack, utan en trigger
            animator.SetTrigger("Attack");
        }
        
        // Debug-utskrift efter att alla animationer är satta
        Debug.Log($"Animation: Walking={isWandering}, Running={shouldChase}, Attack={shouldAttack}");
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // Minska hälsa
        health -= damage;
        Debug.Log($"Zombie tog {damage} skada. Återstående hälsa: {health}");

        // Trigger skadad animation
        if (animator != null)
        {
            animator.SetTrigger("takeDamage");
        }

        // Kontrollera om zombien har dött
        if (health <= 0)
        {
            Die();
        }
        else
        {
            // Om vi tar skada men inte dör, vänd oss mot spelaren
            if (player != null && !ignorePlayer)
            {
                playerDetected = true;
                StopWandering();
                Debug.Log("Tog skada och upptäckte spelaren!");
            }
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Zombie har dött!");

        // Stoppa all rörelse
        isWandering = false;
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Deaktivera alla colliders
        Collider[] colliders = GetComponents<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        // Deaktivera CharacterController om det finns en
        if (controller != null)
        {
            controller.enabled = false;
        }

        // Sätt död-animationsparametrar
        if (animator != null)
        {
            animator.SetTrigger("die");
            animator.SetBool("isDead", true);
        }

        // Förstör efter en fördröjning
        Destroy(gameObject, 5f);
    }

    // För att visualisera räckvidden i editorn
    private void OnDrawGizmosSelected()
    {
        // Visa detektionsradie
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Visa attackradie
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Visa vandringsradie
        Gizmos.color = Color.blue;
        Vector3 startPos = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(startPos, wanderRadius);
    }
}