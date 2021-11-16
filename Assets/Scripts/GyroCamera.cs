using UnityEngine;
using System.Collections;

public class GyroCamera : MonoBehaviour
{
    // STATE
    public static float _initialYAngle = 0f;
    private float _appliedGyroYAngle = 0f;
    private float _calibrationYAngle = 0f;
    private Transform _rawGyroRotation;
    private float _tempSmoothing;

    // SETTINGS
    [SerializeField] private float _smoothing = 0.1f;

    private IEnumerator Start()
    {
        Input.gyro.enabled = true;
        Application.targetFrameRate = 60;

        int currentCamRotation = Mathf.RoundToInt(Camera.main.transform.rotation.eulerAngles.y);
        int compassHeading = Mathf.RoundToInt(Input.compass.trueHeading);
        compassHeading = (compassHeading > 360) ? compassHeading - 360 : (compassHeading < 0) ? compassHeading + 360 : compassHeading;

        _initialYAngle = currentCamRotation - compassHeading;

        _rawGyroRotation = new GameObject("GyroRaw").transform;
        _rawGyroRotation.position = transform.position;
        _rawGyroRotation.rotation = transform.rotation;

        // Wait until gyro is active, then calibrate to reset starting rotation.
        yield return new WaitForSeconds(1);

        StartCoroutine(CalibrateYAngle());
    }

    private void Update()
    {
        if (!Application.isEditor)
        {
            ApplyGyroRotation();



        }
    }
    private void LateUpdate()
    {
        if (!Application.isEditor)
        {
            ApplyCalibration();
            transform.rotation = Quaternion.Slerp(transform.rotation, _rawGyroRotation.rotation, _smoothing);
        }
    }

    private IEnumerator CalibrateYAngle()
    {
        _tempSmoothing = _smoothing;
        _smoothing = 1;
        _calibrationYAngle = _appliedGyroYAngle - _initialYAngle; // Offsets the y angle in case it wasn't 0 at edit time.
        yield return null;
        _smoothing = _tempSmoothing;
    }

    private void ApplyGyroRotation()
    {
        _rawGyroRotation.rotation = Input.gyro.attitude;
        _rawGyroRotation.Rotate(0f, 0f, 180f, Space.Self); // Swap "handedness" of quaternion from gyro.
        _rawGyroRotation.Rotate(90f, 180f, 0f, Space.World); // Rotate to make sense as a camera pointing out the back of your device.

        _appliedGyroYAngle = _rawGyroRotation.eulerAngles.y; // Save the angle around y axis for use in calibration.
    }

    private int GetCompassAngle()
    {
        int compassHeading = Mathf.RoundToInt(Input.compass.trueHeading);
        return (compassHeading >= 360) ? compassHeading - 360 : (compassHeading < 0) ? compassHeading + 360 : compassHeading;
        //return Mathf.RoundToInt(Input.compass.trueHeading);
    }
    private void ApplyCalibration()
    {
        int currentCamRotation = Mathf.RoundToInt(Camera.main.transform.rotation.eulerAngles.y);

        _calibrationYAngle = currentCamRotation - ARVINO_GPS.compassAngle;
        _rawGyroRotation.Rotate(0f, -_calibrationYAngle, 0f, Space.World); // Rotates y angle back however much it deviated when calibrationYAngle was saved.
    }


}