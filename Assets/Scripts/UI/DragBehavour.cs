using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DragBehavour : MonoBehaviour
{
    public static DragBehavour Instance;
    public TextMeshProUGUI dragText;
    public bool firstLandingDone = false;

    void Awake()
    {
        Instance = this;
    }
    public void HideText()
    {
        Debug.Log("HIDE TEXT llamado");
        dragText.gameObject.SetActive(false);
    }
}
