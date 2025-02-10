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
            domainIDInput.SetTextWithoutNotify(domainID);
            domainIDInput.onValueChanged.AddListener(OnDomainIDChanged);
        }

        if (bearerTokenInput != null)
        {
            bearerTokenInput.SetTextWithoutNotify(bearerToken);
            bearerTokenInput.onValueChanged.AddListener(OnBearerTokenChanged);
        }
    }

    private void OnDomainIDChanged(string newValue)
    {
        domainID = newValue;
    }

    private void OnBearerTokenChanged(string newValue)
    {
        bearerToken = newValue;
    }

    private void OnDestroy()
    {
        // Clean up listeners to prevent memory leaks
        if (domainIDInput != null)
        {
            domainIDInput.onValueChanged.RemoveListener(OnDomainIDChanged);
        }
        if (bearerTokenInput != null)
        {
            bearerTokenInput.onValueChanged.RemoveListener(OnBearerTokenChanged);
        }
    }
}