using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SurgeAudio
{
    [System.Serializable]
    public class Base
    {
        public AudioClip BehindYou;
        public AudioClip Objective;
        public AudioClip Person;
        public AudioClip Vehicle;
        public AudioClip Multiple;
        public AudioClip North;
        public AudioClip NorthEast;
        public AudioClip East;
        public AudioClip SouthEast;
        public AudioClip South;
        public AudioClip SouthWest;
        public AudioClip West;
        public AudioClip NorthWest;

    }

    [System.Serializable]
    public class Ranges
    {
        public AudioClip Less_Than_50M;
        public AudioClip Less_Than_100M;
        public AudioClip Less_Than_500M;
        public AudioClip Less_Than_1KM;
        public AudioClip More_Than_1K;

    }




}