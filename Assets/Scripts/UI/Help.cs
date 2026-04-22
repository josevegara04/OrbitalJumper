using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Help : MonoBehaviour
{
    public TextMeshProUGUI clickText;
    public bool help = false;

    void Start()
    {
        clickText.gameObject.SetActive(false);
    }
    public void ToogleHelp()
    {
        if (!help)
        {
            help = true;
            clickText.gameObject.SetActive(true);
        }
        else
        {
            help = false;
            clickText.gameObject.SetActive(false);
        }
    }
}