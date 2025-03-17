using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 20f;  // Kulans hastighet
    public float damage = 10f; // Skada kulan orsakar
    public float lifetime = 5f; // Hur l�nge kulan finns kvar

    [Header("Physics")]
    public Rigidbody rb;

    private void Awake()
    {
        // S�kerst�ll att Rigidbody finns
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // Skjut iv�g kulan
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
        }

        // F�rst�r kulan efter en viss tid
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Hantera kollision
        // Exempelvis skada fiender
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        // Ta bort kulan n�r den tr�ffar n�got
        Destroy(gameObject);
    }
}