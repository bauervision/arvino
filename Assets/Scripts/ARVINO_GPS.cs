using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using UnityEngine.Events;


public class ARVINO_GPS : MonoBehaviour
{

    public static ARVINO_GPS Instance { set; get; }

    #region UI Related
    public OnlineMaps _map;

    #endregion


    #region Class Members
    [Header("User")]
    public float BodyHeight = 1.7f;

    [Header("Actual Location")]
    [Tooltip("Read Only latitude in runtime")]

    public double _UserLat;
    [Tooltip("Read Only longitude in runtime")]

    public double _UserLon;

    [Tooltip("Read Only altitude in runtime")]
    public double _UserAlt;

    public Vector2 _UserCoords;



    [Header("Background Camera")]
    public Boolean ShowLARCameraOnBackground = true;


    [Header("Editor Mode simulator")]
    public double EdLatitude;
    public double EdLongitude;
    public double EdAltitude;
    [Range(0, 20)]
    public float MouseSensibility = 5f;

    [Header("Status Panel")]
    public Text LatitudeText;
    public Text LongitudeText;
    public Text HorizonalAccuracyText;
    public Text VerticalAccuracyText;
    public Text AltitudeText;
    public Text HeadingText;


    public static float compassAngle;

    public bool isGPSAltitude = false;

    private GameObject cameraContainer;


    // Gyro
    public Gyroscope gyro;
    public Quaternion rotation;

    // Camera
    private GameObject LAR_BackgroundCamera;
    private WebCamTexture cam;
    private RawImage background;
    private AspectRatioFitter fit;

    // general
    private bool arReady = false;
    public bool GPS = false;

    public Vector2 currentRotation;


    public Text CompassHeadingText;
    public Text CameraRotationText;
    public Text InitialValueText;


    #endregion

    private void Awake()
    {
        if (!Application.isEditor)
        {
            // ask user permission to use the GPS
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
                Permission.RequestUserPermission(Permission.FineLocation);

            // ask user permission to use the Camera
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
                Permission.RequestUserPermission(Permission.Camera);
        }

    }




    private void Start()
    {
        Instance = this;

        // load debugUI
        background = transform.Find("AR_BackgroundCamera").Find("UI_Background").Find("Background").gameObject.GetComponent<RawImage>();
        fit = transform.Find("AR_BackgroundCamera").Find("UI_Background").Find("Background").gameObject.GetComponent<AspectRatioFitter>();





        if (Application.isEditor)
        {
            //Position Camera
            // cameraContainer = new GameObject("Camera Container");
            // cameraContainer.transform.position = transform.position;
            // transform.SetParent(cameraContainer.transform);
            // cameraContainer.transform.rotation = Quaternion.Euler(-90f, 90f, 0); //(90f, 0, 0);

            if (ShowLARCameraOnBackground)
                GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;

            GPS = true;

            // COORDINATES
            _UserLat = EdLatitude;
            _UserLon = EdLongitude;
            _UserAlt = EdAltitude;

        }

        if (!Application.isEditor)
        {
            // BACKGROUND CAMERA
            if (ShowLARCameraOnBackground)
            {
                // turn on RawImage
                background.GetComponent<RawImage>().enabled = true;

                // check if we support cam
                for (int i = 0; i < WebCamTexture.devices.Length; i++)
                {
                    if (!WebCamTexture.devices[i].isFrontFacing)
                    {
                        cam = new WebCamTexture(WebCamTexture.devices[i].name, Screen.width, Screen.height);
                        break;
                    }
                }

                if (cam == null)
                {
                    Debug.Log("no back Camera");
                    cam = new WebCamTexture(WebCamTexture.devices[1].name, Screen.width, Screen.height);
                    //return;
                }

                cam.Play();
                background.texture = cam;
            }

            if (!ShowLARCameraOnBackground)
            {
                background.enabled = false;
                fit.enabled = false;
            }

            // enable gyro
            gyro = Input.gyro;
            gyro.enabled = true;

            // flag
            arReady = true;
        }


        // SET USER HIGHT
        transform.position = new Vector3(0, BodyHeight, 0);
    }


