using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TimeManager : MonoBehaviour
{
    public int startTime = 0;
    public float tickPerSec = 4;

    public int endOfDay = 900;
    public int startOfNight = -900;

    public float CurrentTime = 0;

    public float dawnTime = -40;
    public float sunsetTime = 860;

    public float lightingChange = .5f;

    public bool dark = false;

    public Light2D globalLight;
    public Light2D playerLight;

    public Color dayBackground;
    
    private void Start()
    {
        CurrentTime = startTime;
    }

    private void Update()
    {
        CurrentTime += Time.deltaTime * tickPerSec;

        if (CurrentTime > endOfDay) CurrentTime = startOfNight;

        //Check for the dawn or sunset.
        if ((CurrentTime > dawnTime && CurrentTime < sunsetTime) && dark) brightenWorldLight();
        if ((CurrentTime > sunsetTime || CurrentTime < dawnTime) && !dark) darkenWorldLight();

        playerLight.intensity = 1 - globalLight.intensity;

        Camera.main.backgroundColor = new Color(dayBackground.r * globalLight.intensity, dayBackground.g * globalLight.intensity, dayBackground.b * globalLight.intensity);
    }

    void brightenWorldLight()
    {
        if(globalLight.intensity >= 1)
        {
            dark = false;
            globalLight.intensity = 1;
            Camera.main.backgroundColor = dayBackground;
        }
        else
        {
            globalLight.intensity += lightingChange * Time.deltaTime;
        }
    }

    void darkenWorldLight()
    {
        if (globalLight.intensity <= 0)
        {
            dark = true;
            globalLight.intensity = 0;
        }
        else
        {
            globalLight.intensity -= lightingChange * Time.deltaTime;
        }
    }
}
