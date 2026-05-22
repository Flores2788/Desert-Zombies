using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [Header("References")]
    public WeaponController weaponController;

    public RectTransform top;
    public RectTransform bottom;
    public RectTransform left;
    public RectTransform right;

    [Header("Crosshair Settings")]
    public float baseGap = 8f;
    public float spreadMultiplier = 6f;

    [Header("Shooting Feedback")]
    public float shotExpansion = 10f;
    public float shotExpansionReturnSpeed = 25f;

    private float currentShotExpansion;

    private void OnEnable()
    {
        if (weaponController != null)
        {
            weaponController.OnShot += HandleShot;
        }
    }

    private void OnDisable()
    {
        if (weaponController != null)
        {
            weaponController.OnShot -= HandleShot;
        }
    }

    private void Update()
    {
        if (weaponController == null)
            return;

        float targetGap = baseGap + weaponController.CurrentSpread * spreadMultiplier + currentShotExpansion;

        top.anchoredPosition = new Vector2(0f, targetGap);
        bottom.anchoredPosition = new Vector2(0f, -targetGap);
        left.anchoredPosition = new Vector2(-targetGap, 0f);
        right.anchoredPosition = new Vector2(targetGap, 0f);

        currentShotExpansion = Mathf.MoveTowards(
            currentShotExpansion,
            0f,
            shotExpansionReturnSpeed * Time.deltaTime
        );
    }

    private void HandleShot()
    {
        currentShotExpansion = shotExpansion;
    }
}