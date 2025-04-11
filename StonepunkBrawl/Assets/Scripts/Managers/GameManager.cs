using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System.Collections.Generic;

namespace StonepunkBrawl.Managers
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        public static GameManager Instance { get; private set; }
        
        [Header("Firebase")]
        private FirebaseAuth auth;
        private DatabaseReference database;
        
        [Header("Game Settings")]
        public GameMode currentGameMode;
        public int maxPlayersPerMatch = 4;
        public float matchDuration = 300f; // 5 minutes
        
        [Header("Prefabs")]
        public GameObject playerPrefab;
        public Transform[] spawnPoints;
        
        private bool isGameActive;
        private float matchTimer;
        private Dictionary<string, PlayerStats> playerStats;
        
        public enum GameMode
        {
            SinglePlayer,
            MultiplayerPvP,
            Training
        }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            playerStats = new Dictionary<string, PlayerStats>();
            InitializeFirebase();
        }
        
        private async void InitializeFirebase()
        {
            // Initialize Firebase
            var app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;
            database = FirebaseDatabase.DefaultInstance.RootReference;
            
            // Initialize dependencies
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus == DependencyStatus.Available)
            {
                Debug.Log("Firebase initialized successfully");
            }
            else
            {
                Debug.LogError($"Could not resolve Firebase dependencies: {dependencyStatus}");
            }
        }
        
        public void StartGame(GameMode mode)
        {
            currentGameMode = mode;
            
            switch (mode)
            {
                case GameMode.SinglePlayer:
                    StartSinglePlayerGame();
                    break;
                case GameMode.MultiplayerPvP:
                    StartMultiplayerGame();
                    break;
                case GameMode.Training:
                    StartTrainingMode();
                    break;
            }
        }
        
        private void StartSinglePlayerGame()
        {
            isGameActive = true;
            matchTimer = matchDuration;
            
            // Spawn local player
            SpawnPlayer(0);
            
            // Spawn AI opponents
            SpawnAIOpponents();
        }
        
        private void StartMultiplayerGame()
        {
            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
            }
            else if (PhotonNetwork.InRoom)
            {
                StartMatch();
            }
            else
            {
                JoinOrCreateRoom();
            }
        }
        
        private void StartTrainingMode()
        {
            isGameActive = true;
            // Spawn player and training dummy
            SpawnPlayer(0);
            SpawnTrainingDummy();
        }
        
        private void SpawnPlayer(int spawnIndex)
        {
            if (currentGameMode == GameMode.MultiplayerPvP)
            {
                PhotonNetwork.Instantiate(playerPrefab.name, 
                    spawnPoints[spawnIndex].position, 
                    spawnPoints[spawnIndex].rotation);
            }
            else
            {
                Instantiate(playerPrefab, 
                    spawnPoints[spawnIndex].position, 
                    spawnPoints[spawnIndex].rotation);
            }
        }
        
        private void JoinOrCreateRoom()
        {
            var roomOptions = new RoomOptions
            {
                MaxPlayers = (byte)maxPlayersPerMatch,
                IsVisible = true,
                IsOpen = true
            };
            
            PhotonNetwork.JoinRandomRoom();
        }
        
        public override void OnJoinRandomRoomFailed(short returnCode, string message)
        {
            // Create new room if joining failed
            PhotonNetwork.CreateRoom(null, new RoomOptions 
            { 
                MaxPlayers = (byte)maxPlayersPerMatch 
            });
        }
        
        public override void OnJoinedRoom()
        {
            Debug.Log($"Joined Room: {PhotonNetwork.CurrentRoom.Name}");
            
            if (PhotonNetwork.IsMasterClient)
            {
                StartMatch();
            }
        }
        
        private void StartMatch()
        {
            isGameActive = true;
            matchTimer = matchDuration;
            
            // Spawn all players
            for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                if (PhotonNetwork.LocalPlayer.ActorNumber - 1 == i)
                {
                    SpawnPlayer(i);
                }
            }
        }
        
        private void Update()
        {
            if (!isGameActive) return;
            
            if (matchTimer > 0)
            {
                matchTimer -= Time.deltaTime;
                if (matchTimer <= 0)
                {
                    EndMatch();
                }
            }
        }
        
        private void EndMatch()
        {
            isGameActive = false;
            
            if (currentGameMode == GameMode.MultiplayerPvP)
            {
                SaveMatchResults();
            }
            
            // Trigger end game UI
            UIManager.Instance?.ShowEndGameScreen();
        }
        
        private async void SaveMatchResults()
        {
            if (auth.CurrentUser == null) return;
            
            var matchData = new Dictionary<string, object>
            {
                { "timestamp", System.DateTime.UtcNow.ToString("o") },
                { "gameMode", currentGameMode.ToString() },
                { "players", playerStats }
            };
            
            await database.Child("matches")
                .Child(auth.CurrentUser.UserId)
                .Push()
                .UpdateAsync(matchData);
        }
        
        public void UpdatePlayerStats(string playerId, PlayerStats stats)
        {
            if (playerStats.ContainsKey(playerId))
            {
                playerStats[playerId] = stats;
            }
            else
            {
                playerStats.Add(playerId, stats);
            }
        }
        
        private void OnApplicationQuit()
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Disconnect();
            }
        }
    }
    
    [System.Serializable]
    public class PlayerStats
    {
        public int kills;
        public int deaths;
        public float damageDealt;
        public float damageTaken;
        public float accuracy;
    }
}
