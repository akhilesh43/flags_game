# LovMates - Our Little World (Unity project)

All 40 screens from the design sheet are implemented in code (uGUI, portrait 1080x1920).

## Setup (2 min)
1. Unity Hub -> New project -> **2D (Built-in or URP)**, Unity 2021.3 LTS or newer (Unity 6 ok).
2. Close Unity, copy this project's `Assets/` folder into the new project (merge).
3. Open project, open any scene (SampleScene), press **Play**. The game boots itself (GameManager.Boot) - no scene wiring needed.
4. Game view: set aspect to 9:16 / Simulator -> a phone.

## Flow
Splash -> Welcome -> Login/Register/Forgot -> Boy select -> Girl select -> Customize -> Couple connect -> Loading -> Home.

## Live "Enter" buttons
- Home: walk with the joystick. Near a door (Our Home, Garden, Love City, Beach, Love Park, Cafe) a live **Enter** button appears and takes you inside.
- Home -> Places: all 9 location cards (Our Home, Bedroom, Living Room, Kitchen, Garden, Love Park, Beach, Love City, Cafe) each with a live **Enter** button.
- Inside a location: walk, Interact, Hug & Kiss, Dance + location extras (Cafe -> Romantic Date, Kitchen -> Cook, Beach -> Photo ...).

## Add your art (optional, drop-in)
- `Assets/Resources/Backgrounds/NN.png` (NN = screen number 01..40, import as Sprite) replaces the gradient background of that screen.
- `Assets/Resources/Characters/boy_0.png ... boy_3.png, girl_0.png ... girl_3.png, pet.png` replace placeholder characters.
- `Assets/Resources/Audio/click.wav, dance.wav` optional sounds. `Resources/Fonts/Main` optional font.
- `Assets/Art/Reference/` has the 40 cropped screens from your design (reference only).

## Code map (Assets/Scripts)
- Core/GameManager.cs - boot, navigation, economy, missions, photo capture
- Core/UI.cs, Widgets.cs - UI factory, avatar, joystick, walkable world
- Core/Services.cs - local auth + partner backend (swap for Firebase/Photon)
- Screens/* - one class per screen group

## Not included (needs SDK / server)
Real Google/Apple login, real email reset, real-time partner chat/sync, localization strings. Hook points are in Services.cs, LoginScreen, ChatScreen.

## Assets added (v2)
- `Resources/Audio`: bgm, romantic (loops), dance (loop), click, coin, success, error, heart, kiss, message, whoosh (all synthesized WAV, royalty-free).
- `Resources/Backgrounds/01-40.jpg`: clean bokeh backgrounds (UI screens) + scene art for 12-20.
- `Resources/Characters`: boy_0..3, girl_0..3 (4 outfit colours), pet.png (cut from your design).
- `Core/Art.cs` loads these without any manual "Sprite" import setting.
- Music/SFX are wired in GameManager (Sfx(), UpdateMusic()). Settings > Sound toggles all audio.

## Build the APK
**Local:** Unity Hub -> install Unity 2022.3 LTS with *Android Build Support (SDK/NDK/OpenJDK)* -> open this folder as a project -> menu **LovMates > Build APK** -> `Builds/LovMates.apk`.
**Cloud (no install):** push to GitHub, add secrets UNITY_LICENSE / UNITY_EMAIL / UNITY_PASSWORD, run the "Build LovMates APK" workflow, download the artifact.
