using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public static Menu instance;
    [Header("Menu Elements")]
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject optionsPanel;
    [SerializeField] GameObject gamePanel;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] GameObject winPanel;
    [SerializeField] TMP_Text volumeText;
    [SerializeField] Slider volumeSlider;
    [SerializeField] Image menuBackground;
    [SerializeField] List<Color> colorList;

    [SerializeField] GameManager gameManager;
    [SerializeField] AudioManager audioManager;

    private bool isPaused;
    private float lerpDuration = 5f;
    private int currentColorIndex = 0;
    private Coroutine changeColors;

    private void Awake()
    {
        instance = this;
        changeColors = StartCoroutine(ChangeColorCoroutine());
        UpdateAudio();
        gameManager.gameObject.SetActive(true);
    }
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

    IEnumerator ChangeColorCoroutine()
    {
        while (true)
        {
            Color startColor = menuBackground.color;
            currentColorIndex = (currentColorIndex + 1) % colorList.Count;
            Color targetColor = colorList[currentColorIndex];

            float timer = 0f;
            while (timer < lerpDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / lerpDuration;
                menuBackground.color = Color.Lerp(startColor, targetColor, progress);
                yield return null;
            }
            menuBackground.color = targetColor;
        }
    }
    public void PlayButton()
    {
        mainMenuPanel.SetActive(false);
        gamePanel.SetActive(true);
        gameManager.ResetGame();
        gameManager.InitializeGame();
        StopCoroutine(changeColors);
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

    public void Defeated()
    {
        gameOverPanel.SetActive(true);
        audioManager.backgroundMusic.Stop();
        audioManager.backgroundMusic.PlayOneShot(audioManager.defeated);
        
    }
    public void Win()
    {
        winPanel.SetActive(true);
        audioManager.backgroundMusic.Stop();
        audioManager.backgroundMusic.PlayOneShot(audioManager.won);
        
    }

    public void BackToMainMenuQuit()
    {
        gameOverPanel.SetActive(false);
        gamePanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        audioManager.backgroundMusic.Play();
    }
    public void BackToMainMenuWin()
    {
        winPanel.SetActive(false);
        gamePanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        audioManager.backgroundMusic.Play();
    }

    public void UpdateAudio()
    {
        volumeText.text = $"Volume : {(int)(volumeSlider.value * 100)}%";
        audioManager.backgroundMusic.volume = volumeSlider.value;
    }
}
