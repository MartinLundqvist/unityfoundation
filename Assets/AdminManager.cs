using UnityEngine;
using TMPro;
using System.Collections.Generic;
public class AdminManager : MonoBehaviour
{
    [SerializeField] private string domainID = "";
    [SerializeField] private string bearerToken = "";
    [SerializeField] private string rootAssetID = "";
    [SerializeField] private TMP_InputField domainIDInput;
    [SerializeField] private TMP_InputField bearerTokenInput;
    [SerializeField] private TMP_InputField rootAssetIDInput;
    [SerializeField] private TMP_Dropdown rootAssetDropdown;
    [SerializeField] private GraphManager graphManager;
    private RootAssetsFetcher rootAssetsFetcher;

    // Maps the index of the dropdown to the root asset ID
    private Dictionary<int, string> rootAssetIDdMapping = new Dictionary<int, string>();

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
            // rootAssetIDInput.onValueChanged.AddListener(OnRootAssetIDChanged);
        }

        if (rootAssetDropdown != null)
        {
            rootAssetsFetcher = rootAssetDropdown.GetComponent<RootAssetsFetcher>();
            if (rootAssetsFetcher == null)
            {
                Debug.LogWarning("RootAssetsFetcher component not found on rootAssetDropdown");
            }
            rootAssetDropdown.onValueChanged.AddListener(OnRootAssetDropdownChanged);
            rootAssetDropdown.options.Clear();
        }
    }

    private void OnDomainIDChanged(string newValue)
    {
        domainID = newValue;
    }

    private void OnBearerTokenChanged(string newValue)
    {
        bearerToken = newValue;
        if (rootAssetsFetcher != null)
        {
            rootAssetsFetcher.UpdateBearerToken(bearerToken);
        }
    }

    // private void OnRootAssetIDChanged(string newValue)
    // {
    //     rootAssetID = newValue;
    // }

    private void OnRootAssetDropdownChanged(int newValue)
    {
        rootAssetID = rootAssetIDdMapping[newValue];

        // Update the TMP input field with the new root asset ID
        if (rootAssetIDInput != null)
        {
            rootAssetIDInput.SetTextWithoutNotify(rootAssetID);
        }

        // Rebuild the graph
        if (graphManager != null)
        {
            graphManager.RebuildGraph();
        }
        else
        {
            Debug.LogWarning("GraphManager reference not set in AdminManager");
        }
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

        if (rootAssetDropdown != null)
        {
            rootAssetDropdown.onValueChanged.RemoveListener(OnRootAssetDropdownChanged);
        }

        // if (rootAssetIDInput != null)
        // {
        //     rootAssetIDInput.onValueChanged.RemoveListener(OnRootAssetIDChanged);
        // }
    }

    public void AddRootAssetMapping(int index, string rootAssetId)
    {
        if (rootAssetIDdMapping == null)
        {
            rootAssetIDdMapping = new Dictionary<int, string>();
        }
        rootAssetIDdMapping[index] = rootAssetId;
    }

    public void ClearRootAssetMappings()
    {
        rootAssetIDdMapping.Clear();
    }
}