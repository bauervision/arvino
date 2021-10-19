using System;
using Newtonsoft.Json;

[System.Serializable]
public enum TargetType { DRONE, PERSON, VEHICLE, OBJECTIVE, ANDROID };

[System.Serializable]
public class TargetActor : ARVINOActor
{

    ///<summary> The active drone that has detected and is actively tracking this target </summary>
    public string _Drone;
    ///<summary>Is this target grouped together with several co-located detections? </summary>

    ///<summary> Distinguishes whether the UI will display: DRONE, PERSON, VEHICLE, ANDROID </summary>
    public int _Type;

    [JsonConstructor]
    public TargetActor() { }

    public TargetActor(TargetActor targetActor)
    {
        this._Alt = targetActor._Alt;
        this._Detections = targetActor._Detections;
        this._Dir = targetActor._Dir;
        this._Drone = targetActor._Drone;
        this._ID = targetActor._ID;
        this._isActive = targetActor._isActive;
        this._Lat = targetActor._Lat;
        this._Lon = targetActor._Lon;
        this._Time = targetActor._Time;
        this._Type = (int)targetActor._Type;
    }

    public TargetActor(TargetType type, double lat, double lng)
    {
        this._Alt = 0;
        this._Detections = 0;
        this._Dir = 0;
        this._Drone = "na";
        this._ID = "+ New Target";
        this._isActive = true;
        this._Lat = lat;
        this._Lon = lng;
        this._Type = (int)type;// which index is the mesh we chose? 1 = person, 2 = vehicle, 3 = objective
        this._Time = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds().ToString();

    }

}