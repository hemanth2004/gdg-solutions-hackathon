using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KG.StateMachine;

namespace ARLabs.Core
{
public class ApparatusSpawnUI : MonoBehaviour
{
    public string url;
    public int apparatusID;
    public Apparatus apparatus;
    public void StartPlaceApparatus()
    {
        ApparatusManager.Instance.CreateApparatus(apparatus.gameObject);
    }
    public void OpenApparatusWebPage()
    {
        if (IsValidUrl(url))
        {
            Application.OpenURL(url);
        }
        else { Debug.LogError("Invalid URL: " + url); }
    }

    private bool IsValidUrl(string url)
    {
            // Code to verify if URL is of AR Labs Website
            string pattern = "";
        return System.Uri.IsWellFormedUriString(url, System.UriKind.Absolute);
    }
}
}
