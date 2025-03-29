using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

namespace Recorder
{
    [RequireComponent(typeof(EventTrigger))]
    public class RecorderUI : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        #region Private Variables
        /// <summary>
        /// Is Recording
        /// </summary>
        private bool isRecording = false;
        /// <summary>
        /// Recording Time
        /// </summary>
        private float recordingTime = 0f;
        /// <summary>
        /// Recording Time Minute and Seconds
        /// </summary>
        private int minute = 0, second = 0;
        #endregion

        public UnityEvent onBeginRecord;
        public UnityEvent onEndRecord;

        #region Editor Exposed Variables
        [SerializeField]
        private float animToScale = 1f;
        [SerializeField]
        private float animFromScale = 1f;

        /// <summary>
        /// Set a keyboard key for recording
        /// </summary>
        [Tooltip("Set a keyboard key for recording")]
        public KeyCode keyCode;
        /// <summary>
        /// Show the Recording Time on the screen
        /// </summary>
        public TMP_Text RecordingTimeText;
        /// <summary>
        /// Set a Button to trigger recording
        /// </summary>
        public Button RecordButton;
        /// <summary>
        /// Record or Save Image for the Record Button
        /// </summary>
        public Image RecordImage, SaveImage;
        /// <summary>
        /// Set max duration in seconds
        /// </summary>
        [Tooltip("Set max duration in seconds")]
        public int timeToRecord = 30;
        /// <summary>
        /// Hold Button to Record
        /// </summary>
        [Tooltip("Press and Hold Record button to Record")]
        public bool holdToRecord = false;
        #endregion

        #region MonoBehaviour Callbacks
        private void Update()
        {
            if (Input.GetKeyDown(keyCode) && !holdToRecord)
            {
                if (isRecording)
                {
                    StopRecording();
                }
                else
                {
                    StartRecording();
                }
            }

            if (recordingTime >= timeToRecord)
            {
                StopRecording();
            }

            if (isRecording)
            {
                recordingTime += Time.deltaTime;

                minute = (int)(recordingTime / 60);
                second = (int)(recordingTime % 60);

                if (minute < 10)
                {
                    if (second < 10)
                    {
                        RecordingTimeText.text = "0" + minute + ":0" + second;
                    }
                    else
                    {
                        RecordingTimeText.text = "0" + minute + ":" + second;
                    }
                }
                else if (second < 10)
                {
                    RecordingTimeText.text = minute + ":0" + second;
                }
                else
                {
                    RecordingTimeText.text = minute + ":" + second;
                }

                RecordImage.gameObject.SetActive(false);
                SaveImage.gameObject.SetActive(true);
            }
            else
            {
                RecordingTimeText.text = "";
                RecordImage.gameObject.SetActive(true);
                SaveImage.gameObject.SetActive(false);
            }
        }
        #endregion

        #region Other Functions
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (!holdToRecord)
            {
                if (isRecording)
                {
                    StopRecording();
                }
                else
                {
                    StartRecording();
                }
            }
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (holdToRecord)
                StartRecording();
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (holdToRecord)
                StopRecording();
        }

        IEnumerator ScaleOverTime(GameObject button, float scaleFactor)
        {
            Vector3 originalScale = button.transform.localScale;
            Vector3 destinationScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

            float currentTime = 0.0f;

            do
            {
                button.transform.localScale = Vector3.Lerp(originalScale, destinationScale, currentTime / 0.5f);
                currentTime += Time.deltaTime;
                yield return null;
            }
            while (currentTime <= 1f);
        }
        #endregion

        #region Recorder Functions
        public void StartRecording()
        {
            recordingTime = 0f;
            isRecording = true;
            StartCoroutine(ScaleOverTime(RecordButton.gameObject, animToScale));
            onBeginRecord.Invoke();
        }

        public void StopRecording()
        {
            if (isRecording)
            {
                StartCoroutine(ScaleOverTime(RecordButton.gameObject, animFromScale));
                onEndRecord.Invoke();
                isRecording = false;
            }
        }
        #endregion
    }
}