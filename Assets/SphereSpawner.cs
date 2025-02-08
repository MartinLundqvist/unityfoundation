using UnityEngine;

public class SphereSpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating("SpawnSphere", 1f, 1f);
    }

    void SpawnSphere()
    {
        // Create a new sphere primitive
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        // Option 1: Spawn at a random position within a defined range
        float x = Random.Range(-5f, 5f);
        float y = Random.Range(-5f, 5f);
        float z = Random.Range(-5f, 5f);
        sphere.transform.position = new Vector3(x, y, z);

        // Option 2: Alternatively, spawn them in a line (uncomment below to use)
        // sphere.transform.position = new Vector3(Time.time, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
