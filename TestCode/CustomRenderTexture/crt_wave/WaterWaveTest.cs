using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterWaveTest : MonoBehaviour
{
    public CustomRenderTexture crt;
    public float updateSize = 0.2f;

    //pass 0, full rt update, used for initialization
    CustomRenderTextureUpdateZone[] fullZones = new CustomRenderTextureUpdateZone[]
    {
        new CustomRenderTextureUpdateZone()
        {
            needSwap = true,
            passIndex = 0,
            updateZoneCenter = new Vector2(0.5f, 0.5f),
            updateZoneSize = new Vector2(1f, 1f)
        }
    };

    // Start is called before the first frame update
    void Start()
    {
        crt.initializationMode = CustomRenderTextureUpdateMode.OnLoad;
        crt.updateMode = CustomRenderTextureUpdateMode.Realtime;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var uv = hit.textureCoord;
                uv.y = 1- uv.y; // Invert Y coordinate for texture space
                CustomRenderTextureUpdateZone zone = new CustomRenderTextureUpdateZone();
                zone.updateZoneCenter = uv;
                zone.updateZoneSize = new Vector2(0.01f, 0.01f);
                zone.passIndex = 1; 
                zone.needSwap = true;

                crt.SetUpdateZones(new CustomRenderTextureUpdateZone[] { zone });
            }
        }
        else
        {
            crt.SetUpdateZones(fullZones);
        }
        //crt.Update();
    }
}
