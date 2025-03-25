using UnityEngine;
using ARLabs.Core;

public class Visualization : MonoBehaviour
{
    private bool _isVisualizing = false;
    public bool IsVisualizing => _isVisualizing;


    public string VisualizationHead;
    public string VisualizationName;

    public virtual bool CheckIfVisualizationIsPossible()
    {
        return true;
    }

    public virtual void BeginVisualization()
    {
        _isVisualizing = true;
    }

    public virtual void EndVisualization()
    {
        _isVisualizing = false;
    }
}

