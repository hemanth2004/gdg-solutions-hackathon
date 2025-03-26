using UnityEngine;
using System.Collections.Generic;
using ARLabs.Core;
using System.IO;
using System;
using UnityEditor;

namespace ARLabs.AI
{
    // Handles the explain experiment button
    public class ExplainExperimentButton : MonoBehaviour
    {
        public AudioClip[] testClips;
        public void OnClick()
        {
            AudActionResponse response = new AudActionResponse();
            List<string> audioClips = new List<string>();
            foreach (var audioClip in testClips)
            {
                audioClips.Add(ACToBS64(audioClip));
            }
            response.audio = audioClips.ToArray();
            response.action = new string[] { "electron_flow on", "electron_flow off" };

            ResponseManager.Instance.ProcessResponse(JsonUtility.ToJson(response));


            // ExperimentContext experimentContext = ExperimentContext.GetExperimentContext();

            // AIChatMessage aiChatMessage = new AIChatMessage();
            // aiChatMessage.sessionID = ExperimentManager.Instance.SessionID;
            // aiChatMessage.prompt = "Explain the experiment concisely, no filler words";
            // aiChatMessage.experimentContext = experimentContext;

            // // Send only the JSON object
            // string jsonMessage = JsonUtility.ToJson(aiChatMessage);
            // APIHandler.Instance.AskBackend(jsonMessage);
        }

        // Utility to convert an audio clip to a base64 string
        public string ACToBS64(AudioClip audioClip)
        {
            // Get the original MP3 file path
            string mp3Path = AssetDatabase.GetAssetPath(audioClip);

            // Read the MP3 file directly and convert to base64
            byte[] mp3Bytes = File.ReadAllBytes(mp3Path);
            return Convert.ToBase64String(mp3Bytes);
        }

    }
}

