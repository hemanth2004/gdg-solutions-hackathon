using UnityEngine;
using ARLabs.Core;
using ARLabs.Chemistry;

public class WatchglassApparatus : Apparatus
{
    public Matter content = null;
    public float maxVolume = 10f;
    public Transform contentMesh;

    protected override void OnStart()
    {
        base.OnStart();

        UpdateUI();
        UpdateVisual();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
    }

    public void UpdateUI()
    {
        if (content != null && (content.volume > 0 || content.mass > 0))
        {
            Fields.TextFields["content"].value = content.formula;
            Fields.TextFields["state"].value = content.state.ToString();
            Fields.TextFields["volume"].value = "" + content.volume;
            Fields.TextFields["mass"].value = "" + content.mass;
            Fields.TextFields["temperature"].value = "" + content.temperature;
        }
        else
        {
            Fields.TextFields["content"].value = "Empty";
            Fields.TextFields["state"].value = "Empty";
            Fields.TextFields["volume"].value = "0";
            Fields.TextFields["mass"].value = "0";
            Fields.TextFields["temperature"].value = "0";
        }
    }

    public void UpdateVisual()
    {
        if (content != null)
        {
            contentMesh.localScale = new Vector3(content.volume / maxVolume,
                                    content.volume / maxVolume,
                                    content.volume / maxVolume);

            contentMesh.GetChild(0).GetComponent<MeshRenderer>().material.color =
                new Color(content.color.r, content.color.g, content.color.b,
                         contentMesh.GetChild(0).GetComponent<MeshRenderer>().material.color.a);
        }
        else
        {
            contentMesh.localScale = new Vector3(0,
                                                0,
                                                0);
        }
    }
}
