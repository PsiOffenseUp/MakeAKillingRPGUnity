using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinuteAtom : AtomAnimation
{
    float ticksInHour = Clock.minutesPerHour * Clock.minuteLength;

    // Start is called before the first frame update
    void Start() { 
   
    }

    // Update is called once per frame
    void Update()
    {
        MoveAlongCircle(GameplayManager.clock.currentTime / ticksInHour);
    }
}
