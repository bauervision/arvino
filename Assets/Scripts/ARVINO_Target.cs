using UnityEngine;
using UnityEngine.UI;

using System;

using System.Collections;

public class ARVINO_Target : MonoBehaviour
{
    public static ARVINO_Target instance;

    #region Public Class members
    public float _headingToCamera;
    public Color _myTgtColor;
    public Color _inActiveTgtColor;
    public Color _ActiveTextColor;

    public bool _isListening = false;
    public double _Lat;
    public double _Lon;
    public double _Alt;
    public int _Detections;

    public string _ID;
    public bool _isActive = true;
    public string _Time = string.Empty;
    public Vector3 _Heading;


    #endregion

    #region UI related members
    public Transform parentCanvas = null;
    public GameObject targetRing = null;


    [Header("Target Coordinates")]
    public GameObject myTracker = null;// bounding 




    private Image _trackerImage;
    private GameObject myIndicator = null;// target ring indicator

    private Camera mainCam = null;
    private Text distanceText;
    private Text detectionsText;

    #endregion


    private float distanceToCamera = 0.0f;
    private double earthRadius = 6372797.560856f;

    private RectTransform _rect;
    private Vector2 myCoords;
    private Vector3 refVelocity = Vector3.zero;
    private AudioSource myAudioSource;

    private Coroutine runningCoroutine;


    ///<summary>When the file initial loads, find and create all the key required assets </summary>
    private void Awake()
    {
        targetRing = GameObject.Find("TargetRing");
        if (targetRing != null)
            myIndicator = Instantiate(Resources.Load<GameObject>("Indicator"), targetRing.transform);

        parentCanvas = GameObject.Find("UI_Canvas").GetComponent<Transform>();

        if (parentCanvas != null)
        {
            myTracker = Instantiate(Resources.Load<GameObject>("ModelTracker"), parentCanvas);
            _trackerImage = myTracker.GetComponent<Image>();
            distanceText = myTracker.transform.GetChild(1).gameObject.GetComponent<Text>();
            detectionsText = myTracker.transform.GetChild(2).gameObject.GetComponent<Text>();
            _rect = myTracker.GetComponent<RectTransform>();

        }
    }


    ///<summary>App has offically loaded, set the main camera </summary>
    private void Start()
    {
        mainCam = Camera.main;
        instance = this;
        myAudioSource = transform.GetComponent<AudioSource>();


    }

    ///<summary>Main update loop, determine whether we display the child mesh geometry for rendering, and position updates </summary>
    private void Update()
    {

        HandlePosUpdate();

        // only handle active target changes if we want to see them
        if (!UIManager.instance.alwaysActive)
            HandleActiveTargetState();
        else
        {
            _isActive = true;
            myTracker.GetComponent<Image>().color = _myTgtColor;
            myTracker.transform.GetChild(1).GetComponent<Text>().color = _ActiveTextColor;
            myTracker.transform.GetChild(2).GetComponent<Text>().color = _ActiveTextColor;
        }

        // set the detections, if any. 
        // note: if we have 1 detection, that means that 1 target has a 2nd target grouped with it.
        //therefore we actually show 2 instead of 1 
        detectionsText.text = (_Detections > 0) ? (_Detections + 1).ToString() : "";


        // if this target has been determined to be active
        if (_isActive)
        {
            // handle whether or not we show target geometry
            transform.GetChild(0).GetComponent<MeshRenderer>().enabled = UIManager.instance.displayGeometry;

            //if we have a tracker, and our color for whatever reason isnt set, set it
            if (myTracker != null && (myTracker.GetComponent<Image>().color != _myTgtColor))
            {
                myTracker.GetComponent<Image>().color = _myTgtColor;
                myTracker.transform.GetChild(1).GetComponent<Text>().color = new Color(1f, 0.8f, 0.4f, 1f);//acctive yellow
                myTracker.transform.GetChild(2).GetComponent<Text>().color = new Color(1f, 0.8f, 0.4f, 1f);//acctive yellow
            }

            if (myIndicator != null && (myIndicator.transform.GetChild(0).GetComponent<Image>().color != _myTgtColor))
                myIndicator.transform.GetChild(0).GetComponent<Image>().color = _myTgtColor;
        }
        else//otherwise it is inActive, meaning it hasn't been updated in 60 seconds
        {
            // never show geometry if inactive
            transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;

            // first make sure we even want to view inactive targets
            if (UIManager.instance.displayInActive)
            {
                // make sure we have a valid tracker first, then set it to grey as inactive
                if (myTracker != null && (myTracker.GetComponent<Image>().color != _inActiveTgtColor))
                {
                    myTracker.GetComponent<Image>().color = _inActiveTgtColor;
                    myTracker.transform.GetChild(1).GetComponent<Text>().color = _inActiveTgtColor;
                    myTracker.transform.GetChild(2).GetComponent<Text>().color = _inActiveTgtColor;
                }

                if (myIndicator != null && (myIndicator.transform.GetChild(0).GetComponent<Image>().color != _inActiveTgtColor))
                    myIndicator.transform.GetChild(0).GetComponent<Image>().color = _inActiveTgtColor;
            }
            else
            {
                HideMyTracker();
            }
        }

    }








