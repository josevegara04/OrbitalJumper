using UnityEngine;

using System.Collections;

public class LoseAnimation : MonoBehaviour

{

    public float duration = 0.5f;

    public IEnumerator PlayLoseAnimation()
    {
        Debug.Log("loseAnimation");

        float time = 0f;

        Vector3 startScale = transform.localScale;
        Vector3 targetScale = startScale * 4f;

        Vector3 originalPosition = transform.position;

        while (time < duration)
        {
            float t = time / duration;

            // easing (más impactante)
            float ease = Mathf.Sin(t * Mathf.PI * 0.5f);

            // SCALE
            transform.localScale = Vector3.Lerp(startScale, targetScale, ease);

            // SHAKE
            float shakeStrength = 0.2f * (1f - t);
            Vector3 shake = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            ) * shakeStrength;

            transform.position = originalPosition + shake;

            // ROTATION
            transform.Rotate(0f, 300f * Time.deltaTime, 0f);

            time += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Termino");

        // restaurar posición por seguridad
        transform.position = originalPosition;
    }

}