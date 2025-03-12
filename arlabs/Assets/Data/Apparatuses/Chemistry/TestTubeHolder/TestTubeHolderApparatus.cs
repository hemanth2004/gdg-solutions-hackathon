using UnityEngine;
using ARLabs.Core;

public class TestTubeHolderApparatus : Apparatus
{
    protected override void OnStart()
    {
        base.OnStart();


        _interactEvents.Add(new ApparatusOperation()
        {
            name = "Attach",
            targetApparatusType = typeof(TestTubeApparatus),
            actualTargetApparatus = null,
            operationEvent = (from, to) => AttachApparatusEvent(from, to)
        });
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
    }
}
