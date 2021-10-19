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
    public Transform compass3d = null;


    public UnityEvent OnGPSLoaded = new UnityEvent();
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


    [Header("Debug Settings")]
    public Boolean ShowDebugConsole;

    [Header("Editor Mode simulator")]
    public double EdLatitude;
    public double EdLongitude;
    public double EdAltitude;
    [Range(0, 20)]
    public float MouseSensibility = 5f;


    [Header("Virtual Floor")]
    public bool ReceiveShadows = true;

    public static float compassAngle;

    public UnityEvent gpsEnabled = new UnityEvent();



    public bool isGPSAltitude = false;

    private string customLat = string.Empty;
    private string customLong = string.Empty;
    private bool useCustomCoords = false;
    public void SetCustomLat(string newLat)
    {
        customLat = newLat;
    }
    public void SetCustomLong(string newLong)
    {
        customLong = newLong;
    }




    private GameObject cameraContainer;


    // Gyro
    public Gyroscope gyro;
    private Quaternion rotation;

    // Camera
    private GameObject LAR_BackgroundCamera;
    private WebCamTexture cam;
    private RawImage background;
    private AspectRatioFitter fit;

    // general
    private bool arReady = false;
    public bool GPS = false;

    private Vector2 currentRotation;

    TargetActor androidUser = null;

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
        DontDestroyOnLoad(gameObject);

        // load debugUI
        // background = transform.Find("AR_BackgroundCamera").Find("UI_Background").Find("Background").gameObject.GetComponent<RawImage>();
        // fit = transform.Find("AR_BackgroundCamera").Find("UI_Background").Find("Background").gameObject.GetComponent<AspectRatioFitter>();


        androidUser = new TargetActor();

        if (Application.isEditor)
        {
            //set horizon visible on editor mode
            // UNLESS ShowLARCameraOnBackground (USING VUFORIA OR ARCORE)
            if (ShowLARCameraOnBackground)
                GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;



            gpsEnabled.Invoke();
            GPS = true;
            // Warn user to install NativeToolkit if its not
            bool classNativeToolkitExists = (null != Type.GetType("NativeToolkit"));
            if (!classNativeToolkitExists)
            {
                Debug.Log("Warning: please install NativeToolkit for better GPS precision!");
            }


            // COORDINATES
            _UserLat = EdLatitude;
            _UserLon = EdLongitude;
            _UserAlt = EdAltitude;

        }

        if (!Application.isEditor)
        {

            // Position Camera
            cameraContainer = new GameObject("Camera Container");
            cameraContainer.transform.position = transform.position;
            transform.SetParent(cameraContainer.transform);
            cameraContainer.transform.rotation = Quaternion.Euler(-90f, 90F, 0); //(90f, 0, 0);

            // check if we support Gyro
            if (!SystemInfo.supportsGyroscope)
            {
                Debug.Log("no Gyro");
            }

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
            rotation = new Quaternion(0, 0, 1, 0);

            // flag
            arReady = true;

            //GPS
            bool classNativeToolkitExists = (null != Type.GetType("NativeToolkit"));
            if (classNativeToolkitExists)
            {
                // START NativeToolkit GPS Interface
                GPS = true;
                gpsEnabled.Invoke();
            }


        }

        // SET USER HIGHT
        transform.position = new Vector3(0, BodyHeight, 0);
    }

    private void Update()
    {

        if (!Application.isEditor)
        {
            if (GPS)
            {
                if (Input.location.status == LocationServiceStatus.Failed)
                {
                    return;
                }
                else // if we have valid location services, run everything else
                {
                    OnGPSLoaded.Invoke();

                    // if we want to useCustomCoords then the setting will occur right away
                    if (!useCustomCoords)
                    {
                        _UserLat = Input.location.lastData.latitude;
                        _UserLon = Input.location.lastData.longitude;
                        _UserAlt = 0;//GetUserElevationData();
                    }
                    _UserCoords = new Vector2((float)_UserLon, (float)_UserLat);


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
                                //background.rectTransform.localScale = new Vector3(1f, scaleY, 1f);
                                int orient = -cam.videoRotationAngle;
                                //background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
                            }

                            //Update Gyro
                            transform.localRotation = gyro.attitude * rotation;

                        }
                    }


                    // make sure all UI fields match current data 
                    UpdateUI();
                    // finally update the position of the user's marker as well
                    OnlineMapsMarkerManager.instance.items[0].SetPosition(_UserLon, _UserLat);


                    // if we have added the user marker, handle its rotation
                    if (OnlineMapsMarkerManager.instance.items.Count > 0)
                    {
                        float _heading = Mathf.Round(Input.compass.trueHeading);
                        OnlineMapsMarkerManager.instance.items[0].rotationDegree = _heading;
                    }

                }

            }
        }


        // EDITOR
        //---------------------------
        if (Application.isEditor)
        {


            // Simulator mouse look
            if (Input.GetMouseButton(1))
            {
                currentRotation.x += Input.GetAxis("Mouse X") * MouseSensibility;
                currentRotation.y -= Input.GetAxis("Mouse Y") * MouseSensibility;
                currentRotation.x = Mathf.Repeat(currentRotation.x, 360);
                currentRotation.y = Mathf.Clamp(currentRotation.y, -80, 80);
                Camera.main.transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);
                compassAngle = currentRotation.x - 90;
                //compassRose.rotation = Quaternion.Euler(0, 0, compassAngle);
                compass3d.localRotation = Quaternion.Euler(0, compassAngle, 0);


            }


            _UserLat = EdLatitude;
            _UserLon = EdLongitude;
            _UserCoords = new Vector2((float)_UserLon, (float)_UserLat);

            // if we have added the user marker, handle its rotation
            if (OnlineMapsMarkerManager.instance.items.Count == 0)
                SetupMapAndMarker();
            else
                OnlineMapsMarkerManager.instance.items[0].rotationDegree = compassAngle;

        }
    }

    private void UpdateUI()
    {
        androidUser._Lat = _UserLat;
        androidUser._Lon = _UserLon;
        androidUser._Alt = _UserAlt;


        _UserCoords = new Vector2((float)_UserLon, (float)_UserLat);


    }

    ///<summary>Called from the UI</summary>
    public void ToggleElevationSource()
    {
        isGPSAltitude = !isGPSAltitude;
    }

    ///<summary>Handle which source to grab elevation data from</summary>
    public float GetUserElevationData()
    {
        if (isGPSAltitude && !Application.isEditor)
            return Input.location.lastData.altitude;
        else
            return OnlineMapsElevationManagerBase.GetUnscaledElevationByCoordinate(_UserLon, _UserLat);
    }

    ///<summary>While we don't yet have valid coordinate data, wait 1 second to see if we get it  </summary>
    public IEnumerator WaitForGPS()
    {
        // if we are running on the device, do the check
        if (!Application.isEditor)
        {
            while (_UserLat == 0)
            {
                yield return new WaitForSeconds(1);
            }
            SetupMapAndMarker();


        }

        if (Application.isEditor)
        {
            yield return new WaitForSeconds(1);
            SetupMapAndMarker();
        }
    }


    ///<summary>Once we have valid GPS data, set the map to the users location and add a marker </summary>
    private void SetupMapAndMarker()
    {
        // fire off the map update
        _map.SetPositionAndZoom(_UserLon, _UserLat, 18);
        // add the marker for our current position
        OnlineMapsMarkerManager.CreateItem(_UserCoords, "You");
    }




}
