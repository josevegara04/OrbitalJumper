using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float lifetime = 1.5f;

    private TextMeshProUGUI text;
    private Color startColor;
    private float time;
    private Transform cam;

    void Start()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            startColor = text.color;

        var mainCam = Camera.main;
        if (mainCam != null)
            cam = mainCam.transform;

        // Destruir cuando termine
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (text == null)
        {
            Destroy(gameObject);
            return;
        }

        time += Time.deltaTime;

        if (!cam)
        {
            var mainCam = Camera.main;
            if (mainCam)
                cam = mainCam.transform;
            else
            {
                Debug.LogWarning("No main camera found!");
                return; // ✅ Solo retorna si realmente no hay cámara
            }
        }

        // Movimiento hacia arriba
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(transform.position - cam.position);

        // Fade out
        float t = time / lifetime;
        float alpha = Mathf.SmoothStep(1f, 0f, t);
        Color c = startColor;
        c.a = alpha;
        text.color = c;
    }
}