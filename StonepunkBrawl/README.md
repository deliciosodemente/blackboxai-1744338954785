# Stonepunk Brawl

A cross-platform VR fighting game set in a unique stone-age punk universe. Battle with primitive yet technologically enhanced weapons in an immersive VR environment.

## Features

- **Cross-Platform VR Support**
  - Oculus Quest/Quest 2
  - SteamVR
  - Mobile VR
  - Smart TV platforms
  - iOS devices

- **Immersive Combat System**
  - Motion-controlled melee combat
  - Gesture-based special moves
  - Physical blocking and dodging
  - Dual-wielding weapons

- **Game Modes**
  - Single Player Campaign
  - Online PvP Matches
  - Local Multiplayer
  - Training Mode

- **Character Classes**
  - Stone Warrior
  - Agile Hunter
  - Tech Shaman

## Technical Requirements

- Unity 2022.3 LTS
- Firebase SDK
- Photon PUN 2
- Oculus Integration SDK
- Google VR SDK

## Setup Instructions

1. **Unity Setup**
   ```bash
   # Clone the repository
   git clone https://github.com/yourusername/stonepunk-brawl.git
   
   # Open project in Unity 2022.3 LTS
   ```

2. **Firebase Setup**
   - Create a new Firebase project
   - Download and import the Firebase Unity SDK
   - Add your Firebase configuration to `firebase.json`

3. **Photon Setup**
   - Create a Photon account at https://www.photonengine.com
   - Set up a new Photon PUN application
   - Add your Photon App ID to PhotonServerSettings

4. **Platform SDKs**
   - Install Oculus Integration from Asset Store
   - Import Google VR SDK
   - Configure platform-specific settings

## Development

### Project Structure
```
StonepunkBrawl/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── PlayerController.cs
│   │   │   ├── WeaponController.cs
│   │   │   └── Weapon.cs
│   │   └── Managers/
│   │       ├── GameManager.cs
│   │       ├── VRManager.cs
│   │       └── UIManager.cs
│   ├── Prefabs/
│   ├── Models/
│   ├── Materials/
│   └── Scenes/
└── ProjectSettings/
```

### Building for Different Platforms

1. **Oculus Quest**
   - Switch platform to Android
   - Enable Oculus as XR Plugin
   - Build APK

2. **iOS**
   - Switch platform to iOS
   - Configure signing
   - Build Xcode project

3. **Smart TV**
   - Switch platform to Android TV
   - Configure TV input
   - Build APK

## Optimization

- Dynamic resolution scaling
- LOD system
- Occlusion culling
- Asset streaming
- Platform-specific graphics settings

## Firebase Integration

### Features
- User authentication
- Cloud saves
- Leaderboards
- Match history
- Analytics

### Database Structure
```json
{
  "users": {
    "userId": {
      "stats": {},
      "inventory": {},
      "progress": {}
    }
  },
  "matches": {
    "matchId": {
      "players": [],
      "scores": {},
      "timestamp": ""
    }
  }
}
```

## Deployment

1. **Play Store (Android)**
   - Build signed APK
   - Create store listing
   - Submit for review

2. **App Store (iOS)**
   - Build through Xcode
   - Create App Store Connect listing
   - Submit for review

3. **Firebase Hosting**
   - Configure hosting settings
   - Deploy backend services
   ```bash
   firebase deploy
   ```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit changes
4. Push to branch
5. Create Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support, email support@stonepunkbrawl.com or join our Discord server.
