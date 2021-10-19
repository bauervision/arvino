using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VoiceCommands : MonoBehaviour
{
    public static VoiceCommands instance;
    public AudioSource northSource;
    public AudioSource northEastSource;
    public AudioSource eastSource;
    public AudioSource southEastSource;
    public AudioSource southSource;
    public AudioSource southWestSource;
    public AudioSource westSource;
    public AudioSource northWestSource;
    public Image activeAudioImage;


    // Use this for initialization
    void Start()
    {
        instance = this;
    }

    enum Base
    {
        start,//activate sounds
        north,// where's north?
        update,// play target audio
        silent,// stop intervals
        update_all,
        update_vehicles,
        update_people,
        update_objectives,
        track_north,
        track_north_east,
        track_east,
        track_south_east,
        track_south,
        track_south_west,
        track_west,
        track_north_west,
        ten_seconds,
        thirty_seconds,
        one_minute,
        two_minutes,
        five_minutes,
        set_interval,
        none

    }

    string CmdStart = "start";
    string CmdNorth = "where's north";
    string CmdUpdate = "update";
    string CmdSilent = "silence";
    string CmdUpdateAll = "update all";
    string CmdUpdateVehicles = "update vehicles";
    string CmdUpdatePeople = "update people";
    string CmdUpdateObjectives = "update objectives";
    string CmdTrackNorth = "track north";
    string CmdTrackNorthEast = "track north east";
    string CmdTrackEast = "track east";
    string CmdTrackSouthEast = "track south east";
    string CmdTrackSouth = "track south";
    string CmdTrackSouthWest = "track south west";
    string CmdTrackWest = "track west";
    string CmdTrackNorthWest = "track north west";

    string Cmd10 = "ten seconds";
    string Cmd30 = "thirty seconds";
    string Cmd60 = "one minute";
    string Cmd120 = "two minutes";
    string Cmd5 = "five minutes";
    string CmdSetInterval = "set interval";


    private Base voiceCommands = Base.start;



    // Update is called once per frame
    void Update()
    {
        switch (voiceCommands)
        {
            case Base.start:
                print("Start ...");
                voiceCommands = Base.none;
                break;
            case Base.north:
                print("Where's North ...");
                northSource.PlayOneShot(AudioManager.instance.baseAudio.North);
                voiceCommands = Base.none;
                break;
            case Base.update:
                print("Update ...");
                voiceCommands = Base.none;
                break;
            case Base.silent:
                print("Silence ...");
                voiceCommands = Base.none;
                break;
            case Base.update_all:
                print("Update All ...");
                voiceCommands = Base.none;
                break;
            case Base.update_vehicles:
                print("Update Vehicles ...");
                voiceCommands = Base.none;
                break;
            case Base.update_people:
                print("Update People ...");
                voiceCommands = Base.none;
                break;
            case Base.update_objectives:
                print("Update Objectives ...");
                voiceCommands = Base.none;
                break;
            case Base.track_north:
                print("Track North ...");
                northSource.PlayOneShot(AudioManager.instance.baseAudio.North);

                voiceCommands = Base.none;
                break;
            case Base.track_north_east:
                print("Track North East ...");
                northEastSource.PlayOneShot(AudioManager.instance.baseAudio.NorthEast);

                voiceCommands = Base.none;
                break;
            case Base.track_east:
                print("Track East ...");
                eastSource.PlayOneShot(AudioManager.instance.baseAudio.East);

                voiceCommands = Base.none;
                break;
            case Base.track_south_east:
                print("Track South East ...");
                southEastSource.PlayOneShot(AudioManager.instance.baseAudio.SouthEast);

                voiceCommands = Base.none;
                break;
            case Base.track_south:
                print("Track South ...");
                southSource.PlayOneShot(AudioManager.instance.baseAudio.South);

                voiceCommands = Base.none;
                break;
            case Base.track_south_west:
                print("Track South West ...");
                southWestSource.PlayOneShot(AudioManager.instance.baseAudio.SouthWest);

                voiceCommands = Base.none;
                break;
            case Base.track_west:
                print("Track West ...");
                westSource.PlayOneShot(AudioManager.instance.baseAudio.West);

                voiceCommands = Base.none;
                break;
            case Base.track_north_west:
                print("Track North West...");
                northWestSource.PlayOneShot(AudioManager.instance.baseAudio.NorthWest);

                voiceCommands = Base.none;
                break;
            case Base.ten_seconds:
                print("10 seconds ...");
                voiceCommands = Base.none;
                break;
            case Base.thirty_seconds:
                print("30 seconds ...");
                voiceCommands = Base.none;
                break;
            case Base.one_minute:
                print("1 minute ...");
                voiceCommands = Base.none;
                break;
            case Base.two_minutes:
                print("2 minutes ...");
                voiceCommands = Base.none;
                break;
            case Base.five_minutes:
                print("5 minutes ...");
                voiceCommands = Base.none;
                break;
            case Base.set_interval:
                print("Set Interval ...");
                voiceCommands = Base.none;
                break;
        }
    }

    public void onReceiveRecognitionResult(string result)
    {
        if (result.Contains(CmdStart))
            voiceCommands = Base.start;
        if (result.Contains(CmdNorth))
            voiceCommands = Base.north;
        if (result.Contains(CmdUpdate))
            voiceCommands = Base.update;
        if (result.Contains(CmdSilent))
            voiceCommands = Base.silent;
        if (result.Contains(CmdUpdateAll))
            voiceCommands = Base.update_all;
        if (result.Contains(CmdUpdateVehicles))
            voiceCommands = Base.update_vehicles;
        if (result.Contains(CmdUpdatePeople))
            voiceCommands = Base.update_people;
        if (result.Contains(CmdUpdateObjectives))
            voiceCommands = Base.update_objectives;
        if (result.Contains(CmdTrackNorth))
            voiceCommands = Base.track_north;
        if (result.Contains(CmdTrackNorthEast))
            voiceCommands = Base.track_north_east;
        if (result.Contains(CmdTrackEast))
            voiceCommands = Base.track_east;
        if (result.Contains(CmdTrackSouthEast))
            voiceCommands = Base.track_south_east;
        if (result.Contains(CmdTrackSouth))
            voiceCommands = Base.track_south;
        if (result.Contains(CmdTrackSouthWest))
            voiceCommands = Base.track_south_west;
        if (result.Contains(CmdTrackWest))
            voiceCommands = Base.track_west;
        if (result.Contains(CmdTrackNorthWest))
            voiceCommands = Base.track_north_west;
        if (result.Contains(Cmd10))
            voiceCommands = Base.ten_seconds;
        if (result.Contains(Cmd30))
            voiceCommands = Base.thirty_seconds;
        if (result.Contains(Cmd60))
            voiceCommands = Base.one_minute;
        if (result.Contains(Cmd120))
            voiceCommands = Base.two_minutes;
        if (result.Contains(Cmd5))
            voiceCommands = Base.five_minutes;
        if (result.Contains(CmdSetInterval))
            voiceCommands = Base.set_interval;

    }

    public void SetAudioActive()
    {
        activeAudioImage.color = Color.green;
    }

}
