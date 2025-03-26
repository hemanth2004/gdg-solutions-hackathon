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

        // Ask the backend for a response to the chat message
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
            string url = $"http://{_apiURL}{_apiEndpoint}";

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "application/json");

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
}