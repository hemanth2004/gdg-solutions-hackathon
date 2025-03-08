using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatteryOn : PhysicalInteraction
{
    [SerializeField] private float swipeThreshold;
    private BatteryScript batteryScript;

    private void Awake()
    {
        batteryScript = transform.parent.GetComponent<BatteryScript>();
    }
    protected override void OnFinishInteraction()
    {
        DetectSwipe();
    }

    private void DetectSwipe()
    {
        float swipeDistance = (touchEnd - touchStart).magnitude;

        if (swipeDistance >= swipeThreshold)
        {
            Vector2 swipeDirection = touchEnd - touchStart;
            Debug.Log(swipeDirection);
                if (swipeDirection.y > 0)
                {
                    batteryScript.on = true;
                }
                else if (swipeDirection.y < 0)
                {
                    batteryScript.on = false;
                }
            
        }
    }
}
