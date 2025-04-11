using UnityEngine;

namespace StonepunkBrawl.Core
{
    public class Weapon : MonoBehaviour
    {
        [Header("Weapon Identity")]
        public int weaponId;
        public string weaponName;
        public WeaponType weaponType;
        
        [Header("Stats")]
        public float baseDamage = 10f;
        public float weight = 1f;
        public float durability = 100f;
        public float attackSpeed = 1f;
        
        [Header("Components")]
        public Animator animator;
        public Collider collider;
        public ParticleSystem specialEffect;
        public AudioSource audioSource;
        
        [Header("VFX")]
        public TrailRenderer weaponTrail;
        public Material weaponMaterial;
        public Color weaponGlowColor;
        
        [Header("Special Abilities")]
        public bool hasSpecialAbility;
        public float specialAbilityCooldown = 10f;
        public float specialAbilityDuration = 5f;
        
        private float currentDurability;
        private bool isSpecialAbilityActive;
        private float specialAbilityCooldownTimer;
        
        public enum WeaponType
        {
            Sword,
            Axe,
            Spear,
            Shield,
            Bow,
            Staff
        }
        
        private void Awake()
        {
            currentDurability = durability;
            
            // Ensure collider starts disabled
            if (collider != null)
            {
                collider.enabled = false;
            }
            
            // Initialize weapon trail
            if (weaponTrail != null)
            {
                weaponTrail.emitting = false;
            }
            
            // Set up material glow
            if (weaponMaterial != null)
            {
                weaponMaterial.SetColor("_EmissionColor", weaponGlowColor);
            }
        }
        
        public virtual void OnSpecialAbilityActivated()
        {
            if (!hasSpecialAbility || isSpecialAbilityActive || specialAbilityCooldownTimer > 0)
                return;
            
            isSpecialAbilityActive = true;
            specialAbilityCooldownTimer = specialAbilityCooldown;
            
            if (specialEffect != null)
            {
                specialEffect.Play();
            }
            
            // Override in derived classes for specific weapon abilities
        }
        
        public virtual void OnSpecialAbilityDeactivated()
        {
            isSpecialAbilityActive = false;
            
            if (specialEffect != null)
            {
                specialEffect.Stop();
            }
            
            // Override in derived classes for specific weapon cleanup
        }
        
        public void TakeDurabilityDamage(float damage)
        {
            currentDurability -= damage;
            
            if (currentDurability <= 0)
            {
                OnWeaponBroken();
            }
        }
        
        protected virtual void OnWeaponBroken()
        {
            // Play break effect
            if (specialEffect != null)
            {
                specialEffect.Play();
            }
            
            // Notify weapon controller
            var weaponController = GetComponentInParent<WeaponController>();
            if (weaponController != null)
            {
                // Handle weapon breaking logic
                weaponController.SendMessage("OnWeaponBroken", this, SendMessageOptions.DontRequireReceiver);
            }
        }
        
        private void Update()
        {
            // Handle special ability cooldown
            if (specialAbilityCooldownTimer > 0)
            {
                specialAbilityCooldownTimer -= Time.deltaTime;
            }
            
            // Handle special ability duration
            if (isSpecialAbilityActive)
            {
                specialAbilityDuration -= Time.deltaTime;
                if (specialAbilityDuration <= 0)
                {
                    OnSpecialAbilityDeactivated();
                }
            }
        }
        
        public float GetSpecialAbilityCooldownProgress()
        {
            return 1 - (specialAbilityCooldownTimer / specialAbilityCooldown);
        }
        
        public bool CanUseSpecialAbility()
        {
            return hasSpecialAbility && !isSpecialAbilityActive && specialAbilityCooldownTimer <= 0;
        }
    }
}
