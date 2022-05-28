using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;


    public static PauseMenu instance;
    [HideInInspector] public int stepsSincePauseEnded;

    private void Awake()
    {
        instance = this;
        stepsSincePauseEnded = 0;
        pauseMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) pauseMenu.SetActive(!pauseMenu.activeInHierarchy);
        Time.timeScale = pauseMenu.activeInHierarchy ? 0 : 1;
        Cursor.lockState = pauseMenu.activeInHierarchy ? CursorLockMode.None : CursorLockMode.Locked;
        stepsSincePauseEnded = pauseMenu.activeInHierarchy ? 0 : stepsSincePauseEnded < 20 ? stepsSincePauseEnded + 1 : 20;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

        Application.Quit();
    }
}
