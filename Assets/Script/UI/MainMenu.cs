using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("The bunker");
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
