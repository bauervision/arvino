using UnityEngine;
using System.Collections;

[System.Serializable]
public class SceneActor
{
    public int id;
    public int targetMeshIndex;
    public double targetLatitude;
    public double targetLongitude;

    public SceneActor(int id, int index, double lat, double lng)
    {
        this.targetMeshIndex = index;// which index is the mesh we chose?
        this.id = id; // specific id for this object index of when it was spawned in the scene.
        this.targetLatitude = lat;
        this.targetLongitude = lng;
    }

}