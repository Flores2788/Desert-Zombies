using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "FPS/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Info")]
    public string weaponName = "Pistol";

    [Header("Damage")]
    public float damage = 25f;
    public float range = 100f;

    [Header("Fire")]
    public bool automatic = false;
    public float fireRate = 0.25f;
    public int pelletsPerShot = 1;

    [Header("Ammo")]
    public int magazineSize = 12;
    public int startingReserveAmmo = 48;
    public float reloadTime = 1.5f;

    [Header("Spread")]
    public float hipSpread = 2f;
    public float adsSpread = 0.2f;

    [Header("Audio")]
    public AudioClip shotSound;
    public float shotVolume = 1f;
    public Vector2 shotPitchRange = new Vector2(0.95f, 1.05f);

    public AudioClip reloadSound;
    public float reloadVolume = 1f;

    [Header("Looped Fire Audio")]
    public bool useLoopingFireSound = false;
    public AudioClip fireLoopSound;
    public float fireLoopVolume = 1f;
    public float fireLoopFadeSpeed = 12f;

    [Header("Viewmodel")]
    public GameObject viewModelPrefab;

    [Header("Visual Recoil")]
    public Vector3 recoilPositionKick = new Vector3(0f, 0f, -0.08f);
    public Vector3 recoilRotationKick = new Vector3(-4f, 1f, 0f);
    public float recoilSnappiness = 20f;
    public float recoilReturnSpeed = 10f;
    public float recoilRandomness = 1f;

    [Header("Muzzle Flash")]
    public GameObject muzzleFlashPrefab;
    public float muzzleFlashLifetime = 0.08f;
    public float muzzleFlashScale = 1f;

}