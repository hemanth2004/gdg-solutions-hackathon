using System;
using System.Collections;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ARLabs.AI
{

    public class APIHandler : MonoBehaviour
    {
        public static APIHandler Instance;

        [SerializeField] private string _apiURL = "localhost:8000";
        [SerializeField] private string _apiEndpoint = "/api/ai/";

        [SerializeField] private bool _saveScreenshot = false;

        private void Awake()
        {
            Instance = this;
        }

        public async Task<string> AskBackend(string aiChatMessage)
        {
            return await SendRequestToBackend(aiChatMessage);
        }

        // Coroutine to wait for the end of the frame (used for async/await)
        private IEnumerator WaitForEndOfFrameCoroutine(TaskCompletionSource<bool> tcs)
        {
            yield return new WaitForEndOfFrame();
            tcs.SetResult(true);
        }

        private async Task<string> SendRequestToBackend(string jsonBody)
        {
            string url = $"{_apiURL}{_apiEndpoint}";

#if !UNITY_EDITOR && UNITY_ANDROID
            // For Android builds, if needed
            UnityWebRequest.ClearCookieCache();
#endif

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "application/json");
                request.SetRequestHeader("User-Agent", "Mozilla/5.0 (Android) Unity/6000.0.23f1");

                // Add this line to allow self-signed certificates and HTTP
                request.certificateHandler = new AcceptAllCertificatesHandler();

                try
                {
                    var operation = request.SendWebRequest();
                    while (!operation.isDone)
                    {
                        await Task.Yield();
                    }

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        return request.downloadHandler.text;
                    }
                    else
                    {
                        Debug.LogError($"Error sending request: {request.responseCode} - {request.error}");
                        Debug.LogError($"Request URL: {url}");
                        Debug.LogError($"Request Body: {jsonBody}");
                        Debug.LogError($"Response: {request.downloadHandler.text}");
                        return null;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Exception in SendRequestToBackend: {e.Message}");
                    return null;
                }
            }
        }

    }

    // Add this class to handle certificates
    public class AcceptAllCertificatesHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true; // Accept all certificates
        }
    }
}