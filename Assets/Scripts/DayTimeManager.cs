using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class DayTimeManager : MonoBehaviour
{
    public DayTimePreset Preset;
    public Light DirectionalLight;

    [Range(0,24)] public float TimeOfDay;

    private void Update()
    {
        OnValidate();
    }

    public void UpdateLightning(float time)
    {
        TimeOfDay = time * 24f;
        if (RenderSettings.ambientLight != null && RenderSettings.fogColor != null && Preset != null)
        RenderSettings.ambientLight = Preset.AmbientColor.Evaluate(time);
        RenderSettings.fogColor = Preset.FogColor.Evaluate(time);

        if(DirectionalLight != null)
        {
            DirectionalLight.color = Preset.DirectionalColor.Evaluate(time);
            DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3((time * 360f) - 90f, 170f, 0));
        }
    }

    private void OnValidate()
    {
        if (DirectionalLight != null)
        {
            return;
        }
        if (RenderSettings.sun != null)
        {
            DirectionalLight = RenderSettings.sun;
        } else
        {
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            foreach(Light l in lights)
            {
                if (l.type == LightType.Directional)
                {
                    DirectionalLight = l;
                    return;
                }
            }
        }
    }
}
