using UnityEngine;
using InventoryAndCrafting;


public class PickupWeapon : MonoBehaviour
{
    public WeaponSystem weaponData;
    public float pickupRange = 3f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            float distance = Vector3.Distance(transform.position,
                                              GameObject.FindWithTag("Player").transform.position);
            if (distance < pickupRange)
            {
                Pickup();
            }
        }
    }

    private void Pickup()
    {
        //if (weaponData != null && InventoryManager.Instance != null)
        //{
        //    bool added = InventoryManager.Instance.AddWeapon(weaponData);
        //    if (added)
        //    {
        //        Destroy(gameObject);
        //    }
        //}
    }
}