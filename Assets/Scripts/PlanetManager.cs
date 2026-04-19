using UnityEngine;
using System.Collections.Generic;

public class PlanetManager : MonoBehaviour
{
    public static PlanetManager Instance;

    public GameObject planetPrefab;
    public GameObject firstPlanet;

    List<GameObject> planets = new List<GameObject>();

    public float minDistance = 20f;
    public float maxDistance = 35f;
    public float coneAngle = 30f; // degrees

    void Awake()
    {
        Instance = this;
    } 

    void Start()
    {
        planets.Add(firstPlanet);
    }

    public void SpawnNextPlanet()
    {
        GameObject lastPlanet = planets[planets.Count - 1];

        // Determine forward direction based on last two planets (or default)
        Vector3 forwardDir;
        if (planets.Count >= 2)
        {
            GameObject current = planets[planets.Count - 1];
            GameObject previous = planets[planets.Count - 2];
            forwardDir = (current.transform.position - previous.transform.position).normalized;
        }
        else
        {
            forwardDir = Vector3.forward;
        }

        // Get a direction within a cone around forwardDir
        Vector3 dir = GetDirectionInCone(forwardDir, coneAngle);

        // keep movement mostly on XZ plane
        dir.y = 0f;
        dir.Normalize();

        float distance = Random.Range(minDistance, maxDistance);

        Vector3 pos = lastPlanet.transform.position + dir * distance;

        GameObject newPlanet = Instantiate(planetPrefab, pos, Quaternion.identity);

        planets.Add(newPlanet);

        PlanetGravity gravity = lastPlanet.GetComponent<PlanetGravity>();
        if(gravity != null)
        {
            gravity.nextPlanet = newPlanet.transform;
        }
    }

    Vector3 GetDirectionInCone(Vector3 forward, float angle)
    {
        float coneRadius = Mathf.Tan(angle * Mathf.Deg2Rad);

        Vector3 randomOffset = new Vector3(
            Random.Range(-coneRadius, coneRadius),
            Random.Range(-coneRadius, coneRadius),
            Random.Range(-coneRadius, coneRadius)
        );

        return (forward + randomOffset).normalized;
    }
}