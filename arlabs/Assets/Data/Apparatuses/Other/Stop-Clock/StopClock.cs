using ARLabs.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ARLabs.Core;

public class StopClock : Apparatus
{
    [SerializeField] private TMPro.TMP_Text displayText;

    private bool isRunning = false;
    private float elapsedTime = 0f;

    protected override void OnStart()
    {
        base.OnStart();
        Fields.ButtonFields["start/stop"].OnChange += ToggleStartStop;
        Fields.ButtonFields["reset"].OnChange += ResetClock;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        if (isRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateClockDisplay();
        }
    }
    private void UpdateClockDisplay()
    {
        string formattedTime = FormatTime(elapsedTime);
        displayText.text = formattedTime;
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60F);
        int seconds = Mathf.FloorToInt(time % 60F);
        int tenths = Mathf.FloorToInt((time * 10F) % 10F);
        return string.Format("{0:00}:{1:00}.{2:0}", minutes, seconds, tenths);
    }

    private void ToggleStartStop(object tem)
    {
        isRunning = !isRunning;
    }

    private void ResetClock(object tem)
    {
        isRunning = false;
        elapsedTime = 0f;
        UpdateClockDisplay();
    }
}
