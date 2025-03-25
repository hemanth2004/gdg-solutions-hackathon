using UnityEngine;
using ARLabs.Core;
using System;
using System.IO;
using System.Collections;
using System.Threading.Tasks;

namespace ARLabs.AI
{
    public class AskNaturalButton : MonoBehaviour
    {

        public bool alsoSaveScreenshot = false;

        private string _latestImage = "";
        private string _latestAudio = "";

        public void OnClick()
        {
            ExperimentContext experiment = ExperimentContext.GetExperimentContext();
            experiment.mainPrompt = "Ask a natural question about the experiment";

            // Convert to JSON string before sending
            string jsonMessage = JsonUtility.ToJson(experiment);
            APIHandler.Instance.AskBackendWithText("Ask a natural question about the experiment\n" + jsonMessage);
        }


        public void OnBeginRecord()
        {

        }

        public void OnEndRecord(AudioClip audioClip)
        {
            string base64Audio = ACToBS64(audioClip);
            _latestAudio = base64Audio;
        }

        private IEnumerator WaitForEndOfFrameCoroutine(TaskCompletionSource<bool> tcs)
        {
            yield return new WaitForEndOfFrame();
            tcs.SetResult(true);
        }

        public void OnClickAttachImage()
        {
            CaptureAndStoreImage();
        }

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
