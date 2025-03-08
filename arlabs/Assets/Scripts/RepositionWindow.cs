using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class RepositionWindow : MonoBehaviour
{
    private Vector3 originalPos, originalRot;

    private Transform indicator;

    [SerializeField] private float rotateAmt = 10f;

    private ARRaycastManager raycastManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();


    private void Start()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();
    }

    private void Update()
    {
        if (indicator == null)
            return;

        var ray = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (raycastManager.Raycast(ray, hits, TrackableType.Planes))
        {
            Pose hitPose = hits[0].pose;

            indicator.position = hitPose.position;

            if (!indicator.gameObject.activeInHierarchy)
            {
                indicator.gameObject.SetActive(true);
            }
        }
    }

    public void RotateApparatus(int dir)
    {
        indicator.eulerAngles += Vector3.up * dir * rotateAmt;
    }

    public void StartReposition(Transform t)
    {
        indicator = t;
        originalPos = indicator.position;
        originalRot = indicator.eulerAngles;

        indicator.GetComponent<Outline>().enabled = true;
    }
    public void EndReposition()
    {
        indicator = null;
    }
    public void CancelReposition()
    {   
        indicator.position = originalPos;
        indicator.rotation = Quaternion.Euler(originalRot);

        indicator = null;
    }
}
