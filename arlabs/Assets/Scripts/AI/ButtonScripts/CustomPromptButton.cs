using UnityEngine;
using System.Collections.Generic;
using ARLabs.Core;
using System.IO;
using System;
using UnityEditor;
using System.Collections;
using System.Threading.Tasks;


namespace ARLabs.AI
{
    // Handles the explain experiment button
    public class CustomPromptButton : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject externalLoadingIcon;
        [SerializeField] private AskNaturalButton askNaturalButton;
        public string promptString;

        public async void OnClick()
        {
            externalLoadingIcon.SetActive(true);
            // Experiment context object
            ExperimentContext experimentContext = ExperimentContext.GetExperimentContext();

            // AI Chat message object
            AIChatMessage aiChatMessage = new AIChatMessage();
            aiChatMessage.sessionID = ExperimentManager.Instance.SessionID;
            aiChatMessage.prompt = promptString;
            aiChatMessage.experimentContext = experimentContext;

            // Capture the screenshot and save it as a base64 string
            string screenshotBase64 = string.Empty;
            ExperimentManager.Instance.StartScreenCapture(canvasGroup, (base64Image) =>
            {
                screenshotBase64 = base64Image;
            });

            aiChatMessage.base64Image = screenshotBase64;

            // Stringify and send over to backend
            string jsonMessage = JsonUtility.ToJson(aiChatMessage);
            Debug.Log("Json message: " + jsonMessage);
            string response = await APIHandler.Instance.AskBackend(jsonMessage);
            externalLoadingIcon.SetActive(false);

            // Process the response
            Debug.Log("Response: " + response);
            ResponseManager.Instance.ProcessResponse(response);

            // Reset the UI
            askNaturalButton.ResetUI();
        }

    }
}

