using System;

[System.Serializable]
public abstract class ARVINOActor
{
    ///<summary> Altitude of this actor </summary>
    public double _Alt;
    public double _Lat;
    public double _Lon;

    ///<summary> Direction this actor is currently facing/heading from the user </summary>
    public float _Direction;

    ///<summary> The database key of this data </summary>
    public string _ID;

    public float _Distance;



}