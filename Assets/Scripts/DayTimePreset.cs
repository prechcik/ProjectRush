using UnityEngine;

[CreateAssetMenu(fileName = "Lighting Preset", menuName = "Scriptables/Lighting Preset")]
public class DayTimePreset : ScriptableObject
{
    public Gradient AmbientColor;
    public Gradient DirectionalColor;
    public Gradient FogColor;
}
