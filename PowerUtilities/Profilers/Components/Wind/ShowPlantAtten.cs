namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ShowPlantAtten : MonoBehaviour
    {

        GameObject cylinderRootGo;
        Material plantMat;

        void OnDrawGizmosSelected()
        {
            if (!cylinderRootGo)
            {
                cylinderRootGo = new GameObject("PlantsGizmos");
                cylinderRootGo.transform.SetParent(transform, false);

                var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder).GetComponent<MeshFilter>();
                cylinder.transform.localRotation = Quaternion.Euler(90, 0, 0);

                cylinder.transform.SetParent(cylinderRootGo.transform, false);

            }
            if (!plantMat)
            {
                plantMat = GetComponent<MeshRenderer>().sharedMaterial;
            }

            if (!plantMat)
                return;

            var attenField = plantMat.GetVector("_AttenField");
            cylinderRootGo.transform.localScale = new Vector3(attenField.x, attenField.x, attenField.y);
        }



        void OnDisable()
        {
            if (cylinderRootGo)
                DestroyImmediate(cylinderRootGo);
        }
    }
}