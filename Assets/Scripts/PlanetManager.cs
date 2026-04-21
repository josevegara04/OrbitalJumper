using UnityEngine;
using System.Collections.Generic;

public class PlanetManager : MonoBehaviour
{
    public static PlanetManager Instance;
    public Texture2D[] planetTextures;

    public GameObject planetPrefab;
    public GameObject firstPlanet;

    List<GameObject> planets = new List<GameObject>();

    public float minDistance = 5f;
    public float maxDistance = 35f;
    public float coneAngle = 50f; // degrees
    public float distanceGrowth = 1.5f; // how much distance increases per planet
    public float maxDistanceCap = 50f; // absolute maximum distance
    public GameObject floatingTextPrefab;
    public int maxPlanets = 5;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        planets.Add(firstPlanet);

        planetTextures = Resources.LoadAll<Texture2D>("Textures");
        AssignRandomTexture(firstPlanet);
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

        // Progressive distance (no randomness)
        float growthFactor = planets.Count * distanceGrowth;

        float targetDistance = minDistance + growthFactor;

        // Clamp so it doesn't grow forever
        float distance = Mathf.Min(targetDistance, maxDistanceCap);

        Vector3 pos = lastPlanet.transform.position + dir * distance;

        GameObject newPlanet = Instantiate(planetPrefab, pos, Quaternion.identity);

        AssignRandomTexture(newPlanet);

        planets.Add(newPlanet);
        DestroyOldPlanet();

        PlanetGravity gravity = lastPlanet.GetComponent<PlanetGravity>();
        if (gravity != null)
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

    void AssignRandomTexture(GameObject planet)
    {
        Renderer rend = planet.GetComponent<Renderer>();

        if (rend != null && planetTextures.Length > 0)
        {
            Texture2D randomTexture = planetTextures[Random.Range(0, planetTextures.Length)];

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            rend.GetPropertyBlock(block);

            block.SetTexture("_MainTex", randomTexture);

            rend.SetPropertyBlock(block);
        }
    }
    
    void DestroyOldPlanet()
    {
        if (planets.Count <= maxPlanets)
        {
            return;
        }
        else
        {
            GameObject oldestPlanet = planets[0];
            planets.RemoveAt(0);

            if (oldestPlanet != null)
            {
                Destroy(oldestPlanet);
            }
        }
    }
}