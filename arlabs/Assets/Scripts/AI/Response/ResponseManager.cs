using UnityEngine;
using ARLabs.Core;
using System.Collections;
using System;
using System.IO;
using UnityEngine.Networking;
using NAudio.Wave;
using System.Collections.Generic;

namespace ARLabs.AI
{
    public class ResponseManager : MonoBehaviour
    {
        public static ResponseManager Instance;
        private void Awake() => Instance = this;

        public bool canProcessResponse = true;
        [SerializeField] private AudioSource _audioSource;

        // Process the response from the backend
        public void ProcessResponse(string jsonResponse)
        {
            if (!canProcessResponse)
            {
                Debug.Log("Response processing is currently disabled");
                return;
            }

            try
            {
                if (jsonResponse.StartsWith("\"") && jsonResponse.EndsWith("\""))
                {
                    jsonResponse = jsonResponse.Substring(1, jsonResponse.Length - 2);
                }
                jsonResponse = jsonResponse.Replace("\\\"", "\"");

                Debug.Log("Processed JSON: " + jsonResponse);

                AIResponse response = JsonUtility.FromJson<AIResponse>(jsonResponse);

                if (response == null || response.sequence == null)
                {
                    Debug.LogError("Failed to parse response: " + jsonResponse);
                    return;
                }

                int totalClips = response.sequence.audio.Count;
                AudioClip[] audioClips = new AudioClip[totalClips];
                string[] actions = response.sequence.vis.ToArray();

                StartCoroutine(ConvertAllAudio(response.sequence.audio, audioClips, () =>
                {
                    Debug.Log("All audio clips converted. Starting playback...");
                    StartCoroutine(PlayAudioAndExecuteAction(audioClips, actions, true, (0, 0)));
                }));
            }
            catch (Exception e)
            {
                Debug.LogError($"Error processing response: {e.Message}");
                Debug.LogException(e);
            }
        }

        private IEnumerator PlayAudioAndExecuteAction(AudioClip[] audioClips, string[] actions, bool isAudioChance, (int audioIndex, int actionIndex) index)
        {
            // Check if processing is allowed at the start of each iteration
            if (!canProcessResponse)
            {
                Debug.Log("Response processing stopped due to canProcessResponse being false");
                yield break;
            }

            if (isAudioChance && index.audioIndex < audioClips.Length)
            {
                if (audioClips[index.audioIndex] != null)
                {
                    _audioSource.clip = audioClips[index.audioIndex];
                    _audioSource.Play();
                    yield return new WaitForSeconds(_audioSource.clip.length);

                    // Check again after audio finishes playing
                    if (!canProcessResponse)
                    {
                        _audioSource.Stop();
                        yield break;
                    }

                    index.audioIndex++;
                }
            }
            else if (!isAudioChance && index.actionIndex < actions.Length)
            {
                ExecuteAction(actions[index.actionIndex]);
                index.actionIndex++;
            }

            // Continue if either array still has elements
            if ((index.audioIndex < audioClips.Length || index.actionIndex < actions.Length) && canProcessResponse)
            {
                // If we've run out of audio clips, just do actions
                if (index.audioIndex >= audioClips.Length)
                {
                    StartCoroutine(PlayAudioAndExecuteAction(audioClips, actions, false, index));
                }
                // If we've run out of actions, just do audio
                else if (index.actionIndex >= actions.Length)
                {
                    StartCoroutine(PlayAudioAndExecuteAction(audioClips, actions, true, index));
                }
                // Otherwise keep alternating
                else
                {
                    StartCoroutine(PlayAudioAndExecuteAction(audioClips, actions, !isAudioChance, index));
                }
            }
        }

        public bool ExecuteAction(string action)
        {
            string[] actionParts = action.Split(' ');
            bool vizCall = actionParts[1] == "on";

            VisualizationManager.Instance.ToggleVisualization(actionParts[0], vizCall);

            return true;
        }

        // public AudioClip ConvertBase64ToAudioClip(string base64EncodedMp3String)
        // {
        //     try
        //     {
        //         byte[] mp3Bytes = Convert.FromBase64String(base64EncodedMp3String);

        //         // Convert MP3 to raw PCM using NAudio
        //         using (var mp3Stream = new MemoryStream(mp3Bytes))
        //         using (var mp3Reader = new Mp3FileReader(mp3Stream))
        //         {
        //             // Force resample to 44100Hz which is more standard for Unity
        //             var waveFormat = new WaveFormat(44100, mp3Reader.WaveFormat.Channels);
        //             using (var resampler = new WaveFormatConversionStream(waveFormat, mp3Reader))
        //             {
        //                 // Calculate total samples
        //                 int sampleCount = (int)(resampler.Length / 2); // 2 bytes per sample

        //                 // Read PCM data
        //                 byte[] pcmBytes = new byte[resampler.Length];
        //                 resampler.Read(pcmBytes, 0, pcmBytes.Length);

        //                 // Convert to float samples that Unity can use
        //                 float[] samples = new float[sampleCount];
        //                 for (int i = 0; i < sampleCount; i++)
        //                 {
        //                     short sample = BitConverter.ToInt16(pcmBytes, i * 2);
        //                     samples[i] = sample / 32768f; // Convert to -1.0f to 1.0f range
        //                 }

        //                 // Create Unity AudioClip
        //                 AudioClip audioClip = AudioClip.Create("ConvertedClip", sampleCount / waveFormat.Channels,
        //                     waveFormat.Channels, waveFormat.SampleRate, false);
        //                 audioClip.SetData(samples, 0);
        //                 return audioClip;
        //             }
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.LogError($"Error converting audio: {e.Message}");
        //         Debug.LogException(e);  // This will print the full stack trace
        //         return null;
        //     }
        // }


        public IEnumerator ConvertBase64ToAudioClip(string base64EncodedMp3String, Action<AudioClip> onComplete)
        {
            byte[] mp3Bytes = Convert.FromBase64String(base64EncodedMp3String);
            string filePath = Path.Combine(Application.persistentDataPath, "temp.mp3");
            File.WriteAllBytes(filePath, mp3Bytes); // Save MP3 to storage

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error loading audio: " + www.error);
                    onComplete?.Invoke(null);
                }
                else
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    onComplete?.Invoke(clip);
                }
            }
        }

        private IEnumerator ConvertAllAudio(List<string> base64AudioList, AudioClip[] audioClips, Action onComplete)
        {
            int completed = 0;

            for (int i = 0; i < base64AudioList.Count; i++)
            {
                int index = i;
                StartCoroutine(ConvertBase64ToAudioClip(base64AudioList[i], (clip) =>
                {
                    audioClips[index] = clip;
                    completed++;

                    if (completed == base64AudioList.Count)
                    {
                        onComplete?.Invoke(); // Call when all clips are ready
                    }
                }));
            }

            yield return null; // Let coroutines execute
        }

    }

}
