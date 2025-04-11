using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace StonepunkBrawl.Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        
        [Header("Menu Panels")]
        public GameObject mainMenuPanel;
        public GameObject gameplayHUD;
        public GameObject pauseMenu;
        public GameObject endGameScreen;
        public GameObject loadingScreen;
        
        [Header("VR UI Elements")]
        public Transform leftHandMenu;
        public Transform rightHandMenu;
        public float menuDistance = 0.5f;
        public float menuScale = 0.001f; // Small scale for VR
        
        [Header("HUD Elements")]
        public TextMeshProUGUI healthText;
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI timerText;
        public Image healthBar;
        public Image specialAbilityProgress;
        
        [Header("Gameplay Feedback")]
        public GameObject damageIndicator;
        public GameObject hitMarker;
        public TextMeshProUGUI killFeedPrefab;
        public Transform killFeedContainer;
        
        private Canvas vrCanvas;
        private bool isMenuVisible;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeUI();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeUI()
        {
            vrCanvas = GetComponent<Canvas>();
            vrCanvas.renderMode = RenderMode.WorldSpace;
            
            // Hide all panels initially
            HideAllPanels();
            ShowMainMenu();
            
            // Initialize VR-specific UI
            SetupVRUI();
        }
        
        private void SetupVRUI()
        {
            // Attach UI to controllers
            if (leftHandMenu != null)
            {
                leftHandMenu.localPosition = new Vector3(0, 0, menuDistance);
                leftHandMenu.localScale = Vector3.one * menuScale;
            }
            
            if (rightHandMenu != null)
            {
                rightHandMenu.localPosition = new Vector3(0, 0, menuDistance);
                rightHandMenu.localScale = Vector3.one * menuScale;
            }
        }
        
        private void HideAllPanels()
        {
            mainMenuPanel?.SetActive(false);
            gameplayHUD?.SetActive(false);
            pauseMenu?.SetActive(false);
            endGameScreen?.SetActive(false);
            loadingScreen?.SetActive(false);
        }
        
        public void ShowMainMenu()
        {
            HideAllPanels();
            mainMenuPanel?.SetActive(true);
        }
        
        public void ShowGameplayHUD()
        {
            HideAllPanels();
            gameplayHUD?.SetActive(true);
        }
        
        public void ShowPauseMenu()
        {
            pauseMenu?.SetActive(true);
        }
        
        public void ShowEndGameScreen()
        {
            HideAllPanels();
            endGameScreen?.SetActive(true);
        }
        
        public void ShowLoadingScreen(string message = "Loading...")
        {
            HideAllPanels();
            loadingScreen?.SetActive(true);
            
            // Update loading text if available
            var loadingText = loadingScreen?.GetComponentInChildren<TextMeshProUGUI>();
            if (loadingText != null)
            {
                loadingText.text = message;
            }
        }
        
        public void UpdateHealth(float currentHealth, float maxHealth)
        {
            if (healthText != null)
            {
                healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{maxHealth}";
            }
            
            if (healthBar != null)
            {
                healthBar.fillAmount = currentHealth / maxHealth;
            }
        }
        
        public void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }
        
        public void UpdateTimer(float timeRemaining)
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(timeRemaining / 60);
                int seconds = Mathf.FloorToInt(timeRemaining % 60);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }
        
        public void ShowDamageIndicator(Vector3 damageDirection)
        {
            if (damageIndicator != null)
            {
                StartCoroutine(FlashDamageIndicator(damageDirection));
            }
        }
        
        private IEnumerator FlashDamageIndicator(Vector3 damageDirection)
        {
            damageIndicator.transform.rotation = Quaternion.LookRotation(damageDirection);
            damageIndicator.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            damageIndicator.SetActive(false);
        }
        
        public void ShowHitMarker()
        {
            if (hitMarker != null)
            {
                StartCoroutine(FlashHitMarker());
            }
        }
        
        private IEnumerator FlashHitMarker()
        {
            hitMarker.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            hitMarker.SetActive(false);
        }
        
        public void AddKillFeedMessage(string message)
        {
            if (killFeedPrefab != null && killFeedContainer != null)
            {
                var killFeedItem = Instantiate(killFeedPrefab, killFeedContainer);
                killFeedItem.text = message;
                StartCoroutine(FadeOutKillFeed(killFeedItem.gameObject));
            }
        }
        
        private IEnumerator FadeOutKillFeed(GameObject killFeedItem)
        {
            yield return new WaitForSeconds(3f);
            
            var canvasGroup = killFeedItem.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                float alpha = 1f;
                while (alpha > 0)
                {
                    alpha -= Time.deltaTime;
                    canvasGroup.alpha = alpha;
                    yield return null;
                }
            }
            
            Destroy(killFeedItem);
        }
        
        public void UpdateSpecialAbilityProgress(float progress)
        {
            if (specialAbilityProgress != null)
            {
                specialAbilityProgress.fillAmount = progress;
            }
        }
        
        public void ToggleVRMenu()
        {
            isMenuVisible = !isMenuVisible;
            
            if (leftHandMenu != null)
            {
                leftHandMenu.gameObject.SetActive(isMenuVisible);
            }
            
            if (rightHandMenu != null)
            {
                rightHandMenu.gameObject.SetActive(isMenuVisible);
            }
        }
        
        private void Update()
        {
            // Update VR UI positions based on controller positions
            if (isMenuVisible)
            {
                UpdateVRMenuPositions();
            }
        }
        
        private void UpdateVRMenuPositions()
        {
            // Update menu positions to follow controllers while maintaining readable angle
            if (leftHandMenu != null)
            {
                Vector3 targetPos = Camera.main.transform.position + 
                                  Camera.main.transform.forward * menuDistance;
                leftHandMenu.position = Vector3.Lerp(leftHandMenu.position, targetPos, Time.deltaTime * 10f);
                leftHandMenu.rotation = Quaternion.LookRotation(leftHandMenu.position - Camera.main.transform.position);
            }
            
            if (rightHandMenu != null)
            {
                Vector3 targetPos = Camera.main.transform.position + 
                                  Camera.main.transform.forward * menuDistance;
                rightHandMenu.position = Vector3.Lerp(rightHandMenu.position, targetPos, Time.deltaTime * 10f);
                rightHandMenu.rotation = Quaternion.LookRotation(rightHandMenu.position - Camera.main.transform.position);
            }
        }
    }
}
