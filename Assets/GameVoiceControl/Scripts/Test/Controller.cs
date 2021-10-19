using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    enum Around
    {
        stop,
        up,
        down,
        left,
        right
    }

    public string CmdRotateToTheLeft = "rotate to the left";
    public string CmdRotateToTheRight = "rotate to the right";
    public string CmdRotateUp = "rotate up";
    public string CmdRotateDown = "rotate down";
    public string CmdStop = "stop";

    private Around rotateTo = Around.stop;

    private float _speed = 0.5f;

    // Update is called once per frame
    void Update()
    {
        switch (rotateTo)
        {
            case Around.left:
                this.transform.Rotate(Vector3.up, _speed);
                break;
            case Around.right:
                this.transform.Rotate(Vector3.down, _speed);
                break;
            case Around.down:
                this.transform.Rotate(Vector3.left, _speed);
                break;
            case Around.up:
                this.transform.Rotate(Vector3.right, _speed);
                break;
        }
    }

    public void onReceiveRecognitionResult(string result)
    {
        if (result.Contains(CmdRotateUp))
            rotateTo = Around.up;
        if (result.Contains(CmdRotateDown))
            rotateTo = Around.down;
        if (result.Contains(CmdRotateToTheLeft))
            rotateTo = Around.left;
        if (result.Contains(CmdRotateToTheRight))
            rotateTo = Around.right;
        if (result.Contains(CmdStop))
            rotateTo = Around.stop;
    }

}
