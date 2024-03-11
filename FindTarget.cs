using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Prototype.Combat
{
    public class FindTarget : MonoBehaviour
    {
        [SerializeField] GameObject cam;
        [SerializeField] GameObject target2;
        [SerializeField] LayerMask layerMask;
        [SerializeField] Cinemachine.CinemachineFreeLook freeLook;
        [SerializeField] Cinemachine.CinemachineVirtualCamera virtualCameraLock;

        [SerializeField] float coneAngle_Forward = 60f;
        [SerializeField] float radius = 1f;
        [SerializeField] float maxDistance;

        bool targetFound;
        PrototypeOfGame inputActions;
        Target target;

        Vector3 ray;
        Vector3 rayangle;
        private void OnEnable()
        {
            inputActions.Player.Enable();
        }
        private void OnDisable()
        {
            inputActions.Player.Disable();
        }
        void Awake()
        {
            inputActions = new PrototypeOfGame();
        }
        private void Start()
        {
            inputActions.Player.Targeting.performed += ctx =>
            {
                if (targetFound == false)
                {
                    RayCastHit();
                }
                else if(targetFound == true)
                {
                    targetFound = false;
                    target = null;
                }
            };
        }
        private void Update()
        {
        }
        void RayCastHit()
        {
            Vector3 forward = cam.transform.forward;
            forward.y = 0;
            forward = forward.normalized;
            RaycastHit[] shpereCast = Physics.SphereCastAll(transform.position, radius, forward, maxDistance, layerMask);
            float cosHalfConeAngle_Z = Mathf.Cos(coneAngle_Forward * 0.5f * Mathf.Deg2Rad);

            RaycastHit nearestHit = new RaycastHit();
            float nearestDistance = Mathf.Infinity;

            foreach (var hit in shpereCast)
            {
                forward = cam.transform.forward;
                forward.y = 0;
                forward.Normalize();
                Vector3 dirToHit = hit.transform.position;
                Vector3 newDir = dirToHit - transform.position;
                newDir.y = 0;
                newDir.Normalize();
                float dot = Vector3.Dot(forward, newDir);

                if (dot >= cosHalfConeAngle_Z)
                {
                    Vector3 position = hit.point;
                    if (hit.point.y > radius)
                    {
                        continue;
                    }
                    position.y = 0;
                    // Calculate distance from hit to origin
                    float distanceToHit = Vector3.Distance(transform.position, hit.point);

                    // Check if this hit is closer than the previous nearest hit
                    if (distanceToHit < nearestDistance)
                    {
                        // Update nearest hit and distance
                        nearestHit = hit;
                        Debug.Log(hit.collider.name);
                        //Debug.Log(dot + " Dot ");
                        nearestDistance = distanceToHit;
                    }
                }
            }
            // Check if a nearest hit was found
            if (nearestHit.collider != null)
            {
                // Get Target component from nearest hit object
                target = nearestHit.collider.GetComponent<Target>();
                if (target != null)
                {
                    Debug.Log(target.name + " TARGETfOUND ");
                    // Lock onto target
                    targetFound = true;
                    // Additional processing if needed
                }
            }
        }

        private Vector3 Modifier(Vector3 forward, Vector3 dirToHit)
        {
            dirToHit.z = -dirToHit.z;
            dirToHit.x = -dirToHit.x;
            return dirToHit.normalized;
        }

        public bool IsLock()
        {
            return targetFound;
        }
        public Transform EnemyToPlayerDir()
        {
            return target.transform;
        }
        private void OnDrawGizmos()
        {
            // Set the color for the Gizmo
            Gizmos.color = Color.yellow;

            // Draw the cone using Gizmo lines
            Vector3 forward = cam.transform.forward;
            forward.y = 0f;
            forward = forward.normalized;
            Quaternion rotation = Quaternion.Euler(0f, -coneAngle_Forward * 0.5f, 0f);
            Vector3 leftRay = rotation * forward;
            Vector3 rightRay = Quaternion.Inverse(rotation) * forward;

            // Draw the left ray
            Gizmos.DrawRay(transform.position, leftRay * maxDistance);

            // Draw the right ray
            Gizmos.DrawRay(transform.position, rightRay * maxDistance);

            Gizmos.DrawWireSphere(transform.position, radius);
            //Gizmos.DrawSphere(transform.position, radius);
            //Debug.Log(Vector3.Dot(cam.transform.up,upRay));

            ray = cam.transform.forward;
            ray.y = 0f;
            ray.Normalize();
            rayangle =target2.transform.position - transform.position;
            rayangle.y = 0;
            rayangle.Normalize();

            Debug.DrawRay(transform.position, ray * 5, Color.red);
            Debug.DrawRay(transform.position, rayangle * 5, Color.black);
            //Debug.Log(Vector3.Angle(ray.normalized, rayangle.normalized));
            //Debug.Log(Vector3.Dot(ray.normalized, rayangle.normalized));
        }
    }
}
