namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class TestGradient : MonoBehaviour
    {
        public PowerGradient g1;
        public PowerGradient g2;

        public Gradient g;

        public float test;

        [Range(0, 1)] public float rate;
        public float speed = 1f;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            rate += speed * Time.deltaTime;

            var c = g1.Evaluate(rate);
            GetComponent<MeshRenderer>().sharedMaterial.color = c;
        }
    }
}