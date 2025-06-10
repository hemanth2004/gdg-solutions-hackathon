using UnityEngine;

namespace ARLabs.AI
{
    public class ActionHandler : MonoBehaviour
    {
        public static bool HandleAction(string actionText)
        {
            // Parse actionText into XML/JSON or other format
            // First figure out whether action is visualization or set_field
            // then parse the rest into either
            //      1. VisualizationAction object OR
            //      2. ApparatusSetAction object
            //
            // and assign to IAction action;
            // then call action.Execute();
            // 

            return true;
        }
    }

    public interface IAction
    {
        bool Execute();
    }
}
