using UnityEngine;

public class WeaponViewModel : MonoBehaviour
{
    [Header("Muzzle")]
    public Transform muzzlePoint;

    [Header("Optional Rotating Barrel")]
    public Transform rotatingBarrel;
    public Vector3 barrelSpinAxis = Vector3.forward;
    public float barrelMaxSpinSpeed = 1200f;
    public float barrelSpinUpSpeed = 2500f;
    public float barrelSpinDownSpeed = 1800f;

    private bool wantsToSpin;
    private float currentSpinSpeed;

    public void SetTriggerHeld(bool isHeld)
    {
        wantsToSpin = isHeld;
    }

    private void Update()
    {
        HandleBarrelSpin();
    }

    private void HandleBarrelSpin()
    {
        if (rotatingBarrel == null)
            return;

        float targetSpeed = wantsToSpin ? barrelMaxSpinSpeed : 0f;
        float acceleration = wantsToSpin ? barrelSpinUpSpeed : barrelSpinDownSpeed;

        currentSpinSpeed = Mathf.MoveTowards(
            currentSpinSpeed,
            targetSpeed,
            acceleration * Time.deltaTime
        );

        rotatingBarrel.Rotate(
            barrelSpinAxis.normalized,
            currentSpinSpeed * Time.deltaTime,
            Space.Self
        );
    }
}