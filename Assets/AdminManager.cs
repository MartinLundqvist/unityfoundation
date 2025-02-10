using UnityEngine;
using TMPro;

public class AdminManager : MonoBehaviour
{
    [SerializeField] private string domainID = "";
    [SerializeField] private string bearerToken = "";
    [SerializeField] private string rootAssetID = "";
    [SerializeField] private TMP_InputField domainIDInput;
    [SerializeField] private TMP_InputField bearerTokenInput;
    [SerializeField] private TMP_InputField rootAssetIDInput;

    public string DomainID => domainID;
    public string BearerToken => bearerToken;
    public string RootAssetID => rootAssetID;
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

        if (rootAssetIDInput != null)
        {
            rootAssetIDInput.SetTextWithoutNotify(rootAssetID);
            rootAssetIDInput.onValueChanged.AddListener(OnRootAssetIDChanged);
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

    private void OnRootAssetIDChanged(string newValue)
    {
        rootAssetID = newValue;
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
        if (rootAssetIDInput != null)
        {
            rootAssetIDInput.onValueChanged.RemoveListener(OnRootAssetIDChanged);
        }
    }
}