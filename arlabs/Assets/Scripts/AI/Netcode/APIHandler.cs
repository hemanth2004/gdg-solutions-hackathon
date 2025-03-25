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
        [SerializeField] private string _apiEndpoint = "/api/chat";

        [SerializeField] private bool _saveScreenshot = false;

        private void Awake()
        {
            Instance = this;
        }

        public async Task<string> AskBackendWithImage(string prompt)
        {
            return await CaptureAndSendImage(prompt);
        }

        public async Task<string> AskBackendWithText(string prompt)
        {
            return await SendRequestToBackend(prompt);
        }

        private async Task<string> CaptureAndSendImage(string prompt)
        {
            // Wait for the end of the frame using a coroutine
            var tcs = new TaskCompletionSource<bool>();
            StartCoroutine(WaitForEndOfFrameCoroutine(tcs));
            await tcs.Task;

            // Capture screenshot
            Texture2D screenTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            screenTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenTexture.Apply();

            // Convert to PNG
            byte[] imageBytes = screenTexture.EncodeToPNG();
            string base64Image = System.Convert.ToBase64String(imageBytes);

#if UNITY_EDITOR
            if (_saveScreenshot)
            {
                string path = Application.persistentDataPath + "/debug_screenshot.png";
                System.IO.File.WriteAllBytes(path, imageBytes);
                Debug.Log("Screenshot saved to: " + path);
            }
#endif

            DestroyImmediate(screenTexture);

            // Create simple JSON
            string jsonBody = JsonUtility.ToJson(new { prompt = prompt, image = base64Image });

            return await SendRequestToBackend(jsonBody);
        }

        // Coroutine to wait for the end of the frame (used for async/await)
        private IEnumerator WaitForEndOfFrameCoroutine(TaskCompletionSource<bool> tcs)
        {
            yield return new WaitForEndOfFrame();
            tcs.SetResult(true);
        }

        private async Task<string> SendRequestToBackend(string jsonBody)
        {
            string url = _apiURL + _apiEndpoint;

            // Set up the web request
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield(); // Wait asynchronously
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;
                    //Debug.Log("JSON Response: " + jsonResponse);

                    return jsonResponse;
                }
                else
                {
                    Debug.LogError($"Error sending request to Gemini: {request.responseCode} - {request.error}\nResponse: {request.downloadHandler.text}");
                    return null;
                }
            }
        }

    }

    public interface IAIMessage
    {
        string mainPrompt { get; set; }
        string sessionID { get; set; }
    }
}