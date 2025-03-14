using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using InventoryAndCrafting;

public class PickupItem : MonoBehaviour
{
    public ItemData itemData; // Referens till ItemData
    public int amount = 1;
    public float pickupRange = 3f; // Pickup avstånd
    public KeyCode pickupKey = KeyCode.F; // Tangent för att plocka upp

    [Header("Animation Settings")]
    public string pickupAnimationTrigger = "Pickup"; // Namn på animation-trigger
    public string pickupAnimationBool = "isPickingUp"; // Alternativ animation som bool
    public float animationDuration = 1.0f; // Hur länge animationen varar

    [Header("Feedback")]
    public AudioClip pickupSound; // Ljud när man plockar upp
    public GameObject pickupEffectPrefab; // Visuell effekt när man plockar upp

    private AudioSource audioSource;

    private void Start()
    {
        // Lägg till AudioSource om den inte redan finns
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && pickupSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f; // 3D ljud
            audioSource.maxDistance = 10f;
            audioSource.volume = 0.7f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player is nearby, you can pick up the item.");

            // Visa en tooltip eller interaktionstext om du vill
            // UI_InteractionManager.Instance?.ShowInteractionText($"Press {pickupKey} to pick up {itemData?.itemName}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Dölj interaktionstext när spelaren går bort
            // UI_InteractionManager.Instance?.HideInteractionText();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(pickupKey)) // tangent för att plocka upp
        {
            // Hitta spelaren
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) return;

            float distance = Vector3.Distance(transform.position, player.transform.position);

            // Kolla om spelaren är nära nog att plocka upp föremålet
            if (distance < pickupRange)
            {
                Debug.Log("Player is close enough to pick up the item.");
                Pickup(player);
            }
            else
            {
                Debug.Log("Player is not close enough to pick up the item.");
            }
        }
    }

    // Uppdaterade Pickup-metoden som tar emot spelarobjektet
    private void Pickup(GameObject player)
    {
        Debug.Log($"Plockar upp: {itemData?.itemName}");

        // Spela upp animation
        PlayPickupAnimation(player);

        // Spela upplock-ljud om det finns
        if (audioSource != null && pickupSound != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f); // Lägg till lite variation
            audioSource.PlayOneShot(pickupSound);
        }

        // Skapa visuell effekt om det finns en sådan
        if (pickupEffectPrefab != null)
        {
            GameObject effect = Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f); // Ta bort efter 2 sekunder
        }

        // Använd InventoryManager för att lägga till föremålet
        if (itemData != null && InventoryManager.Instance != null)
        {
            bool added = InventoryManager.Instance.AddItem(itemData, amount);
            if (added)
            {
                QuestManager.Instance?.UpdateQuestProgress("gather", itemData.itemName, amount);

                // Visa notifikation via NotificationManager
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowItemNotification(itemData, amount);
                    Debug.Log("Notifikation visad via NotificationManager");
                }
                else
                {
                    Debug.LogWarning("NotificationManager.Instance är null!");
                }

                Destroy(gameObject);
            }
        }
        else
        {
            Debug.LogError("ItemData eller InventoryManager saknas!");
        }
    }

    private void PlayPickupAnimation(GameObject player)
    {
        // Försök först med PlayerAnimationController om det finns
        PlayerAnimationController animController = player.GetComponent<PlayerAnimationController>();
        if (animController != null)
        {
            // Använd bool-animation om den är tillgänglig
            if (!string.IsNullOrEmpty(pickupAnimationBool))
            {
                animController.PlayAnimation(pickupAnimationBool, animationDuration);
                Debug.Log($"Spelar pickup-animation via PlayerAnimationController: {pickupAnimationBool}");
                return;
            }

            // Annars använd trigger
            bool success = animController.TriggerAnimation(pickupAnimationTrigger);
            if (success)
            {
                Debug.Log($"Spelar pickup-animation via PlayerAnimationController trigger: {pickupAnimationTrigger}");
                return;
            }
        }

        // Fallback till att använda Animator direkt
        Animator playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator == null)
            playerAnimator = player.GetComponentInChildren<Animator>();

        if (playerAnimator != null)
        {
            // Försök med trigger först
            foreach (AnimatorControllerParameter param in playerAnimator.parameters)
            {
                if (param.name == pickupAnimationTrigger && param.type == AnimatorControllerParameterType.Trigger)
                {
                    playerAnimator.SetTrigger(pickupAnimationTrigger);
                    Debug.Log($"Spelar pickup-animation via Animator trigger: {pickupAnimationTrigger}");
                    return;
                }
                else if (param.name == pickupAnimationBool && param.type == AnimatorControllerParameterType.Bool)
                {
                    // Sätt bool, återställ efter en kort stund
                    playerAnimator.SetBool(pickupAnimationBool, true);
                    StartCoroutine(ResetAnimationBool(playerAnimator, pickupAnimationBool, animationDuration));
                    Debug.Log($"Spelar pickup-animation via Animator bool: {pickupAnimationBool}");
                    return;
                }
            }

            // Försök med generisk "Pickup" trigger om den finns
            foreach (string genericTrigger in new[] { "Pickup", "PickUp", "PickItem", "GetItem", "Take", "Grab" })
            {
                foreach (AnimatorControllerParameter param in playerAnimator.parameters)
                {
                    if (param.name == genericTrigger && param.type == AnimatorControllerParameterType.Trigger)
                    {
                        playerAnimator.SetTrigger(genericTrigger);
                        Debug.Log($"Spelar pickup-animation via generisk trigger: {genericTrigger}");
                        return;
                    }
                }
            }

            Debug.LogWarning("Kunde inte hitta någon lämplig animation för pickup. " +
                          "Lägg till parametern 'Pickup' (trigger) eller 'isPickingUp' (bool) till din Animator.");
        }
        else
        {
            Debug.LogWarning("Kunde inte hitta Animator på spelaren. Ingen pickup-animation spelades.");
        }
    }

    private System.Collections.IEnumerator ResetAnimationBool(Animator animator, string paramName, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (animator != null)
        {
            animator.SetBool(paramName, false);
        }
    }
}

