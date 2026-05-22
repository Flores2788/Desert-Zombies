using UnityEngine;

public class VictoryScreen : MonoBehaviour
{
    public GameObject victoryUI;

    private void Start()
    {
        if (victoryUI != null)
            victoryUI.SetActive(false);
    }

    public void ShowVictory()
    {
        if (victoryUI != null)
            victoryUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }
}
