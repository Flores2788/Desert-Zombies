using UnityEngine;
using TMPro;

public class WaveUI : MonoBehaviour
{
    public TextMeshProUGUI waveText;

    void Update()
    {
        if (EnemySpawner.Instance != null && waveText != null)
        {
            waveText.text = "Wave " + EnemySpawner.Instance.GetCurrentWave();
        }
    }

}