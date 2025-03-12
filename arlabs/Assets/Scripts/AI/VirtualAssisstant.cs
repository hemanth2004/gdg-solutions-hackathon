using UnityEngine;

namespace ARLabs.AI
{
    public class VirtualAssisstant : MonoBehaviour
    {
        [Header("**ENSURE API KEYS ARE SETUP PROPERLY**\n(Select the GeminiHandler And GoogleTTS Game Objects For More Details)")]
        [Header("DEBUG: Press 1 for sending image, 2 for sending only text to Gemini.\nTry to avoid enabling shouldSpeak to stay within free usage limit of Google TTS.\nYou may also ask to respond in fewest amount of characters")]
        [Space]

        [SerializeField]
        private GeminiHandler _geminiHandler;

        [SerializeField]
        private GoogleTTS _googleTTS;

        [SerializeField]
        private string _experimentName = "Ohm's Law Experiment";

        [SerializeField, TextArea(10, 20)]
        private string _prompt;

        [SerializeField]
        private bool _shouldSpeak = true;

        private void Update()
        {
            // FOR Testing
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                AskAndSpeak(_prompt, true);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                AskAndSpeak(_prompt, false);
            }
        }

        public async void AskAndSpeak(string prompt, bool sendImage)
        {
            SetupSystemPrompt();
            string geminiResponse = sendImage ? await _geminiHandler.AskGeminiWithImage(prompt) : await _geminiHandler.AskGeminiWithText(prompt);
            if (string.IsNullOrEmpty(geminiResponse))
            {
                Debug.LogWarning("Gemini response is empty. Cannot speak.");
                return;
            }

            if (_shouldSpeak)
                _googleTTS.Speak(geminiResponse);
            else
                Debug.Log(geminiResponse);
        }

        private void SetupSystemPrompt()
        {
            string systemPrommpt = _geminiHandler.GetSystemPrompt();
            if (!string.IsNullOrEmpty(_experimentName))
                systemPrommpt += $"The user is virtually performing the {_experimentName} experiment.";
            _geminiHandler.UpdateSystemPrompt(systemPrommpt);
        }
    }
}