    private void Update()
    {

        if (!Application.isEditor)
        {
            if (Input.location.status != LocationServiceStatus.Failed)
            {
                _UserLat = Input.location.lastData.latitude;
                _UserLon = Input.location.lastData.longitude;

                HorizonalAccuracyText.text = Input.location.lastData.horizontalAccuracy.ToString();
                VerticalAccuracyText.text = Input.location.lastData.verticalAccuracy.ToString();
                LatitudeText.text = _UserLat.ToString();
                LongitudeText.text = _UserLon.ToString();
                AltitudeText.text = Input.location.lastData.altitude.ToString();

                _UserAlt = 0;//GetUserElevationData();
                _UserCoords = new Vector2((float)_UserLon, (float)_UserLat);

                float smooth = 0.1f;
                float yVelocity = 0.1f;
                compassAngle = Mathf.SmoothDampAngle(compassAngle, Mathf.RoundToInt(Input.compass.trueHeading), ref yVelocity, smooth);

                HeadingText.text = compassAngle.ToString();
                // if we dont want to see the map, show camera things
                if (!UIManager.instance.displayMap)
                {
                    if (arReady)
                    {
                        if (ShowLARCameraOnBackground)
                        {
                            //update Camera
                            float ratio = (float)cam.width / (float)cam.height;
                            fit.aspectRatio = ratio;

                            float scaleY = cam.videoVerticallyMirrored ? -1.0f : 1.0f;
                            background.rectTransform.localScale = new Vector3(1f, scaleY, 1f);
                            int orient = -cam.videoRotationAngle;
                            background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
                        }
                        //
                    }
                }
            }
        }


        if (Application.isEditor)
        {
            // Simulator mouse look
            if (Input.GetMouseButton(1))
            {
                currentRotation.x += Input.GetAxis("Mouse X") * MouseSensibility;
                currentRotation.y -= Input.GetAxis("Mouse Y") * MouseSensibility;
                currentRotation.x = Mathf.Repeat(currentRotation.x, 360);
                currentRotation.y = Mathf.Clamp(currentRotation.y, -80, 80);
                compassAngle = currentRotation.x;
            }

            _UserLat = EdLatitude;
            _UserLon = EdLongitude;
            _UserCoords = new Vector2((float)_UserLon, (float)_UserLat);

        }

        if (_UserLat != 0 && _UserLon != 0)
            HandleUserMarkerAndPosition();


        //Quaternion cameraRotation;
        if (!Application.isEditor)
        {
            int currentCamRotation = Mathf.RoundToInt(Camera.main.transform.rotation.eulerAngles.y);
            CameraRotationText.text = currentCamRotation.ToString();
            CompassHeadingText.text = compassAngle.ToString();

            InitialValueText.text = (currentCamRotation - compassAngle).ToString();
        }


        if (Application.isEditor)
        {
            transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);
        }


    }


    private void HandleUserMarkerAndPosition()
    {
        // if we have added the user marker, handle its rotation
        if (OnlineMapsMarkerManager.instance.items.Count == 0)
        {
            SetupMapAndMarker();
            OnlineMapsMarkerManager.instance.items[0].SetPosition(_UserLon, _UserLat);
        }
        else
            OnlineMapsMarkerManager.instance.items[0].rotationDegree = compassAngle;
    }


    ///<summary>Handle which source to grab elevation data from</summary>
    public float GetUserElevationData()
    {
        if (isGPSAltitude && !Application.isEditor)
            return Input.location.lastData.altitude;
        else
            return OnlineMapsElevationManagerBase.GetUnscaledElevationByCoordinate(_UserLon, _UserLat);
    }


    ///<summary>Once we have valid GPS data, set the map to the users location and add a marker </summary>
    private void SetupMapAndMarker()
    {
        // fire off the map update
        _map.SetPositionAndZoom(_UserLon, _UserLat, 18);
        // add the marker for our current position
        OnlineMapsMarkerManager.CreateItem(_UserCoords, "You");
    }


    public void RefreshCoords()
    {
        _map.SetPositionAndZoom(_UserLon, _UserLat, 18);
        OnlineMapsMarkerManager.instance.items[0].SetPosition(_UserLon, _UserLat);
        // hide the status window
        UIManager.instance.ToggleStatusWindow();
        // and show the updated map
        UIManager.instance.ToggleMap();
    }



}
