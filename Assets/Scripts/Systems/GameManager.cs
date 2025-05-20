using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject uiPanel;
    [SerializeField] private GameObject loseUI;
    [SerializeField] private GameObject winUI;
    [SerializeField] private GameObject menuUI;

    private bool gameEnded = false;
    private bool menuActive = false;

    void Awake()
    {
        if (Instance != null) Destroy(gameObject);

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !gameEnded) SetShowMenu();
    }

    public void ResumeGame() => SetShowMenu();

    public void SetShowMenu()
    {
        menuActive = !menuActive;
        menuUI.SetActive(menuActive);
        Time.timeScale = menuActive ? 0 : 1;
        if (menuActive) ShowCursor();
    }

    public void TriggerLose()
    {
        if (gameEnded) return;

        gameEnded = true;

        // TODO: Play sound, show UI, slow time, etc.
        GameOverSequence();
        loseUI.SetActive(true);
    }

    public void TriggerWin()
    {
        if (gameEnded) return;

        gameEnded = true;

        // TODO: Show win UI, play animation, etc.
        GameOverSequence();
        winUI.SetActive(true);

        // Invoke(nameof(ReloadLevel), 2f); // load next level
    }

    void GameOverSequence()
    {
        ShowCursor();
        uiPanel.SetActive(true);
        PlayerController.Instance.enabled = false;
        Time.timeScale = 0.05f;
    }

    void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ReloadLevel()
    {
        // reload fresh level, removing all singleton scripts as well
        Instance = null;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    public void Quit() => Application.Quit();
}