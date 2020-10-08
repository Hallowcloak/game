using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace ControllerExperiment
{
    public class PlayerController : NetworkBehaviour
    {
        [Header("Found on Awake")]
        public Rigidbody rbody;
        public SphereCollider sphereCollider;

        [Header("Attributes")]
        public float TargetAngle;
        public float JumpForce;

        [Header("Debug")]
        [SerializeField] public Vector3 TargetWalkDir = new Vector3();
        public bool Grounded;
        public bool JumpButtonPressed;
        public bool JumpTriggered;
        public bool JumpUpdated;

        public Dictionary<SubComponents, SubComponent> SubComponentsDic = new Dictionary<SubComponents, SubComponent>();

        public Dictionary<CharacterProc, ProcDel> ProcDic = new Dictionary<CharacterProc, ProcDel>();
        public delegate void ProcDel();

        private void Awake()
        {
            JumpTriggered = false;
            rbody = this.gameObject.GetComponent<Rigidbody>();
            sphereCollider = this.gameObject.GetComponent<SphereCollider>();
        }

        [Client]
        private void Update()
        {
            if (!hasAuthority) { return; }
            if (CrossPlatformInputManager.GetButtonDown("Jump"))
            {
                JumpButtonPressed = true;
            }

            SubComponentsDic[SubComponents.MOVE_HORIZONTAL].OnUpdate();
        }

        private RaycastHit hit;
        [Command]
        private void OnUpdate()
        {
            var player = hit.collider.gameObject.GetComponent<PlayerCollision>();
            if (player)
            {
                StartCoroutine(Respawn(hit.collider.gameObject));
            }
            RpcMove();
        }

        [ClientRpc]
        private void RpcMove() => transform.Translate(TargetWalkDir);

        private void OnCollisionStay(Collision col)
        {
            foreach(ContactPoint p in col.contacts)
            {
                Vector3 bottom = sphereCollider.bounds.center - (Vector3.up * sphereCollider.bounds.extents.y);
                Vector3 curve = bottom + (Vector3.up * sphereCollider.radius);

                Debug.DrawLine(curve, p.point, Color.blue, 0.5f);
                Vector3 dir = curve - p.point;
                
                if (dir.y > 0f)
                {
                    Grounded = true;

                    if (JumpUpdated)
                    {
                        JumpButtonPressed = false;
                        JumpTriggered = false;
                        JumpUpdated = false;
                        ProcDic[CharacterProc.CANCEL_HORIZONTALVELOCITY]();
                    }
                }
            }
        }

        [Server]
        IEnumerator Respawn(GameObject player)
        {
            NetworkServer.UnSpawn(player);
            Transform newPos = NetworkManager.singleton.GetStartPosition();
            player.transform.position = newPos.position;
            player.transform.rotation = newPos.rotation;
            yield return new WaitForSeconds(1f);
            NetworkServer.Spawn(player, player);
        }

        private void FixedUpdate()
        {
            SubComponentsDic[SubComponents.MOVE_HORIZONTAL].OnFixedUpdate();
            SubComponentsDic[SubComponents.ROTATION].OnFixedUpdate();

            Jump();

            if (!JumpButtonPressed && !JumpTriggered)
            {
                CancelVerticalVelocity();
            }

            Grounded = false;

            if (rbody.position.y < 1f)
            {
                FindObjectOfType<GameManager>().KillPlayer(this);
            }
        }

        void CancelVerticalVelocity()
        {
            if (rbody.velocity.y > 0f)
            {
                rbody.AddForce(Vector3.up * -rbody.velocity.y, ForceMode.VelocityChange);
            }
        }

        void Jump()
        {
            if (JumpTriggered)
            {
                JumpUpdated = true;
            }

            if (JumpButtonPressed && !JumpUpdated)
            {
                Debug.Log("jump triggered");
                rbody.AddForce(Vector3.up * -rbody.velocity.y, ForceMode.VelocityChange);
                rbody.AddForce(Vector3.up * JumpForce, ForceMode.VelocityChange);

                JumpButtonPressed = false;
                JumpTriggered = true;
            }
        }
    }
}