//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using System.Collections.Generic;
//using InventoryAndCrafting;

//public class PickupItem : MonoBehaviour
//{
//    public ItemData itemData; // Teferens till ItemDAta
//    public int amount = 1;
//    public float pickupRange = 10; // Öka avståndet för upphämtning för att säkerställa att det räcker
//    //public InventoryItem item;  // den lag jag till nu tabort och testa
//    //public Sprite iconIcon;



//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Player"))
//        {
//            Debug.Log("Player is nearby, you can pick up the item.");
//        }
//    }

//    private void OnTriggerExit(Collider other)
//    {
//        if (other.CompareTag("Player"))
//        {
//            //Debug.Log("Player left the area, you can no longer pick up the item.");
//        }
//    }

//    private void Update()
//    {
//        //Debug.Log("F key pressed.");
//        if (Input.GetKeyDown(KeyCode.F)) // tangent för att plocka upp
//        {
//            Debug.Log($"Distance to player: {pickupRange}");
//            float distance = Vector3.Distance(transform.position, GameObject.FindWithTag("Player").transform.position);

//            // Kolla om spelaren är nära nog att plocka upp föremålet
//            if (distance < pickupRange)
//            {
//               Debug.Log("Player is close enough to pick up the item.");
//                Pickup();
//            }
//            else
//            {
//                Debug.Log("Player is not close enough to pick up the item.");
//            }
//        }
//    }

//    // I PickupItem.cs funktionen Pickup()
//    private void Pickup()
//    {


//        Debug.Log($"Plockar upp: {itemData?.itemName}");

//        // Använd InventoryManager för att lägga till föremålet
//        if (itemData != null && InventoryManager.Instance != null)
//        {
//            bool added = InventoryManager.Instance.AddItem(itemData, amount);
//            if (added)
//            {
//                QuestManager.Instance?.UpdateQuestProgress("gather", itemData.itemName, amount);
//                // Visa notifikation via NotificationManager
//                if (NotificationManager.Instance != null)
//                {
//                    NotificationManager.Instance.ShowItemNotification(itemData, amount);
//                    Debug.Log("Notifikation visad via NotificationManager");
//                }
//                else
//                {
//                    Debug.LogWarning("NotificationManager.Instance är null!");
//                }

//                Destroy(gameObject);
//            }
//        }
//        else
//        {
//            Debug.LogError("ItemData eller InventoryManager saknas!");
//        }
//    }
//}
