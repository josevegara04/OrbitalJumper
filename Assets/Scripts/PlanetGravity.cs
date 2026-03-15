using UnityEngine;
using System.Collections;

public class PlanetGravity : MonoBehaviour
{
    public float gravityStrength = 20f;
    public float orbitDistance = 4f;
    public Transform cameraTransform;
    public Transform nextPlanet;
    public float cameraDistanceFactor = 0.5f;
    bool cameraMoved = false;
    private float orbitSpeed = 10f;

    void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Satellite")) return;
        SatelliteController sc = other.GetComponent<SatelliteController>();
        sc.isOrbiting = true;

        PlanetManager.Instance.SpawnNextPlanet();
        sc.nextPlanet = nextPlanet;
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Satellite")) return;

        // Verificar si el planeta está orbitando
        SatelliteController sc = other.GetComponent<SatelliteController>();
        if(sc != null && !sc.isOrbiting)
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

                if(!cameraMoved && cameraTransform != null && nextPlanet != null)
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
                }
            }
        }
    }

    IEnumerator MoveCameraSmooth(Vector3 targetPosition, float duration)
    {
        float elapsed = 0f;
        Vector3 startingPosition = cameraTransform.position; // Capture the start!

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            
            // This gives us a normalized value (0.0 to 1.0)
            float t = elapsed / duration;

            // SmoothStep removes the "jumpy" start and "abrupt" finish
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            cameraTransform.position = Vector3.Lerp(
                startingPosition, 
                targetPosition, 
                smoothT
            );

            if (nextPlanet != null)
            {
                cameraTransform.LookAt(nextPlanet);
            }

            yield return null;
        }

        cameraTransform.position = targetPosition;
    }
}