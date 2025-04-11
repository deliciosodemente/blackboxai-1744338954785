using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;

namespace StonepunkBrawl.Core
{
    public class PlayerController : MonoBehaviourPunCallbacks
    {
        [Header("VR Settings")]
        public Transform headset;
        public Transform leftController;
        public Transform rightController;
        
        [Header("Movement")]
        public float moveSpeed = 5f;
        public float turnSpeed = 45f;
        
        [Header("Combat")]
        public float attackDamage = 10f;
        public float blockReduction = 0.5f;
        public LayerMask weaponLayer;
        
        private Rigidbody rb;
        private PlayerHealth health;
        private WeaponController weaponController;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            health = GetComponent<PlayerHealth>();
            weaponController = GetComponent<WeaponController>();
            
            if (!photonView.IsMine)
            {
                // Disable control scripts for remote players
                enabled = false;
                return;
            }
        }
        
        private void Update()
        {
            if (!photonView.IsMine) return;
            
            HandleMovement();
            HandleWeapons();
            HandleBlocking();
        }
        
        private void HandleMovement()
        {
            // Get input from left controller for movement
            Vector2 movement = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand)
                .TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 value) ? value : Vector2.zero;
                
            // Apply movement relative to headset orientation
            Vector3 moveDirection = headset.TransformDirection(new Vector3(movement.x, 0, movement.y));
            rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.deltaTime);
        }
        
        private void HandleWeapons()
        {
            // Check trigger press for attacks
            if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand)
                .TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed) && triggerPressed)
            {
                weaponController.Attack();
            }
        }
        
        private void HandleBlocking()
        {
            // Check grip button for blocking
            if (InputDevices.GetDeviceAtXRNode(XRNode.LeftHand)
                .TryGetFeatureValue(CommonUsages.gripButton, out bool gripPressed) && gripPressed)
            {
                weaponController.Block();
            }
        }
        
        [PunRPC]
        public void TakeDamage(float damage)
        {
            if (!photonView.IsMine) return;
            health.TakeDamage(damage);
        }
    }
}
