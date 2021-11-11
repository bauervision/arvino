using System;
using Newtonsoft.Json;

[System.Serializable]
public enum TargetType { DRONE, PERSON, VEHICLE, OBJECTIVE, ANDROID };

[System.Serializable]
public class TargetActor : ARVINOActor
{

    ///<summary> Distinguishes whether the UI will display: DRONE, PERSON, VEHICLE, ANDROID </summary>
    public int _Type;

    [JsonConstructor]
    public TargetActor() { }

    public TargetActor(TargetActor targetActor)
    {
        this._Alt = targetActor._Alt;
        this._Direction = targetActor._Direction;
        this._ID = targetActor._ID;
        this._Lat = targetActor._Lat;
        this._Lon = targetActor._Lon;
        this._Type = (int)targetActor._Type;
    }

    public TargetActor(TargetType type, double lat, double lng)
    {
        this._Alt = 0;
        this._ID = "+ New Target";
        this._Lat = lat;
        this._Lon = lng;
        this._Type = (int)type;// which index is the mesh we chose? 1 = person, 2 = vehicle, 3 = objective


    }

}