using UnityEngine;
using ARLabs.Core;
using ARLabs.Chemistry;

public class JarApparatus : Apparatus
{
    [SerializeField] private Transform contentMesh;

    public float maxVolume = 55;
    public Matter content;

    protected override void OnStart()
    {
        base.OnStart();

        UpdateUI();
        UpdateVisual();
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
            contentMesh.localScale = new Vector3(contentMesh.localScale.x,
                                    content.volume / maxVolume,
                                    contentMesh.localScale.z);

            contentMesh.GetChild(0).GetComponent<MeshRenderer>().material.color =
                new Color(content.color.r, content.color.g, content.color.b,
                         contentMesh.GetChild(0).GetComponent<MeshRenderer>().material.color.a);
        }
        else
        {
            contentMesh.localScale = new Vector3(contentMesh.localScale.x,
                                                0,
                                                contentMesh.localScale.z);
        }
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
    }


}
