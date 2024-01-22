using UnityEngine;

/// <summary>
/// Update baked light probes 
/// </summary>
[ExecuteAlways]
public class LightProbesUpdater : MonoBehaviour
{
    //public Gradient ambientColors = new Gradient
    //{
    //    colorKeys = new GradientColorKey[] { 
    //        new GradientColorKey(Color.black,0),
    //        new GradientColorKey (Color.white,100),
    //    }
    //};
    
    public Color ambient;
    public Light[] lights;

    private void Update()
    {
        var probes = LightmapSettings.lightProbes.bakedProbes;
        var probePos = LightmapSettings.lightProbes.positions;
        var count = LightmapSettings.lightProbes.count;

        for (int i = 0; i < count; i++)
        {
            probes[i].Clear();
            probes[i].AddAmbientLight(ambient);

            foreach (Light light in lights)
            {
                var intensity = light.intensity;
                var dir = -light.transform.forward;
                if (light.type == LightType.Point)
                {
                    dir = light.transform.position - probePos[i];
                    intensity *= 1f / (1 + 25 * (dir.sqrMagnitude / light.range / light.range));
                    dir.Normalize();
                }
                probes[i].AddDirectionalLight(dir, light.color, intensity);
            }
        }
        LightmapSettings.lightProbes.bakedProbes = probes;
    }
}