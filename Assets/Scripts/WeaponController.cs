using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class WeaponController : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public LayerMask hitMask;
    public Transform viewModelHolder;

    [Header("Weapons")]
    public WeaponData[] weapons;
    public int startingWeaponIndex = 0;

    [Header("HUD")]
    public TMP_Text weaponText;
    public TMP_Text ammoText;

    [Header("Effects")]
    public GameObject bulletHolePrefab;
    public float bulletHoleLifetime = 20f;
    public float bulletHoleOffset = 0.002f;

    [Header("Audio")]
    public AudioSource weaponAudioSource;
    public AudioSource fireLoopAudioSource;

    [Header("Aim Zoom")]
    public float normalFOV = 60f;
    public float aimingFOV = 50f;
    public float aimZoomSpeed = 10f;

    [Header("Viewmodel Bob")]
    public float walkBobSpeed = 8f;
    public float walkBobAmount = 0.025f;

    public float runBobSpeed = 12f;
    public float runBobAmount = 0.045f;

    public float bobSideAmount = 0.015f;
    public float bobRotationAmount = 1.5f;

    public float aimBobMultiplier = 0.35f;
    public float bobSmoothing = 10f;

    public bool IsAiming { get; private set; }

    public float CurrentSpread
    {
        get
        {
            if (currentWeapon == null)
                return 0f;

            return IsAiming ? currentWeapon.adsSpread : currentWeapon.hipSpread;
        }
    }

    public event System.Action OnShot;

    private PlayerInput playerInput;

    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction fireAction;
    private InputAction aimAction;
    private InputAction reloadAction;

    private InputAction weapon1Action;
    private InputAction weapon2Action;
    private InputAction weapon3Action;
    private InputAction weapon4Action;
    private InputAction weapon5Action;

    private WeaponData currentWeapon;
    private int currentWeaponIndex;

    private int[] currentAmmoPerWeapon;
    private int[] reserveAmmoPerWeapon;

    private GameObject currentViewModel;
    private WeaponViewModel currentViewModelInfo;

    private Vector3 baseViewModelLocalPosition;
    private Quaternion baseViewModelLocalRotation;

    private Vector3 currentRecoilPosition;
    private Vector3 targetRecoilPosition;

    private Vector3 currentRecoilRotation;
    private Vector3 targetRecoilRotation;

    private float bobTimer;

    private Vector3 currentBobPosition;
    private Vector3 targetBobPosition;

    private Vector3 currentBobRotation;
    private Vector3 targetBobRotation;

    private float nextFireTime;
    private bool isReloading;

    private bool triggerHeld;
    private float fireLoopTargetVolume;

    private void Awake()
    {
        playerInput = GetComponentInParent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError("WeaponController could not find PlayerInput in parent.");
            return;
        }

        InputActionMap playerMap = playerInput.actions.FindActionMap("Player", true);

        moveAction = playerMap.FindAction("Move", true);
        sprintAction = playerMap.FindAction("Sprint", false);

        fireAction = playerMap.FindAction("Fire", true);
        aimAction = playerMap.FindAction("Aim", true);
        reloadAction = playerMap.FindAction("Reload", true);

        weapon1Action = playerMap.FindAction("Weapon1", false);
        weapon2Action = playerMap.FindAction("Weapon2", false);
        weapon3Action = playerMap.FindAction("Weapon3", false);
        weapon4Action = playerMap.FindAction("Weapon4", false);
        weapon5Action = playerMap.FindAction("Weapon5", false);
    }

    private void Start()
    {
        if (playerCamera != null)
        {
            normalFOV = playerCamera.fieldOfView;
        }

        InitializeWeapons();
        EquipWeapon(startingWeaponIndex);
    }

    private void Update()
    {
        HandleWeaponSwitching();

        if (currentWeapon == null)
            return;

        triggerHeld = fireAction.ReadValue<float>() > 0.5f;

        IsAiming = aimAction.ReadValue<float>() > 0.5f && !isReloading;

        if (currentViewModelInfo != null)
        {
            bool hasAmmo = currentAmmoPerWeapon[currentWeaponIndex] > 0;
            currentViewModelInfo.SetTriggerHeld(triggerHeld && !isReloading && hasAmmo);
        }

        HandleAimZoom();
        HandleViewModelBob();
        HandleViewModelRecoil();
        HandleLoopingFireAudio();

        if (isReloading)
            return;

        if (currentWeapon.automatic)
        {
            if (triggerHeld)
            {
                TryShoot(IsAiming);
            }
        }
        else
        {
            if (fireAction.WasPressedThisFrame())
            {
                TryShoot(IsAiming);
            }
        }

        if (reloadAction.WasPressedThisFrame())
        {
            StartCoroutine(Reload());
        }

        UpdateHUD();
    }

    private void InitializeWeapons()
    {
        if (weapons == null || weapons.Length == 0)
        {
            Debug.LogError("No weapons assigned to WeaponController.");
            return;
        }

        currentAmmoPerWeapon = new int[weapons.Length];
        reserveAmmoPerWeapon = new int[weapons.Length];

        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] == null)
            {
                Debug.LogWarning("Weapon slot " + i + " is empty.");
                continue;
            }

            currentAmmoPerWeapon[i] = weapons[i].magazineSize;
            reserveAmmoPerWeapon[i] = weapons[i].startingReserveAmmo;
        }
    }

    private void HandleWeaponSwitching()
    {
        if (isReloading)
            return;

        if (weapon1Action != null && weapon1Action.WasPressedThisFrame())
        {
            EquipWeapon(0);
        }

        if (weapon2Action != null && weapon2Action.WasPressedThisFrame())
        {
            EquipWeapon(1);
        }

        if (weapon3Action != null && weapon3Action.WasPressedThisFrame())
        {
            EquipWeapon(2);
        }

        if (weapon4Action != null && weapon4Action.WasPressedThisFrame())
        {
            EquipWeapon(3);
        }

        if (weapon5Action != null && weapon5Action.WasPressedThisFrame())
        {
            EquipWeapon(4);
        }
    }

    private void EquipWeapon(int weaponIndex)
    {
        if (weapons == null || weapons.Length == 0)
            return;

        if (weaponIndex < 0 || weaponIndex >= weapons.Length)
        {
            Debug.LogWarning("Weapon index " + weaponIndex + " is outside the weapons array.");
            return;
        }

        if (weapons[weaponIndex] == null)
        {
            Debug.LogWarning("Weapon slot " + weaponIndex + " is empty.");
            return;
        }

        StopFireLoopImmediate();

        currentWeaponIndex = weaponIndex;
        currentWeapon = weapons[currentWeaponIndex];

        nextFireTime = 0f;
        IsAiming = false;
        triggerHeld = false;

        UpdateViewModel();
        UpdateHUD();
    }

    private void UpdateViewModel()
    {
        if (viewModelHolder == null)
        {
            Debug.LogWarning("No ViewModelHolder assigned on WeaponController.");
            return;
        }

        for (int i = viewModelHolder.childCount - 1; i >= 0; i--)
        {
            Destroy(viewModelHolder.GetChild(i).gameObject);
        }

        currentViewModel = null;
        currentViewModelInfo = null;

        currentRecoilPosition = Vector3.zero;
        targetRecoilPosition = Vector3.zero;
        currentRecoilRotation = Vector3.zero;
        targetRecoilRotation = Vector3.zero;

        bobTimer = 0f;
        currentBobPosition = Vector3.zero;
        targetBobPosition = Vector3.zero;
        currentBobRotation = Vector3.zero;
        targetBobRotation = Vector3.zero;

        if (currentWeapon == null || currentWeapon.viewModelPrefab == null)
            return;

        currentViewModel = Instantiate(
            currentWeapon.viewModelPrefab,
            viewModelHolder,
            false
        );

        currentViewModelInfo = currentViewModel.GetComponentInChildren<WeaponViewModel>();

        baseViewModelLocalPosition = currentViewModel.transform.localPosition;
        baseViewModelLocalRotation = currentViewModel.transform.localRotation;
    }

    private void HandleAimZoom()
    {
        if (playerCamera == null)
            return;

        float targetFOV = IsAiming ? aimingFOV : normalFOV;

        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFOV,
            Time.deltaTime * aimZoomSpeed
        );
    }

    private void HandleViewModelBob()
    {
        if (currentViewModel == null || moveAction == null)
            return;

        Vector2 moveInput = moveAction.ReadValue<Vector2>();

        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        bool isSprinting = sprintAction != null && sprintAction.ReadValue<float>() > 0.5f;

        if (isMoving)
        {
            float selectedBobSpeed = isSprinting ? runBobSpeed : walkBobSpeed;
            float selectedBobAmount = isSprinting ? runBobAmount : walkBobAmount;

            float aimMultiplier = IsAiming ? aimBobMultiplier : 1f;

            selectedBobAmount *= aimMultiplier;

            bobTimer += Time.deltaTime * selectedBobSpeed;

            float verticalBob = Mathf.Sin(bobTimer) * selectedBobAmount;
            float horizontalBob = Mathf.Cos(bobTimer * 0.5f) * bobSideAmount * aimMultiplier;

            targetBobPosition = new Vector3(
                horizontalBob,
                verticalBob,
                0f
            );

            targetBobRotation = new Vector3(
                Mathf.Sin(bobTimer) * bobRotationAmount,
                Mathf.Cos(bobTimer * 0.5f) * bobRotationAmount,
                Mathf.Cos(bobTimer) * bobRotationAmount
            ) * aimMultiplier;
        }
        else
        {
            bobTimer = 0f;
            targetBobPosition = Vector3.zero;
            targetBobRotation = Vector3.zero;
        }

        currentBobPosition = Vector3.Lerp(
            currentBobPosition,
            targetBobPosition,
            Time.deltaTime * bobSmoothing
        );

        currentBobRotation = Vector3.Lerp(
            currentBobRotation,
            targetBobRotation,
            Time.deltaTime * bobSmoothing
        );
    }

    private void HandleViewModelRecoil()
    {
        if (currentWeapon == null || currentViewModel == null)
            return;

        targetRecoilPosition = Vector3.Lerp(
            targetRecoilPosition,
            Vector3.zero,
            Time.deltaTime * currentWeapon.recoilReturnSpeed
        );

        targetRecoilRotation = Vector3.Lerp(
            targetRecoilRotation,
            Vector3.zero,
            Time.deltaTime * currentWeapon.recoilReturnSpeed
        );

        currentRecoilPosition = Vector3.Lerp(
            currentRecoilPosition,
            targetRecoilPosition,
            Time.deltaTime * currentWeapon.recoilSnappiness
        );

        currentRecoilRotation = Vector3.Lerp(
            currentRecoilRotation,
            targetRecoilRotation,
            Time.deltaTime * currentWeapon.recoilSnappiness
        );

        currentViewModel.transform.localPosition =
            baseViewModelLocalPosition +
            currentBobPosition +
            currentRecoilPosition;

        currentViewModel.transform.localRotation =
            baseViewModelLocalRotation *
            Quaternion.Euler(currentBobRotation) *
            Quaternion.Euler(currentRecoilRotation);
    }

    private void TryShoot(bool isAiming)
    {
        if (Time.time < nextFireTime)
            return;

        if (currentAmmoPerWeapon[currentWeaponIndex] <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        nextFireTime = Time.time + currentWeapon.fireRate;

        currentAmmoPerWeapon[currentWeaponIndex]--;

        OnShot?.Invoke();

        PlayShotSound();
        ApplyVisualRecoil();
        PlayMuzzleFlash();

        for (int i = 0; i < currentWeapon.pelletsPerShot; i++)
        {
            ShootRay(isAiming);
        }

        UpdateHUD();
    }

    private void ApplyVisualRecoil()
    {
        if (currentWeapon == null || currentViewModel == null)
            return;

        targetRecoilPosition += currentWeapon.recoilPositionKick;

        float randomYaw = Random.Range(
            -currentWeapon.recoilRandomness,
            currentWeapon.recoilRandomness
        );

        float randomRoll = Random.Range(
            -currentWeapon.recoilRandomness,
            currentWeapon.recoilRandomness
        );

        targetRecoilRotation += new Vector3(
            currentWeapon.recoilRotationKick.x,
            currentWeapon.recoilRotationKick.y + randomYaw,
            currentWeapon.recoilRotationKick.z + randomRoll
        );

        targetRecoilPosition = Vector3.ClampMagnitude(targetRecoilPosition, 0.3f);
        targetRecoilRotation = Vector3.ClampMagnitude(targetRecoilRotation, 20f);
    }

    private void PlayMuzzleFlash()
    {
        if (currentWeapon == null)
            return;

        if (currentWeapon.muzzleFlashPrefab == null)
            return;

        if (currentViewModelInfo == null || currentViewModelInfo.muzzlePoint == null)
            return;

        GameObject flash = Instantiate(
            currentWeapon.muzzleFlashPrefab,
            currentViewModelInfo.muzzlePoint.position,
            currentViewModelInfo.muzzlePoint.rotation,
            currentViewModelInfo.muzzlePoint
        );

        flash.transform.localPosition = Vector3.zero;
        flash.transform.localRotation = Quaternion.identity;

        SetWorldScale(
            flash.transform,
            Vector3.one * currentWeapon.muzzleFlashScale
        );

        Destroy(flash, currentWeapon.muzzleFlashLifetime);
    }

    private void SetWorldScale(Transform target, Vector3 worldScale)
    {
        if (target.parent == null)
        {
            target.localScale = worldScale;
            return;
        }

        Vector3 parentScale = target.parent.lossyScale;

        target.localScale = new Vector3(
            parentScale.x != 0f ? worldScale.x / parentScale.x : worldScale.x,
            parentScale.y != 0f ? worldScale.y / parentScale.y : worldScale.y,
            parentScale.z != 0f ? worldScale.z / parentScale.z : worldScale.z
        );
    }

    private void ShootRay(bool isAiming)
    {
        if (playerCamera == null)
            return;

        float spread = isAiming ? currentWeapon.adsSpread : currentWeapon.hipSpread;

        Vector3 direction = GetSpreadDirection(playerCamera.transform.forward, spread);

        int shootMask = hitMask & ~LayerMask.GetMask("Player");
        if (Physics.Raycast(
            playerCamera.transform.position,
            direction,
            out RaycastHit hit,
            currentWeapon.range,
            shootMask
        ))

 
        {
            IDamageable damageableTarget = hit.collider.GetComponentInParent<IDamageable>();

            if (damageableTarget != null)
            {
                int damageAmount = Mathf.RoundToInt(currentWeapon.damage);
                damageableTarget.TakeDamage(damageAmount);

                // Do not spawn bullet holes on zombies/enemies.
                return;
            }

            CreateBulletHole(hit);

            Health targetHealth = hit.collider.GetComponentInParent<Health>();

            if (targetHealth != null)
            {
                targetHealth.TakeDamage(currentWeapon.damage);
            }
        }
    }

    private void CreateBulletHole(RaycastHit hit)
    {
        if (bulletHolePrefab == null)
            return;

        Vector3 spawnPosition = hit.point + hit.normal * bulletHoleOffset;

        Quaternion spawnRotation = Quaternion.LookRotation(-hit.normal);

        GameObject bulletHole = Instantiate(
            bulletHolePrefab,
            spawnPosition,
            spawnRotation
        );

        bulletHole.transform.Rotate(0f, 0f, Random.Range(0f, 360f));
        bulletHole.transform.SetParent(hit.collider.transform, true);

        Destroy(bulletHole, bulletHoleLifetime);
    }

    private Vector3 GetSpreadDirection(Vector3 forwardDirection, float spreadAngle)
    {
        float randomX = Random.Range(-spreadAngle, spreadAngle);
        float randomY = Random.Range(-spreadAngle, spreadAngle);

        Quaternion spreadRotation = Quaternion.Euler(randomY, randomX, 0f);

        return spreadRotation * forwardDirection;
    }

    private void PlayShotSound()
    {
        if (currentWeapon == null)
            return;

        if (currentWeapon.useLoopingFireSound)
            return;

        if (weaponAudioSource == null || currentWeapon.shotSound == null)
            return;

        weaponAudioSource.pitch = Random.Range(
            currentWeapon.shotPitchRange.x,
            currentWeapon.shotPitchRange.y
        );

        weaponAudioSource.PlayOneShot(
            currentWeapon.shotSound,
            currentWeapon.shotVolume
        );
    }

    private void HandleLoopingFireAudio()
    {
        if (fireLoopAudioSource == null || currentWeapon == null)
            return;

        bool shouldLoop =
            currentWeapon.useLoopingFireSound &&
            triggerHeld &&
            !isReloading &&
            currentAmmoPerWeapon[currentWeaponIndex] > 0;

        if (shouldLoop)
        {
            if (currentWeapon.fireLoopSound == null)
            {
                Debug.LogWarning("No fire loop sound assigned on " + currentWeapon.weaponName);
                return;
            }

            if (fireLoopAudioSource.clip != currentWeapon.fireLoopSound)
            {
                fireLoopAudioSource.clip = currentWeapon.fireLoopSound;
            }

            fireLoopAudioSource.loop = true;

            if (!fireLoopAudioSource.isPlaying)
            {
                fireLoopAudioSource.volume = 0f;
                fireLoopAudioSource.Play();
                Debug.Log("Started fire loop for " + currentWeapon.weaponName);
            }

            fireLoopTargetVolume = currentWeapon.fireLoopVolume;
        }
        else
        {
            fireLoopTargetVolume = 0f;
        }

        fireLoopAudioSource.volume = Mathf.MoveTowards(
            fireLoopAudioSource.volume,
            fireLoopTargetVolume,
            currentWeapon.fireLoopFadeSpeed * Time.deltaTime
        );

        if (fireLoopAudioSource.isPlaying &&
            fireLoopAudioSource.volume <= 0.001f &&
            fireLoopTargetVolume == 0f)
        {
            fireLoopAudioSource.Stop();
        }
    }

    private void StopFireLoopImmediate()
    {
        fireLoopTargetVolume = 0f;

        if (fireLoopAudioSource == null)
            return;

        fireLoopAudioSource.Stop();
        fireLoopAudioSource.volume = 0f;
        fireLoopAudioSource.clip = null;
    }

    private void PlayReloadSound()
    {
        if (weaponAudioSource == null || currentWeapon == null || currentWeapon.reloadSound == null)
            return;

        weaponAudioSource.pitch = 1f;

        weaponAudioSource.PlayOneShot(
            currentWeapon.reloadSound,
            currentWeapon.reloadVolume
        );
    }

    private IEnumerator Reload()
    {
        if (isReloading)
            yield break;

        if (currentAmmoPerWeapon[currentWeaponIndex] >= currentWeapon.magazineSize)
            yield break;

        if (reserveAmmoPerWeapon[currentWeaponIndex] <= 0)
            yield break;

        isReloading = true;
        IsAiming = false;

        if (currentViewModelInfo != null)
        {
            currentViewModelInfo.SetTriggerHeld(false);
        }

        UpdateHUD("Reloading...");
        PlayReloadSound();

        yield return new WaitForSeconds(currentWeapon.reloadTime);

        int missingAmmo = currentWeapon.magazineSize - currentAmmoPerWeapon[currentWeaponIndex];
        int ammoToLoad = Mathf.Min(missingAmmo, reserveAmmoPerWeapon[currentWeaponIndex]);

        currentAmmoPerWeapon[currentWeaponIndex] += ammoToLoad;
        reserveAmmoPerWeapon[currentWeaponIndex] -= ammoToLoad;

        isReloading = false;

        UpdateHUD();
    }

    private void UpdateHUD(string ammoOverride = "")
    {
        if (weaponText != null && currentWeapon != null)
        {
            weaponText.text = currentWeapon.weaponName;
        }

        if (ammoText != null)
        {
            if (!string.IsNullOrEmpty(ammoOverride))
            {
                ammoText.text = ammoOverride;
            }
            else if (currentWeapon != null)
            {
                ammoText.text =
                    currentAmmoPerWeapon[currentWeaponIndex] +
                    " / " +
                    reserveAmmoPerWeapon[currentWeaponIndex];
            }
        }
    }
}