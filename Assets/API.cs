using System.Collections.Generic;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
public static class UrlHelper
{
    public static string BuildUrlWithParams(string baseUrl, Dictionary<string, string> parameters)
    {
        if (parameters == null || parameters.Count == 0)
            return baseUrl;

        StringBuilder query = new StringBuilder(baseUrl);
        query.Append('?');

        foreach (var param in parameters)
        {
            query.Append($"{UnityWebRequest.EscapeURL(param.Key)}={UnityWebRequest.EscapeURL(param.Value)}&");
        }

        // Remove the last '&' character
        query.Length--;

        return query.ToString();
    }
}

public class DataLoader
{
    public IEnumerator FetchData(
        string url,
        string authToken,
        string requestBody,
        System.Action<string> onSuccess,
        System.Action<string> onError
    )
    {
        // Create the UnityWebRequest with a POST method
        UnityWebRequest request = new UnityWebRequest(url, "POST");

        // Add headers (e.g., Authorization header)
        request.SetRequestHeader("Authorization", $"Bearer {authToken}");
        request.SetRequestHeader("Content-Type", "application/json");

        // If you need to send a body with the request
        if (!string.IsNullOrEmpty(requestBody))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        }

        // Set up the download handler to capture the response
        request.downloadHandler = new DownloadHandlerBuffer();

        // Send the request and wait for the response
        yield return request.SendWebRequest();

        // Handle errors
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            onError?.Invoke(request.error);
        }
        else
        {
            onSuccess?.Invoke(request.downloadHandler.text);
        }
    }
}
