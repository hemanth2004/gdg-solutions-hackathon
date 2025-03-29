using Google.Api.Gax.Grpc.Rest;
using Google.Cloud.Speech.V1;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Android;

namespace ARLabs.AI
{
    public class GoogleSTT : MonoBehaviour
    {
        #region Singleton
        public static GoogleSTT Instance;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }
        #endregion

        [SerializeField]
        private bool _playbackAudio = false;

        [SerializeField]
        private int MAX_RECORDING_TIME = 60;

        [SerializeField]
        private int SAMPLE_RATE = 16000;

        private string _latestTranscription = "";

        private AudioClip _audioClip;
        private bool _isRecording = false;
        private AudioSource _audioSource; // FOR DEBUGGING

        public bool IsRecording => _isRecording;

        private void Start()
        {
            RequestMicrophonePermission();

            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path.Combine(Application.streamingAssetsPath, "stt-credentials.json"));

            _audioSource = GetComponent<AudioSource>();
        }

        private void RequestMicrophonePermission()
        {
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }

            if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Debug.Log("Microphone permission granted.");
            }
            else
            {
                Debug.LogError("Microphone permission denied!");
            }
#endif
        }

        // Call externally
        public void StartRecording()
        {
            if (_isRecording)
            {
                Debug.Log("Already Recording");
                return;
            }

            int sampleRate = SAMPLE_RATE;
            _audioClip = Microphone.Start(null, true, MAX_RECORDING_TIME, sampleRate);
            Debug.Log("Recording...");

            _isRecording = true;
        }

        public void StopRecording()
        {
            if (!_isRecording) return;

            Microphone.End(null);
            Debug.Log("Recording stopped.");

#if UNITY_EDITOR
            if (_playbackAudio)
            {
                _audioSource.clip = _audioClip;
                _audioSource.Stop();
                _audioSource.Play();
            }
#endif

            _isRecording = false;
        }


        // Call externally
        public async Task<string> TranscribeSavedClip()
        {
            if (_audioClip == null) return "";

            try
            {
                byte[] pcmData = ExtractRawPCM(_audioClip);
                await TranscribePCM(pcmData);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to transcribe audio: {e.Message}");
            }

            _isRecording = false;

            return _latestTranscription;
        }

        //        // Call externally
        //        public async Task<string> StopRecordingAndTranscribe()
        //        {
        //            if (!_isRecording) return  "";

        //            Microphone.End(null);
        //            Debug.Log("Recording stopped.");

        //#if UNITY_EDITOR
        //            if (_playbackAudio)
        //            {
        //                _audioSource.clip = _audioClip;
        //                _audioSource.Stop();
        //                _audioSource.Play();
        //            }
        //#endif

        //            try
        //            {
        //                byte[] pcmData = ExtractRawPCM(_audioClip);
        //                await TranscribePCM(pcmData);
        //            }
        //            catch (System.Exception e)
        //            {
        //                Debug.LogError($"Failed to transcribe audio: {e.Message}");
        //            }

        //            _isRecording = false;

        //            return _latestTranscription;
        //        }

        private byte[] ExtractRawPCM(AudioClip clip)
        {
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            byte[] pcmData = new byte[samples.Length * 2];

            for (int i = 0; i < samples.Length; i++)
            {
                short intSample = (short)(samples[i] * short.MaxValue);
                pcmData[i * 2] = (byte)(intSample & 0xFF);
                pcmData[i * 2 + 1] = (byte)((intSample >> 8) & 0xFF);
            }

            Debug.Log($"Extracted PCM data: {pcmData.Length} bytes");
            return pcmData;
        }

        private async Task TranscribePCM(byte[] pcmData)
        {
            var speechClient = await new SpeechClientBuilder
            {
                GrpcAdapter = RestGrpcAdapter.Default
            }.BuildAsync();

            var audio = RecognitionAudio.FromBytes(pcmData);
            var config = new RecognitionConfig
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,  // Raw PCM format
                SampleRateHertz = SAMPLE_RATE,
                LanguageCode = LanguageCodes.English.India
            };

            RecognizeResponse response = null;
            try
            {
                response = await speechClient.RecognizeAsync(config, audio);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"API call failed: {ex.Message}");
                return;
            }

            _latestTranscription = "";

            if (response != null && response.Results.Count > 0)
            {
                foreach (var result in response.Results)
                {
                    if (result.Alternatives.Count > 0)
                    {
                        _latestTranscription += result.Alternatives[0].Transcript + " ";
                    }
                }

                _latestTranscription = _latestTranscription.Trim();
                Debug.Log($"Full Transcript: {_latestTranscription}");
            }
            else
            {
                Debug.Log("No transcription results.");
            }
        }
    }
}