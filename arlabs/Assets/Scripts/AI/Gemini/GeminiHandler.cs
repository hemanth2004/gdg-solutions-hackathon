using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ARLabs.AI
{
    public class GeminiHandler : MonoBehaviour
    {
        #region Response Classes
        [System.Serializable]
        public class Part
        {
            public string text = null;
            public InlineData inlineData = null; // for images
        }

        [System.Serializable]
        public class InlineData
        {
            public string mimeType = null;
            public string data = null; // Base64 encoded image
        }

        [System.Serializable]
        public class Content
        {
            public Part[] parts;
            public string role;
        }

        [System.Serializable]
        public class Candidate
        {
            public Content content;
            public string finishReason;
        }

        [System.Serializable]
        public class GeminiResponse
        {
            public Candidate[] candidates;
        }
        #endregion

        private string _apiKey = System.Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        private string _apiURL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=";

        [Header("Make sure to get a gemini API key and save it as an\nEnvironment Variable with the name GEMINI_API_KEY")]
        [Header("You Can Access Functions Through Context Menu For Testing\nAskWithImage, AskWithText, ClearConversationHistory")]
        [Space]

        [SerializeField, TextArea(3, 10)]
        private string _systemPrompt = "You are a virtual lab instructor for science experiments following the CBSE syllabus.Guide students step by step, answer their questions, and help troubleshoot issues. Only answer relevant questions. If the questions are irrelevant, ask the user to only ask relevant questions.";

        [SerializeField, TextArea(10, 20)]
        private string _prompt;

        [SerializeField, TextArea(10, 20)]
        private string _response; // Only for viewing in the inspector

        [SerializeField]
        private int _maxHistoryLength = 5;

        [SerializeField]
        private List<Content> _conversationHistory = new List<Content>();

        [SerializeField]
        private bool _saveScreenshot = false;

        public async Task<string> AskGeminiWithImage(string prompt)
        {
            return await CaptureAndSendImage(prompt);
        }

        public async Task<string> AskGeminiWithText(string prompt)
        {
            return await SendRequestToGemini(CreateJsonWithHistory(prompt));
        }

        // Adds the system prompt as the first item in the conversation history
        public void UpdateSystemPrompt(string prompt)
        {
            Content systemMessage = new Content
            {
                role = "user",
                parts = new Part[] { new Part { text = prompt, inlineData = null } }
            };

            if (_conversationHistory.Count > 0)
                _conversationHistory[0] = systemMessage;
            else
                _conversationHistory.Add(systemMessage);
        }

        // Escapes special characters in a JSON string to prevent API and parsing errors
        private string EscapeJsonString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return "";

            return str.Replace("\\", "\\\\")  // Escape backslashes
                      .Replace("\"", "\\\"")  // Escape quotes
                      .Replace("\n", "\\n")   // Escape new lines
                      .Replace("\r", "\\r");  // Escape carriage returns
        }

        // Creates a JSON string with the conversation history
        // TODO: Use NewtonSoft
        private string CreateJsonWithHistory(string prompt, string base64Image = null)
        {
            List<Part> userParts = new List<Part> { new Part { text = EscapeJsonString(prompt), inlineData = null } };

            if (!string.IsNullOrEmpty(base64Image))
            {
                userParts.Add(new Part
                {
                    inlineData = new InlineData
                    {
                        mimeType = "image/png",
                        data = base64Image.Replace("\n", "") // Remove newlines
                    }
                });
            }

            _conversationHistory.Add(new Content { role = "user", parts = userParts.ToArray() });

            // Trim history to 10 items to avoid excessive tokens
            if (_conversationHistory.Count > _maxHistoryLength + 1)
            {
                _conversationHistory.RemoveAt(1);
            }

            // Convert history to JSON
            // TODO: Use NewtonSoft
            string historyJson = "[";
            foreach (var content in _conversationHistory)
            {
                string partJson = "[";
                foreach (var part in content.parts)
                {
                    if (part.inlineData != null && !string.IsNullOrEmpty(part.inlineData.data))
                    {
                        partJson += $@"{{""inlineData"": {{""mimeType"": ""image/png"", ""data"": ""{part.inlineData.data}""}}}},";
                    }
                    else
                    {
                        partJson += $@"{{""text"": ""{EscapeJsonString(part.text)}""}},";
                    }
                }
                partJson = partJson.TrimEnd(',') + "]";

                historyJson += $@"{{""role"": ""{content.role}"", ""parts"": {partJson}}},";
            }
            historyJson = historyJson.TrimEnd(',') + "]";


            return $@"{{""contents"": {historyJson}}}";
        }

        // Coroutine to wait for the end of the frame (used for async/await)
        private IEnumerator WaitForEndOfFrameCoroutine(TaskCompletionSource<bool> tcs)
        {
            yield return new WaitForEndOfFrame();
            tcs.SetResult(true);
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

            return await SendRequestToGemini(CreateJsonWithHistory(prompt, base64Image));
        }


        private async Task<string> SendRequestToGemini(string jsonBody)
        {
            string url = _apiURL + _apiKey;

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

                    // Parse Json
                    GeminiResponse parsedResponse = JsonUtility.FromJson<GeminiResponse>(jsonResponse);

                    if (parsedResponse != null && parsedResponse.candidates.Length > 0 && parsedResponse.candidates[0].content.parts.Length > 0)
                    {
                        string description = parsedResponse.candidates[0].content.parts[0].text;
                        _response = description;
                        //Debug.Log("Gemini Description: " + description);

                        // Store AI response in history
                        _conversationHistory.Add(new Content
                        {
                            role = "model",
                            parts = new Part[] { new Part { text = description, inlineData = null } }
                        });

                        return description;
                    }
                    else
                    {
                        Debug.LogError("Failed to parse Gemini response.");
                        return null;
                    }
                }
                else
                {
                    Debug.LogError($"Error sending request to Gemini: {request.responseCode} - {request.error}\nResponse: {request.downloadHandler.text}");
                    return null;
                }
            }
        }

        public void ClearHistory() => _conversationHistory.Clear();
        public string GetSystemPrompt() => _systemPrompt;

#if UNITY_EDITOR
        [ContextMenu("Ask Gemini With Text")]
        private async void ContextMenu_AskWithText()
        {
            await AskGeminiWithText(_prompt);
        }

        [ContextMenu("Ask Gemini With Image")]
        private async void ContextMenu_AskWithImage()
        {
            await AskGeminiWithImage(_prompt);
        }

        [ContextMenu("Clear Conversation History")]
        private void ContextMenu_ClearHistory()
        {
            _conversationHistory.Clear();
        }
#endif
    }
}
