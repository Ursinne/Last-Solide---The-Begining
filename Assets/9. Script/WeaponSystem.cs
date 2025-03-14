using UnityEngine;
using InventoryAndCrafting;

public enum WeaponType
{
    None,
    Sword,
    Rifle,
    Explosive
}

public class WeaponSystem : MonoBehaviour
{
    public WeaponType weaponType = WeaponType.None;
    public float damage = 10f;
    public float range = 5f;
    public float fireRate = 1f;
    public Vector3 weaponPositionOffset = Vector3.zero;
    public Vector3 weaponRotationOffset = Vector3.zero;
    public bool usesTwoHands = false;
    public Vector3 leftHandPosition = new Vector3(0, 0, -0.3f);

    public string animationTrigger = "";
    public string animationBoolParam = "";

    private Transform playerTransform;
    private Animator playerAnimator;
    private PlayerSkills playerSkills;
    private IKControlLeft leftHandIK;
    private PlayerAnimationController animationController;

    private bool isEquipped = false;

    private void Awake()
    {
        SetAnimationTriggerForWeaponType();
    }

    public void Initialize(Transform player)
    {
        playerTransform = player;

        if (player != null)
        {
            playerAnimator = player.GetComponentInChildren<Animator>();
            playerSkills = player.GetComponent<PlayerSkills>();
            leftHandIK = player.GetComponentInChildren<IKControlLeft>();
            animationController = player.GetComponentInChildren<PlayerAnimationController>();
        }

        ConfigureDefaultSettings();
        isEquipped = true;

        if (usesTwoHands)
        {
            ConfigureTwoHandedGrip();
        }
    }

    private void ConfigureTwoHandedGrip()
    {
        if (!usesTwoHands || leftHandIK == null) return;

        GameObject leftHandTarget = new GameObject("LeftHandTarget");
        leftHandTarget.transform.SetParent(transform);
        leftHandTarget.transform.localPosition = leftHandPosition;

        leftHandIK.leftHandObj = leftHandTarget.transform;
        leftHandIK.ikActive = true;
    }

    private void ConfigureDefaultSettings()
    {
        switch (weaponType)
        {
            case WeaponType.Rifle:
                usesTwoHands = true;
                break;
        }

        if (usesTwoHands)
        {
            switch (weaponType)
            {
                case WeaponType.Rifle:
                    leftHandPosition = new Vector3(0, -0.15f, -0.3f);
                    break;
            }
        }
    }

    private void SetAnimationTriggerForWeaponType()
    {
        switch (weaponType)
        {
            case WeaponType.Sword:
                animationTrigger = "Attack";
                animationBoolParam = "isMeleeing";
                break;
            case WeaponType.Rifle:
                animationTrigger = "Shoot";
                animationBoolParam = "isShooting";
                break;
            case WeaponType.Explosive:
                animationTrigger = "Throw";
                animationBoolParam = "isThrowing";
                break;
            default:
                animationTrigger = "Attack";
                animationBoolParam = "";
                break;
        }
    }

    private void Update()
    {
        if (isEquipped)
        {
            UpdateTransform();
        }
    }

    private void UpdateTransform()
    {
        transform.localPosition = weaponPositionOffset;
        transform.localRotation = Quaternion.Euler(weaponRotationOffset);
    }

    public void UseWeapon()
    {
        bool animationStarted = PlayWeaponAnimation();

        if (animationStarted)
        {
            // Implement weapon specific functionality here
            switch (weaponType)
            {
                case WeaponType.Sword:
                    // Melee attack logic
                    break;
                case WeaponType.Rifle:
                    // Shooting logic  
                    break;
                case WeaponType.Explosive:
                    // Throwing explosive logic
                    break;
            }
        }
    }

    private bool PlayWeaponAnimation()
    {
        if (animationController != null)
        {
            if (!string.IsNullOrEmpty(animationTrigger))
            {
                bool success = animationController.TriggerAnimation(animationTrigger);
                if (success) return true;
            }

            if (!string.IsNullOrEmpty(animationBoolParam))
            {
                animationController.PlayAnimation(animationBoolParam, 1.5f);
                return true;
            }
        }

        if (playerAnimator != null)
        {
            if (!string.IsNullOrEmpty(animationTrigger) && CheckAnimatorHasParameter(animationTrigger))
            {
                playerAnimator.SetTrigger(animationTrigger);
                return true;
            }

            if (!string.IsNullOrEmpty(animationBoolParam) && CheckAnimatorHasParameter(animationBoolParam))
            {
                playerAnimator.SetBool(animationBoolParam, true);
                return true;
            }
        }

        return false;
    }

    private bool CheckAnimatorHasParameter(string paramName)
    {
        if (playerAnimator == null || string.IsNullOrEmpty(paramName)) return false;

        foreach (AnimatorControllerParameter param in playerAnimator.parameters)
        {
            if (param.name == paramName) return true;
        }

        return false;
    }

    public void OnUnequipped()
    {
        isEquipped = false;

        if (usesTwoHands && leftHandIK != null)
        {
            leftHandIK.ikActive = false;
            leftHandIK.leftHandObj = null;
        }
    }
}