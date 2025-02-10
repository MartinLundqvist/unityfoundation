using UnityEngine;
using TMPro;

public class AdminManager : MonoBehaviour
{
    [SerializeField] private string domainID = "";
    [SerializeField] private string bearerToken = "";
    [SerializeField] private TMP_InputField domainIDInput;
    [SerializeField] private TMP_InputField bearerTokenInput;

    public string DomainID => domainID;
    public string BearerToken => bearerToken;

    // Singleton pattern
    private static AdminManager instance;
    public static AdminManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AdminManager>();
                if (instance == null)
                {
                    Debug.LogError("No AdminManager found in scene!");
                }
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (domainIDInput != null)
        {
            domainIDInput.text = domainID;
        }

        if (bearerTokenInput != null)
        {
            bearerTokenInput.text = bearerToken;
        }
    }
}