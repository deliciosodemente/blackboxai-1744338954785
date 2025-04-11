using UnityEngine;
using Photon.Pun;

namespace StonepunkBrawl.Core
{
    public class WeaponController : MonoBehaviourPunCallbacks
    {
        [Header("Weapon Settings")]
        public Transform weaponHolder;
        public float swingThreshold = 0.5f;
        public float damageMultiplier = 1.0f;
        
        [Header("VFX")]
        public ParticleSystem hitEffect;
        public ParticleSystem blockEffect;
        public TrailRenderer weaponTrail;
        
        [Header("Sound Effects")]
        public AudioClip swingSound;
        public AudioClip hitSound;
        public AudioClip blockSound;
        
        private Weapon currentWeapon;
        private bool isBlocking;
        private Vector3 previousPosition;
        private float swingVelocity;
        private AudioSource audioSource;
        
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (!photonView.IsMine)
            {
                enabled = false;
                return;
            }
        }
        
        private void Update()
        {
            if (!photonView.IsMine) return;
            
            // Calculate swing velocity
            swingVelocity = (weaponHolder.position - previousPosition).magnitude / Time.deltaTime;
            previousPosition = weaponHolder.position;
            
            // Update weapon trail based on velocity
            if (weaponTrail != null)
            {
                weaponTrail.emitting = swingVelocity > swingThreshold;
            }
        }
        
        public void Attack()
        {
            if (isBlocking || currentWeapon == null) return;
            
            if (swingVelocity > swingThreshold)
            {
                photonView.RPC("PerformAttack", RpcTarget.All);
            }
        }
        
        public void Block()
        {
            if (currentWeapon == null) return;
            
            isBlocking = true;
            photonView.RPC("ActivateBlock", RpcTarget.All);
        }
        
        [PunRPC]
        private void PerformAttack()
        {
            // Play swing animation
            if (currentWeapon.animator != null)
            {
                currentWeapon.animator.SetTrigger("Attack");
            }
            
            // Play effects
            if (swingSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(swingSound);
            }
            
            // Enable weapon collider for a brief moment
            StartCoroutine(EnableWeaponCollider());
        }
        
        [PunRPC]
        private void ActivateBlock()
        {
            if (blockEffect != null)
            {
                blockEffect.Play();
            }
            
            if (blockSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(blockSound);
            }
        }
        
        private System.Collections.IEnumerator EnableWeaponCollider()
        {
            if (currentWeapon.collider != null)
            {
                currentWeapon.collider.enabled = true;
                yield return new WaitForSeconds(0.1f);
                currentWeapon.collider.enabled = false;
            }
        }
        
        public void EquipWeapon(Weapon weapon)
        {
            if (currentWeapon != null)
            {
                // Unequip current weapon
                Destroy(currentWeapon.gameObject);
            }
            
            // Equip new weapon
            currentWeapon = Instantiate(weapon, weaponHolder);
            currentWeapon.transform.localPosition = Vector3.zero;
            currentWeapon.transform.localRotation = Quaternion.identity;
            
            photonView.RPC("SyncWeapon", RpcTarget.All, weapon.weaponId);
        }
        
        [PunRPC]
        private void SyncWeapon(int weaponId)
        {
            // Sync weapon across network
            if (!photonView.IsMine)
            {
                // Load weapon prefab based on ID and equip it
                var weaponPrefab = Resources.Load<Weapon>($"Weapons/{weaponId}");
                if (weaponPrefab != null)
                {
                    EquipWeapon(weaponPrefab);
                }
            }
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (!photonView.IsMine || currentWeapon == null) return;
            
            // Check if we hit another player
            var otherPlayer = collision.gameObject.GetComponent<PlayerController>();
            if (otherPlayer != null)
            {
                float damage = currentWeapon.baseDamage * damageMultiplier * (swingVelocity / swingThreshold);
                otherPlayer.photonView.RPC("TakeDamage", RpcTarget.All, damage);
                
                if (hitEffect != null)
                {
                    hitEffect.Play();
                }
                
                if (hitSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(hitSound);
                }
            }
        }
    }
}
