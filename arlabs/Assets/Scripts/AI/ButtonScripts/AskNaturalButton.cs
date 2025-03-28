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
        public UnityEngine.UI.Button recordButton;
        public Lean.Gui.LeanToggle voiceInputWindowToggle;
        public GameObject loadingIcon, mainText;
        public TMPro.TMP_Text transcriptDisplay, recordTimeText;

        private string _latestImage = "";
        private string _latestAudio = "";
        private ExperimentContext _latestExperimentContext;


        // When the button on the AI tray is clicked, 
        // which opens the menu for voice and image capture
        public void OnClick()
        {
            ExperimentContext experiment = ExperimentContext.GetExperimentContext();
            _latestExperimentContext = experiment;
            Debug.Log("Experiment context: " + _latestExperimentContext);
            Debug.Log("Experiment context: " + _latestExperimentContext.name);
            Debug.Log("Experiment context: " + _latestExperimentContext.subject);

            recordButton.interactable = true;
            recordButton.GetComponent<Image>().raycastTarget = true;
            transcriptDisplay.text = "";
            loadingIcon.SetActive(false);
            mainText.SetActive(true);
            voiceInputWindowToggle.TurnOn();
            recordTimeText.text = "";

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
            loadingIcon.SetActive(false);
            recordButton.interactable = true;
            recordButton.GetComponent<Image>().raycastTarget = true;

            if (GoogleSTT.Instance.IsRecording)
            {
                GoogleSTT.Instance.StopRecording();
                loadingIcon.SetActive(true);
                recordButton.interactable = false;
                recordButton.GetComponent<Image>().raycastTarget = false;
                FinalizeChatMessage();
            }
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
                recordButton.interactable = true;
                recordButton.GetComponent<Image>().raycastTarget = true;
                recordTimeText.text = "";
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
            string response = "";
            try
            {
                response = await APIHandler.Instance.AskBackend(jsonMessage);
                Debug.Log("Response: " + response);
                ResponseManager.Instance.ProcessResponse(response);
                voiceInputWindowToggle.TurnOff();
            }
            catch (Exception e)
            {
                Debug.LogError("Error: " + e.Message);
            }

            voiceInputWindowToggle.TurnOff();
            mainText.SetActive(true);
            transcriptDisplay.text = "";
            loadingIcon.SetActive(false);
            _latestImage = "";
            _latestAudio = "";
        }
    }
}
