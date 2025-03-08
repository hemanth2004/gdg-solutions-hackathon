using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ARLabs.Core;

public class PendulumApparatus : Apparatus
{
    [SerializeField] private float lengthScale = 0.02f; // 0.02 unity units = 5cm
    [SerializeField] private LineRenderer threadLine;
    [SerializeField] private Transform clamp, bob;

    private float length; // actual length of the pendulum in meters
    private float gravity; // gravitational acceleration
    private float angle; // initial angle in radians
    private float angularVelocity; // angular velocity for the pendulum
    private bool isOscillating = true;

    protected override void OnStart()
    {
        base.OnStart();

        Fields.SliderFields["length"].OnChange += OnChangeLength;
        Fields.SliderFields["gravity"].OnChange += OnChangeGravity;
        Fields.SliderFields["angle"].OnChange += OnChangeAngle;
        Fields.ButtonFields["stop"].OnChange += OnClickStopOscillation;

        length = Fields.SliderFields["length"].value;
        gravity = Fields.SliderFields["gravity"].value;
        angle = Mathf.Deg2Rad * Fields.SliderFields["angle"].value;

        InitializePendulum();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (isOscillating)
        {
            UpdatePendulum(Time.deltaTime);
        }

        threadLine.SetPosition(0, clamp.position - threadLine.transform.position);
        threadLine.SetPosition(1, bob.position - threadLine.transform.position);


        Vector3 up = clamp.position - bob.position;
        bob.localRotation = Quaternion.LookRotation(bob.forward, up);
    }

    private void OnChangeLength(object value)
    {
        length = (float)value;
        InitializePendulum();
    }

    private void OnChangeGravity(object value)
    {
        gravity = (float)value;
        InitializePendulum();
    }

    private void OnChangeAngle(object value)
    {
        angle = Mathf.Deg2Rad * (float)value;
        InitializePendulum();
    }

    public void OnClickStopOscillation(object ignore)
    {
        isOscillating = false;
    }

    private void InitializePendulum()
    {
        if (length <= 0 || float.IsNaN(length) || float.IsNaN(angle) || float.IsNaN(gravity))
        {
            Debug.LogError("Invalid pendulum parameters. Initialization aborted.");
            return;
        }

        float scaledLength = length * lengthScale;
        float x = clamp.position.x + scaledLength * Mathf.Sin(angle);
        float y = clamp.position.y - scaledLength * Mathf.Cos(angle);
        bob.position = new Vector3(x, y, clamp.position.z);
        angularVelocity = 0;
    }

    private void UpdatePendulum(float deltaTime)
    {
        if (length <= 0 || float.IsNaN(length) || float.IsNaN(angle) || float.IsNaN(gravity))
        {
            Debug.LogError("Invalid pendulum parameters. Update aborted.");
            return;
        }

        float angularAcceleration = -(gravity / length) * Mathf.Sin(angle);
        angularVelocity += angularAcceleration * deltaTime;
        angle += angularVelocity * deltaTime;

        float scaledLength = length * lengthScale;
        float x = clamp.position.x + scaledLength * Mathf.Sin(angle);
        float y = clamp.position.y - scaledLength * Mathf.Cos(angle);
        bob.position = new Vector3(x, y, clamp.position.z);
    }
}
