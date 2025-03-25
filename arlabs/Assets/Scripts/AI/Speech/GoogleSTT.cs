using Google.Api.Gax.Grpc.Rest;
using Google.Cloud.TextToSpeech.V1;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace ARLabs.AI
{
    public class GoogleSTT : MonoBehaviour
    {
        private const string API_KEY = "AIzaSyB0000000000000000000000000000000";

        private string _latestAudio = "";

        private void Start()
        {
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path.Combine(Application.streamingAssetsPath, "stt-credentials.json"));
        }
    }
}