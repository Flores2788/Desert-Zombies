using UnityEngine;

public class LadderLiftZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        FPSController player = other.GetComponentInParent<FPSController>();

        if (player != null)
        {
            player.SetInLadderLiftZone(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        FPSController player = other.GetComponentInParent<FPSController>();

        if (player != null)
        {
            player.SetInLadderLiftZone(false);
        }
    }
}
