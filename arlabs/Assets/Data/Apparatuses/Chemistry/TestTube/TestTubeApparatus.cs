using UnityEngine;
using ARLabs.Core;
using ARLabs.Chemistry;

public class TestTubeApparatus : Apparatus
{
    [SerializeField] private Transform contentMesh;

    public float maxVolume = 55;
    public Matter content = null;

    protected override void OnStart()
    {
        base.OnStart();

        UpdateUI();
        UpdateVisual();

        _interactEvents.Add(new ApparatusOperation()
        {
            name = "Add 0.01g from Jar",
            targetApparatusType = typeof(JarApparatus),
            actualTargetApparatus = null,
            operationEvent = (from, to) => TransferFromJarToTestTube(from, to, 0.01f)
        });

        _interactEvents.Add(new ApparatusOperation()
        {
            name = "Add 0.1g from Jar",
            targetApparatusType = typeof(JarApparatus),
            actualTargetApparatus = null,
            operationEvent = (from, to) => TransferFromJarToTestTube(from, to, 0.1f)
        });

        _interactEvents.Add(new ApparatusOperation()
        {
            name = "Add 1g from Jar",
            targetApparatusType = typeof(JarApparatus),
            actualTargetApparatus = null,
            operationEvent = (from, to) => TransferFromJarToTestTube(from, to, 1f)
        });

        _interactEvents.Add(new ApparatusOperation()
        {
            name = "Add 10g from Jar",
            targetApparatusType = typeof(JarApparatus),
            actualTargetApparatus = null,
            operationEvent = (from, to) => TransferFromJarToTestTube(from, to, 10f)
        });

        _interactEvents.Add(new ApparatusOperation()
        {
            name = "Transfer 0.1g to Watchglass",
            targetApparatusType = typeof(WatchglassApparatus),
            actualTargetApparatus = null,
            operationEvent = (from, to) => TransferFromTestTubeToWatchglass(from, to, 0.1f)
        });

        _interactEvents.Add(new ApparatusOperation()
        {
            name = "Transfer 1g to Watchglass",
            targetApparatusType = typeof(WatchglassApparatus),
            actualTargetApparatus = null,
            operationEvent = (from, to) => TransferFromTestTubeToWatchglass(from, to, 1f)
        });

        _interactEvents.Add(new ApparatusOperation()
        {
            name = "Transfer 0.1g from Watchglass",
            targetApparatusType = typeof(WatchglassApparatus),
            actualTargetApparatus = null,
            operationEvent = (from, to) => TransferFromWatchglassToTestTube(to, from, 0.1f)
        });

        _interactEvents.Add(new ApparatusOperation()
        {
            name = "Transfer 1g from Watchglass",
            targetApparatusType = typeof(WatchglassApparatus),
            actualTargetApparatus = null,
            operationEvent = (from, to) => TransferFromWatchglassToTestTube(to, from, 1f)
        });
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

    private void TransferFromJarToTestTube(Apparatus from, Apparatus to, float amount)
    {
        if (to is JarApparatus jar && from is TestTubeApparatus testTube)
        {
            if (jar.content != null && jar.content.mass >= amount)
            {
                jar.content.mass -= amount;
                jar.content.volume -= jar.content.volume * (amount / jar.content.mass);
                if (testTube.content == null)
                {
                    testTube.content = new Matter
                    {
                        formula = jar.content.formula,
                        color = jar.content.color,
                        state = jar.content.state,
                        mass = amount,
                        volume = jar.content.volume * (amount / jar.content.mass), // Assuming density is constant
                        temperature = jar.content.temperature,
                        matterDetails = jar.content.matterDetails
                    };
                }
                else
                {
                    testTube.content.formula = jar.content.formula;
                    testTube.content.color = jar.content.color;
                    testTube.content.state = jar.content.state;
                    testTube.content.temperature = jar.content.temperature;
                    testTube.content.matterDetails = jar.content.matterDetails;
                    testTube.content.mass += amount;
                    testTube.content.volume += jar.content.volume * (amount / jar.content.mass); // Assuming density is constant
                }


                // Update UI and visuals
                testTube.UpdateUI();
                testTube.UpdateVisual();
                jar.UpdateUI();
                jar.UpdateVisual();
            }
            else
            {
                Debug.Log("Not enough matter in the jar to transfer.");
            }
        }
    }

    private void TransferFromTestTubeToWatchglass(Apparatus from, Apparatus to, float amount)
    {
        if (from is TestTubeApparatus testTube && to is WatchglassApparatus watchglass)
        {
            if (testTube.content != null && testTube.content.mass >= amount)
            {
                testTube.content.mass -= amount;
                float volumeTransferred = testTube.content.volume * (amount / testTube.content.mass); // Assuming density is constant

                if (watchglass.content == null)
                {
                    watchglass.content = new Matter
                    {
                        formula = testTube.content.formula,
                        color = testTube.content.color,
                        state = testTube.content.state,
                        mass = amount,
                        volume = volumeTransferred,
                        temperature = testTube.content.temperature,
                        matterDetails = testTube.content.matterDetails
                    };
                }
                else
                {
                    watchglass.content.formula = testTube.content.formula;
                    watchglass.content.color = testTube.content.color;
                    watchglass.content.state = testTube.content.state;
                    watchglass.content.temperature = testTube.content.temperature;
                    watchglass.content.matterDetails = testTube.content.matterDetails;
                    watchglass.content.mass += amount;
                    watchglass.content.volume += volumeTransferred;
                }


                // Update UI and visuals
                testTube.UpdateUI();
                testTube.UpdateVisual();
                watchglass.UpdateUI();
                watchglass.UpdateVisual();
            }
            else
            {
                Debug.Log("Not enough matter in the test tube to transfer.");
            }
        }
    }

    private void TransferFromWatchglassToTestTube(Apparatus from, Apparatus to, float amount)
    {
        if (from is WatchglassApparatus watchglass && to is TestTubeApparatus testTube)
        {
            if (watchglass.content != null && watchglass.content.mass >= amount)
            {
                watchglass.content.mass -= amount;
                float volumeTransferred = watchglass.content.volume * (amount / watchglass.content.mass); // Assuming density is constant

                if (testTube.content == null)
                {
                    testTube.content = new Matter
                    {
                        formula = watchglass.content.formula,
                        color = watchglass.content.color,
                        state = watchglass.content.state,
                        mass = amount,
                        volume = volumeTransferred,
                        temperature = watchglass.content.temperature,
                        matterDetails = watchglass.content.matterDetails
                    };
                }
                else
                {
                    testTube.content.mass += amount;
                    testTube.content.volume += volumeTransferred;
                }

                // Update UI and visuals
                testTube.UpdateUI();
                testTube.UpdateVisual();
                watchglass.UpdateUI();
                watchglass.UpdateVisual();
            }
            else
            {
                Debug.Log("Not enough matter in the watchglass to transfer.");
            }
        }
    }
}
