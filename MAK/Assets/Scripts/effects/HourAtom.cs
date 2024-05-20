using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HourAtom : AtomAnimation
{
    float ticksInDay = Clock.minutesPerHour * Clock.minuteLength * Clock.hourLength * Clock.hoursPerDay;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        MoveAlongCircle(GameplayManager.clock.currentTime / ticksInDay);
    }
}
