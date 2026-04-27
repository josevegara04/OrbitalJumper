using System;
using System.Collections;
using System.Numerics;
using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SatelliteController : MonoBehaviour
{
    Rigidbody rb;

    UnityEngine.Vector3 startMousePosition;
    UnityEngine.Vector3 endMousePosition;
    UnityEngine.Vector3 startSatellitePosition;
    bool isDragging = false;
    public Transform sun;
    public bool isOrbiting = false;
    public float launchPower = 0.1f;
    public float dragSensitivity = 0.0005f;
    public float maxDragDistance = 300f;
    public float minDragDistance = 300f;
    public float maxDragAngle = 10f;
    public float orbitLaunchForce = 1f;
    bool launched = false;
    float lastDistance = Mathf.Infinity;
    float failTimer = 0f;
    public float failDelay = 2f;
    float currentDistance = 0f;

    public Transform cameraTransform;
    public Transform nextPlanet;
    UnityEngine.Vector3 cameraOffset;

    public LineRenderer aimLine;
    UnityEngine.Vector3 finalAimDirection;
    float finalForce;

    void Start()
    {
        rb = GetComponent<Rigidbody>();


        if (cameraTransform != null)
        {
            cameraOffset = cameraTransform.position - transform.position;
        }

        // Desactivar la línea de proyección del satélite.
        aimLine.enabled = false;
    }

    void Update()
    {
        Shader.SetGlobalVector("_SunPosition", sun.position);
        Debug.Log("Sun pos: " + sun.position);

        // Verificar si el satélite está orbitando en un planeta
        if (isOrbiting)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame && nextPlanet != null)
            {
                print("expulsando");
                LaunchFromOrbit();
            }
        }

        GameObject first = GameObject.FindWithTag("FirstPlanet");
        if (nextPlanet == null && first != null)
        {
            nextPlanet = first.transform;
        }

        if (launched && !isOrbiting && nextPlanet != null)
        {
            currentDistance = UnityEngine.Vector3.Distance(transform.position, nextPlanet.position);

            SphereCollider sphere = nextPlanet.GetComponent<SphereCollider>();

            float planetRadius = sphere.radius * nextPlanet.localScale.x;
            float maxDistance = planetRadius * 8f;

            if (currentDistance > lastDistance)
            {
                failTimer += Time.deltaTime;

                RestartGame();
            }
            else
            {
                failTimer = 0f;
            }

            lastDistance = currentDistance;
        }

        // Cuando se presiona el mouse
        if (Mouse.current.leftButton.wasPressedThisFrame && !launched)
        {
            // Se guarda la posición inicial del mouse y del satélite.
            startMousePosition = Mouse.current.position.ReadValue();
            startSatellitePosition = transform.position;
            isDragging = true;
            aimLine.enabled = true;
        }

        // Cuando el click del mouse se manteniene presionado.
        if (isDragging && Mouse.current.leftButton.isPressed && !launched)
        {
            // se guarda la posición del mouse.
            UnityEngine.Vector3 currentMousePosition = Mouse.current.position.ReadValue();

            // Se guarda el vector del arrestre hasta el momento.
            UnityEngine.Vector2 dragScreen = (UnityEngine.Vector2)(currentMousePosition - startMousePosition);

            // limitar distancia
            dragScreen = UnityEngine.Vector2.ClampMagnitude(dragScreen, maxDragDistance);

            // dirección de disparo
            UnityEngine.Vector3 aimDirection = new UnityEngine.Vector3(-dragScreen.x, 0, -dragScreen.y);

            // limitar ángulo
            float angle = UnityEngine.Vector3.Angle(UnityEngine.Vector3.forward, aimDirection);

            // Cuando el ángulo de arrastre es mayor al permitido
            if (angle > maxDragAngle)
            {
                UnityEngine.Vector3 clampedDir = UnityEngine.Vector3.RotateTowards(
                    UnityEngine.Vector3.forward,
                    aimDirection.normalized,
                    maxDragAngle * Mathf.Deg2Rad,
                    0
                );

                // mantener la misma magnitud pero con dirección limitada
                aimDirection = clampedDir * aimDirection.magnitude;

                // recalcular dragScreen para que el satélite también respete el límite
                dragScreen = new UnityEngine.Vector2(-aimDirection.x, -aimDirection.z);
            }

            finalAimDirection = aimDirection.normalized;
            finalForce = dragScreen.magnitude;

            // mover satélite usando el vector ya limitado
            UnityEngine.Vector3 move = new UnityEngine.Vector3(dragScreen.x, 0, dragScreen.y) * dragSensitivity;
            transform.position = startSatellitePosition + move;

            aimLine.SetPosition(0, transform.position);
            aimLine.SetPosition(1, transform.position + aimDirection * 0.01f);

            if (cameraTransform != null)
            {
                cameraTransform.position = cameraOffset + transform.position;
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging && !launched)
        {
            endMousePosition = Mouse.current.position.ReadValue();
            
            UnityEngine.Vector2 dragVector = (UnityEngine.Vector2)(startMousePosition - endMousePosition);

            if (dragVector.magnitude < minDragDistance)
            {
                // No fue suficiente arrastre → volver a la posición inicial
                transform.position = startSatellitePosition;
                aimLine.enabled = false;
            }
            else
            {
                Launch();
            }

            isDragging = false;
        }

        // Logic for LineRenderer visuals
        if (isDragging && aimLine.enabled)
        {
            float pulse = Mathf.Sin(Time.time * 10f) * 0.5f + 0.5f;
            float alpha = Mathf.Lerp(0.3f, 1f, pulse);

            // ✅ Asigna el color completo, no solo el alpha
            aimLine.startColor = new Color(1f, 1f, 1f, alpha);
            aimLine.endColor   = new Color(1f, 1f, 1f, 0f);
        }
    }

    void Launch()
    {
        rb.AddForce(finalAimDirection * finalForce * launchPower, ForceMode.Impulse);
        launched = true;
        aimLine.enabled = false;
    }

    void LaunchFromOrbit()
    {
        if (nextPlanet == null) return;

        // Dirección hacia el siguiente planeta
        UnityEngine.Vector3 launchDirection = rb.linearVelocity.normalized;

        if (launchDirection.sqrMagnitude < 0.001f)
        {
            launchDirection = transform.forward;
        }

        // Detener la velocidad orbital
        rb.linearVelocity = UnityEngine.Vector3.zero;

        // Aplicar impulso hacia el siguiente planeta
        rb.AddForce(launchDirection * orbitLaunchForce, ForceMode.Impulse);

        isOrbiting = false;
        lastDistance = UnityEngine.Vector3.Distance(transform.position, nextPlanet.position);
        failTimer = 0f;
    }
    
    public LoseAnimation loseAnimation;

    public void RestartGame()
    {
        StartCoroutine(LoseSequence());
    }

    IEnumerator LoseSequence()
    {
        if (loseAnimation != null)
        {
            yield return StartCoroutine(loseAnimation.PlayLoseAnimation());
        }

        yield return new WaitForSeconds(0.2f);

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}