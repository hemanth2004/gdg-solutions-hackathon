using UnityEngine;
using ARLabs.Core;
using System;
using System.IO;
using System.Collections;
using System.Threading.Tasks;

namespace ARLabs.AI
{
    // Handles the ask natural button
    public class AskNaturalButton : MonoBehaviour
    {
        public GoogleSTT googleSTT;
        public bool alsoSaveScreenshot = false;

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
            // do something idk
        }

        // When the record button is released   
        public void OnEndRecord(AudioClip audioClip)
        {
            string base64Audio = ACToBS64(audioClip);
            _latestAudio = base64Audio;

            FinalizeChatMessage();
        }

        // Finalizes the chat message and send to backend
        public async void FinalizeChatMessage()
        {
            string transcript = await googleSTT.GetTextFromAudio(_latestAudio);

            AIChatMessage aiChatMessage = new AIChatMessage();
            aiChatMessage.sessionID = ExperimentManager.Instance.SessionID;
            aiChatMessage.prompt = transcript;
            aiChatMessage.experimentContext = _latestExperimentContext;
            aiChatMessage.base64Image = _latestImage;

            // Send only the JSON object
            string jsonMessage = JsonUtility.ToJson(aiChatMessage);
            APIHandler.Instance.AskBackend(jsonMessage);

            _latestImage = "";
            _latestAudio = "";
        }


        // Attaches an image to the chat message
        public void OnClickAttachImage()
        {
            CaptureAndStoreImage();
        }

        // Frame capture mechanism
        private async void CaptureAndStoreImage()
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
            if (alsoSaveScreenshot)
            {
                string path = Application.persistentDataPath + "/debug_screenshot.png";
                System.IO.File.WriteAllBytes(path, imageBytes);
                Debug.Log("Screenshot saved to: " + path);
            }
#endif
            DestroyImmediate(screenTexture);

            _latestImage = base64Image;
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

        private IEnumerator WaitForEndOfFrameCoroutine(TaskCompletionSource<bool> tcs)
        {
            yield return new WaitForEndOfFrame();
            tcs.SetResult(true);
        }
    }
}
