using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Button playBtn;
    public Button exitBtn;
    public string firstLevelSceneName = "SampleScene"; // Thay thế bằng tên scene gameplay của bạn
    private void Start()
    {
        playBtn.onClick.AddListener(PlayGame);
        exitBtn.onClick.AddListener(ExitGame);
    }
    private void PlayGame()
    {
        SceneManager.LoadScene(firstLevelSceneName);
    }
    private void ExitGame()
    {
        Application.Quit();
    }
}
