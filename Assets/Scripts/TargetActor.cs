using System;
using Newtonsoft.Json;


[System.Serializable]
public class TargetActor : ARVINOActor
{

    [JsonConstructor]
    public TargetActor() { }

    public TargetActor(TargetActor targetActor)
    {
        this._Alt = targetActor._Alt;
        this._Direction = targetActor._Direction;
        this._ID = targetActor._ID;
        this._Lat = targetActor._Lat;
        this._Lon = targetActor._Lon;

    }

}