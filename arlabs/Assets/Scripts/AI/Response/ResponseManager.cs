using UnityEngine;
using ARLabs.Core;
using System.Collections;
using System;
using System.IO;
using UnityEngine.Networking;
using NAudio.Wave;
using System.Collections.Generic;
using System.Xml;

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

                // Clean and fix JSON formatting issues
                jsonResponse = CleanJsonResponse(jsonResponse);

                Debug.Log("Processed JSON: " + jsonResponse);

                AIResponse response = JsonUtility.FromJson<AIResponse>(jsonResponse);
                Debug.Log("Parsed JSON");

                if (!string.IsNullOrEmpty(response.xml))
                {
                    ProcessXML(response.xml);
                    return;
                }

                /*
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
                }));*/
            }
            catch (Exception e)
            {
                Debug.LogError($"Error processing response: {e.Message}");
                Debug.LogException(e);
            }
        }

        public void Test()
        {
            ExperimentContext context = ExperimentContext.GetExperimentContext();
            Debug.Log(JsonUtility.ToJson(context));
        }

        private string CleanJsonResponse(string jsonResponse)
        {
            try
            {
                // Find the xml field and its value
                int xmlIndex = jsonResponse.IndexOf("\"xml\":");
                if (xmlIndex == -1) return jsonResponse;

                int xmlValueStart = jsonResponse.IndexOf("\"", xmlIndex + 6);
                if (xmlValueStart == -1) return jsonResponse;

                int xmlValueEnd = jsonResponse.LastIndexOf("\"");
                if (xmlValueEnd <= xmlValueStart) return jsonResponse;

                // Extract the xml content
                string before = jsonResponse.Substring(0, xmlValueStart + 1);
                string xmlContent = jsonResponse.Substring(xmlValueStart + 1, xmlValueEnd - xmlValueStart - 1);
                string after = jsonResponse.Substring(xmlValueEnd);

                // Escape problematic characters in the XML content
                xmlContent = xmlContent.Replace("\\", "\\\\"); // Escape backslashes first
                xmlContent = xmlContent.Replace("\"", "\\\""); // Escape quotes
                xmlContent = xmlContent.Replace("\n", "\\n");  // Escape newlines
                xmlContent = xmlContent.Replace("\r", "\\r");  // Escape carriage returns
                xmlContent = xmlContent.Replace("\t", "\\t");  // Escape tabs

                string cleanedJson = before + xmlContent + after;
                Debug.Log("Cleaned JSON formatting");
                return cleanedJson;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error cleaning JSON: {e.Message}");
                return jsonResponse; // Return original if cleaning fails
            }
        }


        public void ProcessXML(string xml)
        {
            try
            {
                Debug.Log("Processing XML response: " + xml);
                
                // Parse the XML
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                
                XmlNode responseNode = doc.SelectSingleNode("Response");
                if (responseNode == null)
                {
                    Debug.LogError("No Response element found in XML");
                    return;
                }
                
                // Extract audio elements and actions in order
                List<(string type, string content)> orderedElements = new List<(string, string)>();
                
                ExtractOrderedElements(responseNode, orderedElements);
                
                Debug.Log($"Extracted {orderedElements.Count} elements in order");
                
                // Separate audio and actions while preserving order information
                List<string> base64AudioList = new List<string>();
                List<string> actionStrings = new List<string>();
                List<int> audioIndices = new List<int>();
                List<int> actionIndices = new List<int>();
                
                for (int i = 0; i < orderedElements.Count; i++)
                {
                    if (orderedElements[i].type == "Audio")
                    {
                        base64AudioList.Add(orderedElements[i].content);
                        audioIndices.Add(i);
                    }
                    else if (orderedElements[i].type == "Action")
                    {
                        actionStrings.Add(orderedElements[i].content);
                        actionIndices.Add(i);
                    }
                }
                
                // Create audio clips array and actions array
                AudioClip[] audioClips = new AudioClip[base64AudioList.Count];
                string[] actions = actionStrings.ToArray();
                
                // Convert base64 audio data to AudioClips
                if (base64AudioList.Count > 0)
                {
                    StartCoroutine(ConvertAllAudio(base64AudioList, audioClips, () =>
                    {
                        Debug.Log("All audio clips converted. Starting ordered playback...");
                        StartCoroutine(PlayOrderedAudioAndActions(audioClips, actions, orderedElements, audioIndices, actionIndices));
                    }));
                }
                else
                {
                    // No audio segments, just execute actions
                    Debug.Log("No audio segments found. Executing actions only...");
                    StartCoroutine(PlayOrderedAudioAndActions(audioClips, actions, orderedElements, audioIndices, actionIndices));
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error processing XML: {e.Message}");
                Debug.LogException(e);
            }
        }
        
        private void ExtractOrderedElements(XmlNode parentNode, List<(string type, string content)> orderedElements)
        {
            foreach (XmlNode childNode in parentNode.ChildNodes)
            {
                if (childNode.NodeType == XmlNodeType.Element && childNode.Name == "Audio")
                {
                    // This is an Audio element - extract the base64 content
                    string audioContent = childNode.InnerText?.Trim();
                    if (!string.IsNullOrWhiteSpace(audioContent))
                    {
                        orderedElements.Add(("Audio", audioContent));
                        Debug.Log($"Extracted audio segment at position {orderedElements.Count - 1} (length: {audioContent.Length})");
                    }
                }
                else if (childNode.NodeType == XmlNodeType.Element && childNode.Name == "Action")
                {
                    // This is an Action element - convert to string
                    string actionXml = childNode.OuterXml;
                    orderedElements.Add(("Action", actionXml));
                    Debug.Log($"Extracted action at position {orderedElements.Count - 1}: {actionXml}");
                }
                else if (childNode.NodeType == XmlNodeType.Element)
                {
                    // Other elements - recurse to find audio and actions
                    ExtractOrderedElements(childNode, orderedElements);
                }
            }
        }

        private IEnumerator PlayOrderedAudioAndActions(AudioClip[] audioClips, string[] actions, List<(string type, string content)> orderedElements, List<int> audioIndices, List<int> actionIndices)
        {
            int currentAudioIndex = 0;
            int currentActionIndex = 0;
            
            foreach (var element in orderedElements)
            {
                // Check if processing is allowed
                if (!canProcessResponse)
                {
                    Debug.Log("Response processing stopped due to canProcessResponse being false");
                    yield break;
                }
                
                if (element.type == "Audio")
                {
                    if (currentAudioIndex < audioClips.Length && audioClips[currentAudioIndex] != null)
                    {
                        Debug.Log($"Playing audio segment {currentAudioIndex}");
                        _audioSource.clip = audioClips[currentAudioIndex];
                        _audioSource.Play();
                        yield return new WaitForSeconds(_audioSource.clip.length);
                        
                        // Check again after audio finishes
                        if (!canProcessResponse)
                        {
                            _audioSource.Stop();
                            yield break;
                        }
                    }
                    currentAudioIndex++;
                }
                else if (element.type == "Action")
                {
                    if (currentActionIndex < actions.Length)
                    {
                        Debug.Log($"Executing action {currentActionIndex}: {actions[currentActionIndex]}");
                        ActionHandler.HandleAction(actions[currentActionIndex]);
                    }
                    currentActionIndex++;
                }
            }
            
            Debug.Log("Finished processing all ordered elements");
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
                ActionHandler.HandleAction(actions[index.actionIndex]);
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
