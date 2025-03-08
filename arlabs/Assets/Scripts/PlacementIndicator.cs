using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlacementIndicator : MonoBehaviour
{
    private GameObject _apparatusMesh;

    [SerializeField] private float rotateAmt = 10f;
    public GameObject PlacementMesh
    {
        get { return _apparatusMesh; }
        set
        {
            _apparatusMesh = value;
            Destroy(transform.GetChild(0).gameObject);
            GameObject apparatusMeshObject = Instantiate(value, transform);

            MeshRenderer[] meshRenderers = apparatusMeshObject.GetComponentsInChildren<MeshRenderer>(true);

            foreach (MeshRenderer renderer in meshRenderers)
            {
                if(renderer.gameObject.GetComponent<TextMeshPro>() ==  null)
                    renderer.material = transparentMat;
            }

            indicator = apparatusMeshObject.transform;
        }

    }
    public Transform indicator;
   

    private ARRaycastManager raycastManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    [SerializeField] private Material transparentMat;

    void Start()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();
    }

    void Update()
    {
        var ray = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if(raycastManager.Raycast(ray, hits, TrackableType.Planes))
        {
            Pose hitPose = hits[0].pose;

            indicator.position = hitPose.position;

            if(!indicator.gameObject.activeInHierarchy)
            {
                indicator.gameObject.SetActive(true);
            }
        }
    }

    public void RotatePlacementIndicator(int dir)
    {
        indicator.eulerAngles += Vector3.up * dir * rotateAmt;
    }

    private void OnDisable()
    {
        transform.rotation = Quaternion.identity;
        transform.position = Vector3.zero;
    }
}
