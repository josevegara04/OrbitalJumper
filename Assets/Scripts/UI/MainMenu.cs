using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene"); // tu escena del juego
    }

    public void QuitGame()
    {
        Application.Quit();
        UnityEngine.Debug.Log("Quit");
    }
}