namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [ExecuteAlways]
    public class SendPlayerPos : MonoBehaviour
    {
        [Header("Input")]
        public bool canControl = true;
        public float speed = 4;

        Rigidbody r;

        [Header("Material")]
        [Tooltip("grass or planarShadow")]
        public Material mat;

        [Header("grass mat")]
        public bool controlCullingAnim = true;
        public float cullDistance = 10;
        public bool isSendGlobal;


        [Header("Player")]
        public Transform playerTr;
        public Vector3 posOffset;

        // Start is called before the first frame update
        void OnEnable()
        {
            r = GetComponent<Rigidbody>();
        }

        Vector3 GetPlayerPos()
        {
            return (playerTr ? playerTr.position : transform.position) + posOffset;
        }

        // Update is called once per frame
        void Update()
        {
            if (canControl)
            {
                var h = Input.GetAxis("Horizontal");
                var v = Input.GetAxis("Vertical");
                var newPos = new Vector3(h, 0, v) * (Time.deltaTime * speed);
                if (r)
                {
                    r.MovePosition(transform.position + newPos);
                }
                else
                {
                    transform.Translate(newPos);
                }
            }

            if (isSendGlobal)
            {
                Shader.SetGlobalVector("_PlayerPos", GetPlayerPos());
            }

            if (mat)
            {
                mat.SetVector("_PlayerPos", GetPlayerPos());
            }

            if (controlCullingAnim && mat)
            {
                mat.SetVector("_CullPos", GetPlayerPos(), null);
                mat.SetFloat("_CullDistance", cullDistance, null);
            }
        }
    }
}