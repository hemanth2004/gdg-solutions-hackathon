using Google.Api.Gax.Grpc.Rest;
using Google.Cloud.TextToSpeech.V1;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace ARLabs.AI
{
    [RequireComponent(typeof(AudioSource))]
    public class GoogleTTS : MonoBehaviour
    {
        private const int CHARACTER_LIMIT = 2000000; // 2 million character limit

        [Header("Get credentials json from GoogleCloud Console\nand place it in Assets/StreamingAssets folder with the name \"tts-credentials.json\"")]
        [Space]

        [SerializeField]
        private string _languageCode = "en-US";

        [SerializeField]
        private string _voiceName = "en-US-Standard-C";

        [SerializeField]
        private float _speakingRate = 1.5f;

        private AudioSource _audioSource;

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path.Combine(Application.streamingAssetsPath, "tts-credentials.json"));

            // Debugging current character count
            Debug.Log("Current character count: " + GetCharacterCount());
        }

        public async void Speak(string text)
        {
            // Check if the character limit has been reached
            int currentCount = GetCharacterCount();
            if (currentCount + text.Length > CHARACTER_LIMIT)
            {
                Debug.LogWarning("Character limit reached! No more requests allowed.");
                return;
            }

            // Update the stored character count
            AddCharacterCount(text.Length);

            TextToSpeechClient client = await new TextToSpeechClientBuilder
            {
                GrpcAdapter = RestGrpcAdapter.Default
            }.BuildAsync();

            SynthesizeSpeechRequest request = new SynthesizeSpeechRequest
            {
                Input = new SynthesisInput { Text = text },
                Voice = new VoiceSelectionParams
                {
                    LanguageCode = _languageCode,
                    Name = _voiceName,
                    SsmlGender = SsmlVoiceGender.Neutral
                },
                AudioConfig = new AudioConfig
                {
                    AudioEncoding = AudioEncoding.Mp3,
                    SpeakingRate = _speakingRate
                }
            };

            SynthesizeSpeechResponse response = await client.SynthesizeSpeechAsync(request);

            // Save and play audio
            string outputPath = Path.Combine(Application.persistentDataPath, "output.mp3");
            File.WriteAllBytes(outputPath, response.AudioContent.ToByteArray());

            StartCoroutine(PlayAudio(outputPath));
        }

        private IEnumerator PlayAudio(string path)
        {
            using (UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    _audioSource.clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                    _audioSource.Play();
                }
                else
                {
                    Debug.LogError("Error loading audio: " + www.error);
                }
            }
        }

        // Store character count in PlayerPrefs
        private int GetCharacterCount()
        {
            return PlayerPrefs.GetInt("TTS_CharacterCount", 0);
        }

        private void AddCharacterCount(int count)
        {
            int newCount = GetCharacterCount() + count;
            PlayerPrefs.SetInt("TTS_CharacterCount", newCount);
            PlayerPrefs.Save();
            Debug.Log("Updated character count: " + newCount);
        }

#if UNITY_EDITOR
        [ContextMenu("Speak Hello")]
        private void ContextMenu_SpeakHello()
        {
            Start();
            Speak("Hello");
        }
#endif
    }
}