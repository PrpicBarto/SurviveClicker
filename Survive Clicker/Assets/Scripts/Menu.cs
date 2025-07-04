using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [Header("Menu Elements")]
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject optionsPanel;
    [SerializeField] GameObject gamePanel;
    [SerializeField] TMP_Text volumeText;
    [SerializeField] Slider volumeSlider;

    [SerializeField] GameManager gameManager;
    private bool isPaused;

    private void Update()
    {
        PauseGame();
    }
    private void PauseGame()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && isPaused == false&& mainMenuPanel.gameObject.activeInHierarchy == false && optionsPanel.gameObject.activeInHierarchy == false)
        {
            isPaused = !isPaused;
            Time.timeScale = 0;
            Debug.Log($"Paused");
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && isPaused == true && mainMenuPanel.gameObject.activeInHierarchy == false && optionsPanel.gameObject.activeInHierarchy == false)
        {
            isPaused = !isPaused;
            Time.timeScale = 1;
            Debug.Log($"Unpaused");
        }
    }
    public void PlayButton()
    {
        mainMenuPanel.SetActive(false);
        gamePanel.SetActive(true);
        gameManager.InitializeGame();
    }
    public void MainMenuButton()
    {
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
    public void OptionsButton()
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }
    public void QuitButton()
    {
        Application.Quit();
    }

    public void UpdateText()
    {
        volumeText.text = $"Volume : {(int)(volumeSlider.value * 100)}%";
    }
}
