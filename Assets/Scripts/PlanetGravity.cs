using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class PlanetGravity : MonoBehaviour
{
    public float gravityStrength = 20f;
    public float orbitDistance = 4f;
    public Transform cameraTransform;
    public Transform nextPlanet;
    public float cameraDistanceFactor = 0.5f;
    bool cameraMoved = false;
    private float orbitSpeed = 7f;

    void Awake()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Satellite")) return;
        SatelliteController sc = other.GetComponent<SatelliteController>();
        sc.isOrbiting = true;
        ScoreManager.Instance.AddScore(100);

        PlanetManager.Instance.SpawnNextPlanet();
        sc.nextPlanet = nextPlanet;

        if (!cameraMoved)
        {
            StartCoroutine(TriggerCameraMoveWithDelay(0f, transform));
            cameraMoved = true;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Satellite")) return;

        // Verificar si el planeta está orbitando
        SatelliteController sc = other.GetComponent<SatelliteController>();
        if (sc != null && !sc.isOrbiting)
        {
            return;
        }

        Rigidbody rb = other.GetComponent<Rigidbody>();

        // Calcular la magnitud y dirección entre el planeta y el satélite.
        Vector3 direction = transform.position - other.transform.position;
        float distance = direction.magnitude;
        direction.Normalize();

        // aplicar gravedad normal
        rb.AddForce(direction * gravityStrength);

        // si está muy cerca del planeta
        if (distance < orbitDistance)
        {
            // compute planet radius (accounts for scale)
            float planetRadius = 1f;
            SphereCollider sphere = GetComponent<SphereCollider>();
            if (sphere != null)
            {
                planetRadius = sphere.radius * transform.localScale.x;
            }

            // compute satellite radius using its collider bounds
            float satelliteRadius = 0.5f;
            Collider satCollider = other.GetComponent<Collider>();
            if (satCollider != null)
            {
                satelliteRadius = Mathf.Max(
                    satCollider.bounds.extents.x,
                    satCollider.bounds.extents.y,
                    satCollider.bounds.extents.z
                );
            }

            // if the surfaces touch -> crash
            if (distance <= planetRadius + satelliteRadius)
            {
                Debug.Log("CRASH");
                return;
            }
            else
            {
                // Only enforce orbital motion if the satellite is currently orbiting
                if (sc != null && sc.isOrbiting)
                {
                    // Forzar órbita pero respetando la dirección actual del movimiento
                    Vector3 tangent = Vector3.ProjectOnPlane(rb.linearVelocity, direction);

                    // Si la proyección es muy pequeña, usamos un fallback
                    if (tangent.sqrMagnitude < 0.001f)
                    {
                        tangent = Vector3.Cross(direction, Vector3.up);
                    }

                    tangent = tangent.normalized;

                    rb.linearVelocity = tangent * orbitSpeed;
                }

                /* if(!cameraMoved && cameraTransform != null && nextPlanet != null)
                {
                    // camera distance depends on planet size
                    float cameraDistance = planetRadius * cameraDistanceFactor;

                    // direction from planet1 to planet2
                    Vector3 planetToNext = (nextPlanet.position - transform.position).normalized;

                    float cameraHeight = planetRadius * 2f;
                    UnityEngine.Vector3 basePosition = transform.position - planetToNext * cameraDistance;

                    Vector3 targetPosition = basePosition + UnityEngine.Vector3.up * cameraHeight;

                    // move camera smoothly instead of teleporting
                    StartCoroutine(MoveCameraSmooth(targetPosition, 1f));

                    cameraMoved = true;
                } */
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        cameraMoved = false;
    }

    IEnumerator TriggerCameraMoveWithDelay(float delay, Transform planet)

    {

        yield return new WaitForSeconds(delay);

        if (cameraTransform == null || nextPlanet == null) yield break;

        float planetRadius = 1f;

        SphereCollider sphere = GetComponent<SphereCollider>();

        if (sphere != null)

        {

            planetRadius = sphere.radius * transform.localScale.x;

        }

        float cameraDistance = planetRadius * cameraDistanceFactor;

        Vector3 planetToNext = (nextPlanet.position - planet.position).normalized;

        float cameraHeight = planetRadius * 2f;

        Vector3 basePosition = planet.position - planetToNext * cameraDistance;

        Vector3 targetPosition = basePosition + Vector3.up * cameraHeight;

        StartCoroutine(MoveCameraSmooth(targetPosition, 1f));

    }

    IEnumerator MoveCameraSmooth(Vector3 targetPosition, float duration)
    {
        float elapsed = 0f;
        Vector3 startingPosition = cameraTransform.position; // Capture the start!

        Vector3 velocity = Vector3.zero;

        while (Vector3.Distance(cameraTransform.position, targetPosition) > 0.01f)
        {
            float distance = Vector3.Distance(cameraTransform.position, targetPosition);
            float smoothTime = Mathf.Lerp(0.6f, 0.2f, 1 - (distance / 20f));

            cameraTransform.position = Vector3.SmoothDamp(
                cameraTransform.position,
                targetPosition,
                ref velocity,
                smoothTime // este valor controla suavidad
            );

            if (nextPlanet != null)
            {
                Vector3 direction = (nextPlanet.position - cameraTransform.position).normalized;

                Quaternion targetRotation = Quaternion.LookRotation(direction);

                cameraTransform.rotation = Quaternion.Slerp(

                    cameraTransform.rotation,

                    targetRotation,

                    Time.deltaTime * 5f // velocidad de rotación

                );
            }

            yield return null;
        }

        cameraTransform.position = targetPosition;
    }
}