    private void HandleActiveTargetState()
    {
        if (_Time != string.Empty)
        {
            long parsedTimestamp = long.Parse(_Time);
            // setup the 2 essential dates to compare
            DateTimeOffset currentDate = DateTimeOffset.Now;
            DateTimeOffset targetUpdateDate = DateTimeOffset.FromUnixTimeMilliseconds(parsedTimestamp).ToLocalTime();
            // find the timespan between them
            long elapsedTicks = currentDate.Ticks - targetUpdateDate.Ticks;
            TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);

            //now set the _isActive state
            _isActive = elapsedSpan.Minutes <= 1;


        }

    }



    ///<summary>Determine any position updates by interpolating where we are, with where we need to be</summary>
    private void HandlePosUpdate()
    {
        // get where the camera is located
        Vector3 coord = CalculateCameraPosUpdate();
        // update this target position
        transform.position = Vector3.SmoothDamp(transform.position, coord, ref refVelocity, 0.3f);
        HandleScreenPos();
    }

    ///<summary>Calculate the camera's 3d position based on GPS coordinates of the user</summary>
    private Vector3 CalculateCameraPosUpdate()
    {
        // make sure our vector 2 is updated
        SetPositionVectors();

        // handle all GPS coordinate updates
        double clat = _Lat - ARVINO_GPS.Instance._UserLat;
        clat = GetMetricCoordinates(earthRadius, clat);
        double clon = _Lon - ARVINO_GPS.Instance._UserLon;
        double radiusLat = Mathf.Cos((float)_Lat) * earthRadius;
        clon = GetMetricCoordinates(radiusLat, clon);

        // return the new vector3 for updating
        return new Vector3((float)clat, 0, (float)clon);
    }


    ///<summary>In order to quickly calculate distance in SURGE_Utils.DistanceBetweenPoints, set our Lat and Lon into a vector2 </summary>
    private void SetPositionVectors()
    {
        myCoords.y = (float)_Lat;
        myCoords.x = (float)_Lon;
    }

    ///<summary>Handle Target Indicator updates, and determine if this target is actually on the screen, if so, turn on the bounding box, otherwise we hide it</summary>
    private void HandleScreenPos()
    {
        // // always process the target ring indicator
        if (myIndicator != null)
            HandleTargetRingIndicatorUpdate();

        // get the heading of this target in relation to the camera
        _Heading = transform.position - mainCam.transform.position;
        _Heading.y = 0;

        // if this target is in view of the camera
        if (Vector3.Dot(mainCam.transform.forward, _Heading) > 0)
        {

            myTracker.SetActive(true);

            // process bounding box updates now that it is in view
            // find where this target is located in the camera view
            Vector3 trackerPos = mainCam.WorldToScreenPoint(this.transform.position);
            // update the bounding box to that screen position
            myTracker.transform.position = trackerPos;
            // figure out the distance from the camera
            HandleDistanceToCamera();

        }
        else
        {

            myTracker.SetActive(false);
        }

    }

    public double KilometerToMeter(double km)
    {
        double METER = 0;

        METER = km * 1000;

        return METER;
    }

    ///<summary>Simple conversion to get metric coordinates based on incoming GPS data</summary>
    private double GetMetricCoordinates(double radius, double angle)
    {
        double metrics = (radius / 180) * Mathf.PI;
        metrics *= angle;
        return metrics;
    }



    ///<summary>As long as the user wants to see the target ring, handle all calculations</summary>
    private void HandleTargetRingIndicatorUpdate()
    {
        // make sure we haven't lost myIndicator for whatever reason
        if (myIndicator == null)
            return;

        // if the user doesn't want to see the indicator, hide it and stop processing
        if (!UIManager.instance.showTargetRing)
        {
            myIndicator.SetActive(false);
            return;
        }

        if (UIManager.instance.showTargetRing && !myIndicator.activeInHierarchy)     // make sure we only turn it on, when it is not on, but the user wants it on
            myIndicator.SetActive(true);

        // begin processing all calculations

        // round the compass heading so we don't get as many updates, makes for a smoother experience
        float compassHeading = Mathf.Round(Input.compass.trueHeading);

        // get the angle based on our heading
        float angle = Mathf.Atan2(_Heading.z, _Heading.x) * Mathf.Rad2Deg;

        // handle 360 rotations for angle updates
        if (System.Math.Abs(Camera.main.transform.position.x - transform.position.x) > 360) angle = 360 - angle;

        float finalAngle;

        // if we are running on the device, use the actual compass
        if (!Application.isEditor)
            finalAngle = angle - compassHeading;
        else// otherwise use our generated one from the mouse
            finalAngle = angle - ARVINO_GPS.compassAngle;

        // make the update
        myIndicator.transform.localRotation = Quaternion.Slerp(myIndicator.transform.localRotation, Quaternion.Euler(0, 0, finalAngle), Time.deltaTime * 2f);

    }

    ///<summary>When this target is visible on screen, process the distance for rendering on the text field</summary>
    private void HandleDistanceToCamera()
    {
        // get distance between us and the camera
        distanceToCamera = ARVINO_Utils.DistanceBetweenPoints(myCoords, ARVINO_GPS.Instance._UserCoords).magnitude;
        //float meterDist = distanceToCamera;

        distanceToCamera = (float)System.Math.Round(distanceToCamera, 2);
        // if 1 kilometer or greater, use "km"
        if (distanceToCamera >= 1.0f)
            distanceText.text = (UIManager.instance.displayBox) ? distanceToCamera.ToString() + "km" : "";
        else // less than 1 kilometer, so switch to meters
            distanceText.text = (UIManager.instance.displayBox) ? ((float)System.Math.Round(KilometerToMeter(distanceToCamera), 2)).ToString() + "m" : "";

        float closeScale = 0.4f;
        float midScale = 0.3f;
        float farScale = 0.2f;

        // now let's handle how the tracker icon ( bounding box ) looks based on its distance to the user
        // we will change it's opacity and border thickness depending
        if (distanceToCamera >= 0.5f)
        {
            _myTgtColor.a = 0.3f;
            _trackerImage.color = _myTgtColor;
            myIndicator.transform.GetChild(0).localScale = new Vector3(farScale, farScale, farScale);
        }
        else if (distanceToCamera < 0.5f)
        {
            if (distanceToCamera >= 0.25)
            {
                _myTgtColor.a = 0.5f;
                _trackerImage.color = _myTgtColor;
                myIndicator.transform.GetChild(0).localScale = new Vector3(midScale, midScale, midScale);
            }
            else // within 0.25km of user
            {
                _myTgtColor.a = 1f;
                _trackerImage.color = _myTgtColor;
                myIndicator.transform.GetChild(0).localScale = new Vector3(closeScale, closeScale, closeScale);
            }
        }

        // finally handle bounding box vs vertical line
        _rect.sizeDelta = (UIManager.instance.displayBox) ? new Vector2(200, 200) : new Vector2(10, 200);
    }

    ///<summary>Called whenver we need to toggle the visual display of the bounding box tracker </summary>
    public void ToggleMyTracker()
    {
        myTracker.GetComponent<Image>().enabled = UIManager.instance.displayGeometry;
    }

    public void HideMyTracker()
    {
        if (myTracker.GetComponent<Image>().enabled == true)
        {
            myTracker.GetComponent<Image>().enabled = false;
            myTracker.transform.GetChild(1).gameObject.SetActive(false);
            myTracker.transform.GetChild(2).gameObject.SetActive(false);
            myIndicator.transform.GetChild(0).GetComponent<Image>().enabled = false;
        }
    }

    public void ShowMyTracker()
    {
        if (myTracker.GetComponent<Image>().enabled == false)
        {
            myTracker.GetComponent<Image>().enabled = true;
            myTracker.transform.GetChild(1).gameObject.SetActive(true);
            myTracker.transform.GetChild(2).gameObject.SetActive(true);
            myIndicator.transform.GetChild(0).GetComponent<Image>().enabled = true;
        }
    }


    /// <summary>
    /// UpdatePosition is called from Target Manager/ SetTargetData, when new data updates come in
    /// </summary>
    public void UpdatePosition(ARVINOActor actor)
    {
        if (myTracker != null)
            myTracker.GetComponent<Image>().enabled = !UIManager.instance.displayGeometry;

        // run through some simple checks to make sure we dont update needlessly
        bool positionNeedsUpdate = false;
        if (actor._Lon != _Lon)
        {
            _Lon = (float)actor._Lon;
            positionNeedsUpdate = true;
        }

        if (actor._Lat != _Lat)
        {
            _Lat = (float)actor._Lat;
            positionNeedsUpdate = true;
        }

        _Time = actor._Time;
        //TODO: re-implement altitude once we resolve the issue
        // if (actor._Alt != _Alt)
        // {
        //     _Alt = OnlineMapsElevationManagerBase.GetUnscaledElevationByCoordinate(actor._Lon, actor._Lat);
        // }


        if (positionNeedsUpdate)
            SetPositionVectors();

        _Detections = actor._Detections;


    }


    ///<summary>When this target data is loaded, set its data, bounding box and target ring indicator</summary>
    public void SetInitialPosition(ARVINOActor actor)
    {
        // set all of my surge data right away
        float alt = OnlineMapsElevationManagerBase.GetUnscaledElevationByCoordinate(actor._Lon, actor._Lat);

        _Alt = 0;
        // _Alt = (Application.isEditor) ? -alt : alt;// negative because of the flipped view in the editor
        _ID = actor._ID;
        _Lat = actor._Lat;
        _Lon = actor._Lon;
        _Time = actor._Time;
        _Detections = actor._Detections;

        // my vector 2 for ease of use when we need to determine distance
        myCoords.x = (float)actor._Lon;
        myCoords.y = (float)actor._Lat;

        /* Only useful for testing in the editor:
        if (myTracker != null)
             myTracker.name = $"TgtTracker:{type}";
        */

        HandleTargetRingIndicatorUpdate();
    }

    ///<summary>Called from TargetManager: will handle the removal of this target's gameobject, bounding box tracker, and target ring indicator </summary>
    public void RemoveTarget()
    {
        DestroyImmediate(myTracker);
        DestroyImmediate(myIndicator);
        DestroyImmediate(gameObject);
    }






}
