using TMPro;
using UnityEngine;

public enum GameDifficulty { Easy, Normal, Hard };

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    public GameDifficulty currentDifficulty;

    public DifficultySettings easySettings;
    public DifficultySettings normalSettings;
    public DifficultySettings hardSettings;

    public DifficultySettings CurrentSettings { get; private set; }

    public TMP_Dropdown difficultyDropdown;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Loads the current difficulty from player prefs, defaulting to the value of currentDifficulty if not found.
    /// Also sets up a listener for the dropdown menu to set the difficulty when changed.
    /// </summary>
    void Start()
    {
        // load from player prefs or default to normal
        int saved = PlayerPrefs.GetInt(nameof(GameDifficulty), (int)currentDifficulty);  // 0=Easy, 1=Normal, 2=Hard
        currentDifficulty = (GameDifficulty)saved;

        difficultyDropdown.value = saved;
        difficultyDropdown.onValueChanged.AddListener(SetDifficulty);
    }

    public void SetDifficulty(int value)
    {
        currentDifficulty = (GameDifficulty)value;
        PlayerPrefs.SetInt(nameof(GameDifficulty), value);
        PlayerPrefs.Save();

        ApplySettings();

        Debug.Log("Difficulty set to " + currentDifficulty);
    }

    /// <summary>
    /// Applies the appropriate difficulty settings based on the current difficulty level.
    /// Updates the CurrentSettings field to match the selected GameDifficulty.
    /// </summary>
    private void ApplySettings()
    {
        switch (currentDifficulty)
        {
            case GameDifficulty.Easy: CurrentSettings = easySettings; break;
            case GameDifficulty.Normal: CurrentSettings = normalSettings; break;
            case GameDifficulty.Hard: CurrentSettings = hardSettings; break;
        }
    }
}
