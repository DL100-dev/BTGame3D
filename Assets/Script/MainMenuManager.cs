using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Button playBtn;
    public Button exitBtn;
    public string firstLevelSceneName = "SampleScene"; 
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
