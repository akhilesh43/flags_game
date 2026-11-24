# flags_game

# LovMates - Our Little World (Unity 2D Mobile Game)

A complete portrait 2D romantic couple life-simulation mobile game designed for Android (Portrait 1080x1920).
All **40 interactive screens** from the design specification are fully implemented, connected, and functional.

---

## 📱 Features & All 40 Connected Screens

The game self-boots in any Unity scene (`GameManager.Boot`) and creates its Canvas, Camera, EventSystem, and screen routing dynamically.

### 1. Authentication & Onboarding (Screens 1 - 5)
- **Screen 1: Splash** - Animated loading bar, automatic transition to Welcome.
- **Screen 2: Welcome** - Title, tagline, Start Game button.
- **Screen 3: Login** - Email/username, password, Google & Apple guest logins, Register/Forgot buttons.
- **Screen 4: Register** - Full registration form (name, email, password matching).
- **Screen 5: Forgot Password** - Password reset link dispatch.

### 2. Character Creation & Couple Connection (Screens 6 - 10)
- **Screen 6: Boy Selection** - Outfit & model selector with live dynamic avatar preview.
- **Screen 7: Girl Selection** - Outfit & model selector with live preview.
- **Screen 8: Customize** - Live customization of Hair, Outfit, Shoes, Accessories, Skin, and playing gender toggle.
- **Screen 9: Couple Connect** - Shareable invite code, partner code pairing, anniversary date picker.
- **Screen 10: Loading** - Dynamic loading sequence into the main world.

### 3. Main World & 9 Walkable Locations (Screens 11 - 20)
- **Screen 11: Home** - Virtual joystick walkable world with interactive live doors, HUD (level, coins, gems, affection), quick action grid, and bottom navigation bar.
- **Screen 12: Our Home** - Cozy love nest with doors to Bedroom, Living Room, Kitchen.
- **Screen 13: Bedroom** - Bed, decoration shortcuts, love diary access.
- **Screen 14: Living Room** - Movie night mini-activity with coin rewards.
- **Screen 15: Kitchen** - Couple cooking together activity.
- **Screen 16: Garden** - Flower planting activity.
- **Screen 17: Love Park** - Ferris wheel date activity with high coin rewards.
- **Screen 18: Beach** - Swimming activity and photo capture.
- **Screen 19: Love City** - Nightlife shopping strip connecting to Gift Shop.
- **Screen 20: Cafe & Restaurant** - Dining area connecting to Romantic Date.

### 4. Couple Activities & Minigames (Screens 21 - 25)
- **Screen 21: Romantic Date** - 4-stage interactive date with floating hearts and love points.
- **Screen 22: Couple Interaction** - 6 emotional reactions (Heart, Wow, Kiss, Smile, Hug, Gift) with SFX.
- **Screen 23: Hug & Kiss** - Intimate affection animation with sound effects and affection counter.
- **Screen 24: Dance** - Music minigame with dancing characters and rhythm loop audio.
- **Screen 25: Virtual Pet** - Feed, play, clean, and name your pet with live hunger/happiness bars.

### 5. Economy, Fashion & Interior Decor (Screens 26 - 28)
- **Screen 26: Gift Shop** - Buy and send gifts to your partner with coins.
- **Screen 27: Wardrobe** - Try on and wear unlocked outfits with live avatar preview.
- **Screen 28: Room Decoration** - Place lamps, plants, rugs, sofas, TVs, and royal beds in your room.

### 6. Social & Couple Memories (Screens 29 - 32)
- **Screen 29: Love Diary** - Write and save persistent daily couple diary entries.
- **Screen 30: Photo Gallery** - View saved in-game camera snapshots.
- **Screen 31: Couple Chat** - Live real-time style message exchange with bubble UI.
- **Screen 32: Love Letters** - Heartfelt romantic letters library.

### 7. Progression, Milestones & Settings (Screens 33 - 40)
- **Screen 33: Mood Tracker** - Daily mood log (Love, Happy, Okay, Sad, Angry) with streak tracker.
- **Screen 34: Couple Goals** - Track shared couple goals and claim coin rewards.
- **Screen 35: Special Days** - Countdown to anniversaries, birthdays, and relationship milestones.
- **Screen 36: Daily Missions** - Daily tasks (talk, photo, date, pet, visit) with rewards.
- **Screen 37: Settings** - Sound toggle, graphic quality selector, language picker.
- **Screen 38: Pause Screen** - Resume, settings, and main menu shortcuts.
- **Screen 39: Special Events** - Limited-time seasonal romance events and bonuses.
- **Screen 40: End / Celebration Screen** - Milestone celebration and thank-you screen.

---

## 🛠️ Project Structure

```
flags_game/
├── .github/
│   └── workflows/
│       ├── build-apk.yml        # Automated Android APK Build via GameCI
│       └── activation.yml       # 1-Click Unity License Activation helper
├── LovMates/                    # Unity 2022.3 LTS Project
│   ├── Assets/
│   │   ├── Art/Reference/       # 40 UI reference designs
│   │   ├── Editor/BuildAndroid.cs # Automated CI build script
│   │   ├── Resources/
│   │   │   ├── Audio/           # 11 synthesized audio & BGM WAV tracks
│   │   │   ├── Backgrounds/     # 40 screen backgrounds (01-40)
│   │   │   └── Characters/      # Character & Pet sprites
│   │   └── Scripts/
│   │       ├── Core/            # GameManager, SaveSystem, Services, UI, Art
│   │       └── Screens/         # All 40 screen implementations
│   ├── Packages/
│   │   └── manifest.json        # UGUI, Audio, Physics, ScreenCapture packages
│   └── ProjectSettings/
│       └── ProjectVersion.txt   # Unity 2022.3.45f1 configuration
├── .gitignore                   # Unity & OS ignore rules
└── README.md
```

---

## 🚀 Building the APK via GitHub Actions

This repository includes a fully configured **GitHub Actions CI/CD pipeline** (`.github/workflows/build-apk.yml`) that builds an Android `.apk` on every push to `main` and on manual trigger (`workflow_dispatch`).

### One-Time Setup for GitHub Actions Unity License

1. **Get an Activation File:**
   - Go to the **Actions** tab on your GitHub repository.
   - Select **Acquire Unity Activation File (.alf)** and click **Run workflow**.
   - Download the generated `.alf` file artifact from the workflow run.

2. **Activate Personal License:**
   - Visit [license.unity3d.com](https://license.unity3d.com) and upload the `.alf` file.
   - Select **Unity Personal Edition** (free) and download the resulting `.ulf` file.

3. **Add Secret to GitHub:**
   - Open your repo on GitHub: **Settings** > **Secrets and variables** > **Actions**.
   - Add a new repository secret:
     - Name: `UNITY_LICENSE`
     - Value: *(paste the full text of your `.ulf` license file)*

4. **Build APK:**
   - Go to **Actions** > **Build Android APK** > **Run workflow** (or simply push a commit).
   - Once completed, download `LovMates-APK` from the workflow summary artifacts.

---

## 💻 Local Development

1. Open **Unity Hub** and choose **Add project from disk**.
2. Select the `LovMates` folder.
3. Recommended Unity version: **Unity 2022.3 LTS** (or Unity 2021.3+ / Unity 6).
4. Open any scene and press **Play** — the game boots automatically without any manual scene setup!
5. To build an APK locally: click the top menu **LovMates > Build APK**.

