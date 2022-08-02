namespace TopdownShooter
{
    using GameUtilsFramework;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class PlayerControl : MonoBehaviour
    {
        public float speed = 4;
        public float rotateLerpSpeed = 0.2f;

        [Header("Anim")]
        public bool useRootMotion;
        public float animSpeed = 1;

        [Header("Shoot")]
        public GameObject bulletPrefab;
        public Transform GunDir;
        public float shootInterval = 0.5f;



        Camera cam;
        Animator anim;

        InputControl playerInput;


        float bulletLastTime;

        // Start is called before the first frame update
        void Start()
        {
            playerInput = GetComponent<InputControl>();
            cam = Camera.main;
            anim = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {

            var targetForwardDir = MouseAim();

            var hv = playerInput.movement;


            //hv.x = AnimatorTools.QuantifyInputValue(hv.x);
            //hv.y = AnimatorTools.QuantifyInputValue(hv.y);

            var moveDir = new Vector3(hv.x, 0, hv.y);
            float leftTurnRate = 0;
            if (hv.sqrMagnitude > 0)
            {
                if (!useRootMotion)
                {
                    moveDir.Normalize();
                    transform.Translate(moveDir * speed * Time.deltaTime, Space.World);
                }
            }
            else
            {
                leftTurnRate = CalcLeftTurnRate(targetForwardDir);
            }


            UpdateMoveAnim(moveDir, leftTurnRate);

            UpdateFire();
        }

        private void OnAnimatorMove()
        {
            if (!useRootMotion)
                return;

            if (playerInput.movement.sqrMagnitude == 0)
                return;

            var deltaPos = anim.deltaPosition;
            transform.position += deltaPos;
        }

        float CalcLeftTurnRate(Vector3 targetForwardDir)
        {
            Debug.DrawRay(transform.position, transform.forward * 10);
            Debug.DrawRay(transform.position, targetForwardDir * 10, Color.blue);

            var dir = Vector3.Cross(targetForwardDir, transform.forward);
            Debug.DrawRay(transform.position, dir, Color.green);

            var leftTurnRate = Vector3.Dot(Vector3.Cross(transform.forward, targetForwardDir), transform.up);
            leftTurnRate = Mathf.Abs(leftTurnRate) > 0.01f ? Mathf.Sign(leftTurnRate) : 0;
            return leftTurnRate;
        }

        private void UpdateMoveAnim(Vector3 moveDir, float leftTurnRate)
        {
            anim.speed = animSpeed;

            // move
            var speedX = Vector3.Dot(moveDir, transform.right) + leftTurnRate;
            var speedZ = Vector3.Dot(moveDir, transform.forward);

            if (Mathf.Abs(speedX) < 0.001)
                speedX = 0;
            if (Mathf.Abs(speedZ) < 0.001)
                speedZ = 0;

            anim.SetFloat("SpeedX", speedX, 0.1f, Time.deltaTime);
            anim.SetFloat("SpeedZ", speedZ, 0.1f, Time.deltaTime);
        }

        private Vector3 MouseAim()
        {
            var dir = transform.forward;
            var ray = cam.ScreenPointToRay(playerInput.topdownLook);
            if (Physics.Raycast(ray, out var hit))
            {
                var pos = hit.point;
                pos.y = transform.position.y;
                dir = (pos - transform.position).normalized;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), rotateLerpSpeed);

            }
            return dir;
        }



        void UpdateFire()
        {
            var canShoot = playerInput.IsRightHandAttack() && (Time.time - bulletLastTime) > shootInterval;
            anim.SetBool("IsFire", canShoot);

            var fireLayerId = anim.GetLayerIndex("Fire");
            if (anim.GetLayerWeight(fireLayerId) < 1)
                anim.SetLayerWeight(fireLayerId, 1);

            if (canShoot)
            {
                bulletLastTime = Time.time;

                ShootABullet();
            }
        }

        private void ShootABullet()
        {
            var b = Instantiate(bulletPrefab, GunDir.position, GunDir.rotation);
            b.GetComponent<Rigidbody>().AddForce(b.transform.forward * 100, ForceMode.Impulse);
            Destroy(b, 1);
        }
    }
}