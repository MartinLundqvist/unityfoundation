using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;

public class RootAssetsFetcher : MonoBehaviour
{
    private TMP_Dropdown dropdown;
    private DataLoader dataLoader;
    private string apiUrl;
    private string bearerToken;

    [System.Serializable]
    private class Vertex
    {
        public string id;
        public string name;
        public string type;
        public bool validated;
        public Attribute[] attributes;
        public Attribute[] attributeSpecifications;
    }

    [System.Serializable]
    private class Attribute
    {
        public string name;
        public string value;
    }

    [System.Serializable]
    private class RootAssetsResponse
    {
        public Vertex[] vertices;
        public object[] relationships;
        public object[] identifiablePaths;
    }

    void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        dataLoader = new DataLoader();
    }

    void Start()
    {
        // Reset these in case they were changed by the AdminManager
        apiUrl = "https://dev.domain.foundation.arundo.com/domain/" + AdminManager.Instance.DomainID + "/graph";
        bearerToken = AdminManager.Instance.BearerToken;

        StartCoroutine(FetchRootAssets());
    }

    private IEnumerator FetchRootAssets()
    {
        Debug.Log("Fetching root assets from API...");
        Debug.Log($"API URL: {apiUrl}");

        string requestBody = @"{
            ""entrypoints"": {
                ""type"": ""Asset""
            },
            ""depth"": ""0"",
            ""relationships"": [
                {
                    ""type"": ""HAS_CHILD"",
                    ""direction"": ""outgoing""
                }
            ],
            ""vertices"": [
                ""Asset""
            ],
            ""returnVertices"": [
                ""Asset""
            ]
        }";

        yield return dataLoader.FetchData(
            apiUrl,
            bearerToken,
            requestBody, // No request body needed for GET request
            (jsonResponse) =>
            {
                try
                {
                    Debug.Log($"Received API response: {jsonResponse.Substring(0, Mathf.Min(500, jsonResponse.Length))}...");
                    PopulateDropdown(jsonResponse);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing root assets data: {e.Message}");
                    Debug.LogError($"Stack trace: {e.StackTrace}");
                }
            },
            (error) =>
            {
                Debug.LogError($"Error fetching root assets data: {error}");
            }
        );
    }

    private void PopulateDropdown(string jsonResponse)
    {
        try
        {
            RootAssetsResponse response = JsonUtility.FromJson<RootAssetsResponse>(jsonResponse);

            if (response.vertices == null || response.vertices.Length == 0)
            {
                Debug.LogWarning("No root assets found in response");
                return;
            }

            // Clear existing options and mappings
            dropdown.ClearOptions();
            AdminManager.Instance.ClearRootAssetMappings();

            // Create new list of dropdown options
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

            foreach (Vertex vertex in response.vertices)
            {
                // Only add vertices of type "Asset"
                if (vertex.type.Equals("Asset", StringComparison.OrdinalIgnoreCase))
                {
                    options.Add(new TMP_Dropdown.OptionData(vertex.name));

                    // Store the mapping of index to asset ID in AdminManager
                    int index = options.Count - 1;
                    AdminManager.Instance.AddRootAssetMapping(index, vertex.id);
                }
            }

            // Add options to dropdown
            dropdown.AddOptions(options);

            // If we have options, ensure the rootAssetID is set to the first one
            if (options.Count > 0)
            {
                dropdown.value = 0;
                dropdown.RefreshShownValue();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in PopulateDropdown: {e.Message}");
            Debug.LogError($"JSON response: {jsonResponse}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
    }

    public void RefreshRootAssets()
    {
        StartCoroutine(FetchRootAssets());
    }

    public void UpdateBearerToken(string newToken)
    {
        bearerToken = newToken;
        RefreshRootAssets();
    }
}