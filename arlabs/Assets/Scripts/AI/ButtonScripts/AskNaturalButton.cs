using UnityEngine;
using ARLabs.Core;
using System;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.UI;
namespace ARLabs.AI
{
    // Handles the ask natural button
    public class AskNaturalButton : MonoBehaviour
    {
        public CanvasGroup canvasGroup;
        public bool alsoSaveScreenshot = false;
        public Lean.Gui.LeanToggle voiceInputWindowToggle;
        public GameObject loadingIcon, mainText;
        public TMPro.TMP_Text transcriptDisplay;

        private string _latestImage = "";
        private string _latestAudio = "";
        private ExperimentContext _latestExperimentContext;


        // When the button on the AI tray is clicked, 
        // which opens the menu for voice and image capture
        public void OnClick()
        {
            ExperimentContext experiment = ExperimentContext.GetExperimentContext();
            _latestExperimentContext = experiment;
        }

        // When the record button is held down  
        public void OnBeginRecord()
        {
            ExperimentManager.Instance.StartScreenCapture(canvasGroup, (base64Image) =>
            {
                _latestImage = base64Image;
            });
            GoogleSTT.Instance.StartRecording();
        }

        // When the record button is released   
        public void OnEndRecord()
        {
            GoogleSTT.Instance.StopRecording();
            loadingIcon.SetActive(true);
            FinalizeChatMessage();

        }

        // Finalizes the chat message and send to backend
        public async void FinalizeChatMessage()
        {
            string transcript = await GoogleSTT.Instance.TranscribeSavedClip();
            Debug.Log("Transcript: " + transcript);
            if (transcript == "")
            {
                voiceInputWindowToggle.TurnOff();
                mainText.SetActive(true);
                transcriptDisplay.text = "";
                loadingIcon.SetActive(false);
                return;
            }

            mainText.SetActive(false);
            transcriptDisplay.text = transcript;
            AIChatMessage aiChatMessage = new AIChatMessage();
            aiChatMessage.sessionID = ExperimentManager.Instance.SessionID;
            aiChatMessage.prompt = transcript;
            aiChatMessage.experimentContext = _latestExperimentContext;
            aiChatMessage.base64Image = _latestImage;
            Debug.Log("b64 " + _latestImage);

            // Send only the JSON object
            string jsonMessage = JsonUtility.ToJson(aiChatMessage);
            string response = await APIHandler.Instance.AskBackend(jsonMessage);
            Debug.Log("Response: " + response);
            voiceInputWindowToggle.TurnOff();
            mainText.SetActive(true);
            transcriptDisplay.text = "";
            loadingIcon.SetActive(false);
            ResponseManager.Instance.ProcessResponse(response);


            _latestImage = "";
            _latestAudio = "";
        }

        // Utility to convert an audio clip to a base64 string
        public string ACToBS64(AudioClip audioClip)
        {
            float[] data = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(data, 0);

            byte[] bytes = new byte[data.Length * 2];
            int index = 0;
            foreach (float sample in data)
            {
                short convertedSample = (short)(sample * short.MaxValue);
                BitConverter.GetBytes(convertedSample).CopyTo(bytes, index);
                index += 2;
            }

            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
                    writer.Write(36 + bytes.Length);
                    writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
                    writer.Write(new char[4] { 'f', 'm', 't', ' ' });
                    writer.Write(16);
                    writer.Write((ushort)1);
                    writer.Write((ushort)audioClip.channels);
                    writer.Write(audioClip.frequency);
                    writer.Write(audioClip.frequency * audioClip.channels * 2);
                    writer.Write((ushort)(audioClip.channels * 2));
                    writer.Write((ushort)16);
                    writer.Write(new char[4] { 'd', 'a', 't', 'a' });
                    writer.Write(bytes.Length);
                    writer.Write(bytes);
                }
                byte[] wavBytes = stream.ToArray();
                return Convert.ToBase64String(wavBytes);
            }
        }
    }
}
