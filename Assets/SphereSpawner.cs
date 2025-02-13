using UnityEngine;

public class SphereSpawner : MonoBehaviour
{

    public GameObject nodeLabelWithBackgroundPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // InvokeRepeating("SpawnSphere", 1f, 1f);
        InvokeRepeating("CreateRandomLabel", 1f, 1f);

    }

    void CreateRandomLabel()
    {
        // create 5 randomg strings of 3-10 words of different lengths
        string[] labels = { "Hello", "Hello world", "Hello world this is a test", "Hello world this is a test label", "Hello world this is a test label for the sphere" };
        SpawnNodeLabelWithBackground(labels[Random.Range(0, labels.Length)]);
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

    void SpawnNodeLabelWithBackground(string label)
    {
        // Create a new sphere primitive
        GameObject nodeLabelWithBackground = Instantiate(nodeLabelWithBackgroundPrefab);

        // Option 1: Spawn at a random position within a defined range
        float x = Random.Range(-20f, 20f);
        float y = Random.Range(-20f, 20f);
        float z = Random.Range(-20f, 20f);
        nodeLabelWithBackground.transform.position = new Vector3(x, y, z);

        nodeLabelWithBackground.GetComponent<LabelManager>().SetLabel(label);

        // Option 2: Alternatively, spawn them in a line (uncomment below to use)
        // Option 2: Alternatively, spawn them in a line (uncomment below to use)
        // sphere.transform.position = new Vector3(Time.time, 0, 0);
    }


    // Update is called once per frame
    void Update()
    {

    }
}
