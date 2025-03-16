using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ARLabs.Core;

public class PendulumApparatus : Apparatus
{
    [SerializeField] private LineRenderer threadLine;
    [SerializeField] private Transform clamp, bob, visualBall;

    [SerializeField] private float hookLength = 1f;  // 1cm hook length
    [SerializeField] private float bobRadius = 2f;   // 2cm bob radius
    private float effectiveLength;  // Total length including hook and bob radius

    private float currentAngle;      // Current angle in radians
    private float angularVelocity;   // Angular velocity in radians/second
    private float length;            // Length in meters
    private float gravity;           // Gravity in m/s²
    private bool isOscillating = true;
    private float period;

    protected override void OnStart()
    {
        base.OnStart();

        Fields.TextFields["hookLength"].isReadOnly = true;
        Fields.TextFields["hookLength"].value = hookLength.ToString();

        Fields.SliderFields["length"].OnChange += OnChangeLength;
        Fields.SliderFields["gravity"].OnChange += OnChangeGravity;
        Fields.SliderFields["angle"].OnChange += OnChangeAngle;
        Fields.ButtonFields["reset"].OnChange += OnClickResetOscillation;
        Fields.SliderFields["bobRadius"].OnChange += OnChangeBobRadius;

        InitializePendulum();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (isOscillating)
        {
            if (Fields.SliderFields["angle"].value != 0)
            {
                UpdatePendulum(Time.deltaTime);
                // Update the rotation of the bob
                Vector3 up = clamp.position - bob.position;
                bob.localRotation = Quaternion.LookRotation(bob.forward, up);
            }
            else
            {
                // When angle is 0, keep bob looking straight up
                bob.localRotation = Quaternion.identity;
            }
        }

        // Update the line renderer
        threadLine.SetPosition(0, clamp.position - threadLine.transform.position);
        threadLine.SetPosition(1, bob.position - threadLine.transform.position);
    }

    private void InitializePendulum()
    {
        // All measurements are in cm, convert to meters since Unity units are in meters
        float measurementLength = Fields.SliderFields["length"].value * 0.01f;
        effectiveLength = measurementLength + (bobRadius * 0.01f) + (hookLength * 0.01f);

        // Get gravity value from slider field
        gravity = Fields.SliderFields["gravity"].value;

        // Calculate period using real-world units (meters)
        period = 2 * Mathf.PI * Mathf.Sqrt(effectiveLength / gravity);
        isOscillating = true;  // Ensure oscillation is enabled when initializing
        ResetPendulum();
    }

    private void ResetPendulum()
    {
        currentAngle = Fields.SliderFields["angle"].value * Mathf.Deg2Rad;
        angularVelocity = 0;
        UpdateBobPosition();
    }

    private void UpdateBobPosition()
    {
        // Use measured length for visual positioning since bob is pivoted at center
        float measurementLength = Fields.SliderFields["length"].value * 0.01f;

        // Position using only the string length (in meters)
        float x = measurementLength * Mathf.Sin(currentAngle);
        float y = -measurementLength * Mathf.Cos(currentAngle);

        Vector3 newPosition = clamp.position + new Vector3(x, y, 0);
        bob.position = newPosition;

        // Update string position
        threadLine.SetPosition(0, clamp.position - threadLine.transform.position);
        threadLine.SetPosition(1, newPosition - threadLine.transform.position);
    }

    private void UpdatePendulum(float deltaTime)
    {
        // Use effective length for physics calculations
        float angularAcceleration = -(gravity / effectiveLength) * Mathf.Sin(currentAngle);

        angularVelocity += angularAcceleration * deltaTime;
        currentAngle += angularVelocity * deltaTime;

        UpdateBobPosition();
    }

    private void OnChangeLength(object value) => InitializePendulum();
    private void OnChangeGravity(object value) => InitializePendulum();
    private void OnChangeAngle(object value) => InitializePendulum();
    private void OnChangeBobRadius(object value)
    {
        bobRadius = (float)value;
        // Scale the bob visual in meters (Unity units)
        bob.transform.localScale = Vector3.one * (bobRadius * 0.01f * 2);
        InitializePendulum();
    }

    public void OnClickResetOscillation(object ignore)
    {
        // Reset angle to initial value from slider
        currentAngle = Fields.SliderFields["angle"].value * Mathf.Deg2Rad;
        // Reset velocity to zero
        angularVelocity = 0f;
        // Update position
        UpdateBobPosition();
    }
}
