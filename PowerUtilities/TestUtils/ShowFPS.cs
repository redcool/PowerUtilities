using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace PowerUtilities.Test
{
    public class ShowFPS : MonoBehaviour
    {
        public Text fpsText;
        //public TMPro.TextMeshProUGUI textMeshProUGUI;

        [Range(30,2000)]public int maxFps=2000;

        int fps;
        float startTime;
        // Start is called before the first frame update
        void Start()
        {
            Application.targetFrameRate = maxFps;

            if (!fpsText)
                enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.time - startTime > 1)
            {

                if(fpsText)
                    fpsText.text = fps.ToString();

                //if(textMeshProUGUI)
                //    textMeshProUGUI.text = fps.ToString();


                startTime = Time.time;
                fps = 0;
            }

            fps++;
        }
    }
}