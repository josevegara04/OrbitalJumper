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

        Vector3 dir = Random.onUnitSphere;
        dir.y = 0;
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
}