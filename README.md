# Toast & Tales

A Heritage AR Experience for Ya Kun Kaya Toast <br>
Developers: Toh Rui Min & Geng Bai Hui

## 1. Project Overview

Toast & Tales is an Augmented Reality (AR) mobile game designed to rejuvenate interest in the heritage of Singapore’s iconic Ya Kun Kaya Toast. By using AR to bring static images to life, players can interact with virtual history. The experience is gamified with a progress tracking system that syncs real-time achievements to a companion web portal, and features an interactive AR Face Filter to engage users socially.

- **Genre**: AR Simulation / Educational
- **Target Platform**: Android Mobile
- **Key tech**: Unity (AR Foundation, ARCore Face Tracking), Google Firebase (Auth & Realtime Database)

## 2. Platforms & Hardware Requirements

- **Hardware**: Android Smartphone with rear-facing camera (for Game Levels) and front-facing camera (for Face Filters).
- **Software**: Android 7.0 (Nougat) or higher (Must support ARCore).
- **Network**: Active Internet connection required for Login and Database synchronization.
- **Phisical Requirements**: The game requires specific image targets (photos of Kopi, Toast, and Set Meal) to function.

## 3. How to Install & Run

1. **Install**: Download the .apk file from GitHub and transfer the .apk file to your Android device and install it (ensure "Install from Unknown Sources" is enabled).
2. **Prepare Markers**: Have the 5 target images ready (displayed on a computer/phone screen or printed on paper).
   - _Marker 1_: Kopi Photo
   - _Marker 2: Kettle Photo
   - _Marker 3_: Kaya Toast Photo
   - _Marker 4_: Knife Photo
   - _Marker 5_: Full Set Meal Photo
3. **Lauch**: Open the Toast & Tales app.

## 4. Walkthrough & Controls

### Phase 1: Start Menu

1. **Login / Sign up**: Enter a valid email and password to create an account. This enables the firebase tracking system.
2. **Welcome Hub**: You will see three interactive buttons:
   - **Read History**: Tap to read the Ya Kun Story.
   - **Start Journey**: Tap to start the gameplay and select a cooking level.
   - **AR Filter**: Tap to enter Phase 3 (AR Filter).

### Phase 2: Gameplay Levels

Select a dish from the menu. Point your camera at the matching marker to begin.

- Level 1: The Perfect Brew (Kopi)
  - Scan the kopi marker, and at the same time scan the Kettle Marker.
  - Tap and drag the kettle to pour coffee into the cup.
  - Unlocks the "Secret Roast" story panel in-game (badge unlocked on website).
- Level 2: The Charcoal Grill (Toast)
  - Scan the toast and knife marker.
  - Swipe / Drag the knife to spread kaya & butter on the bread.
  - Unlocks the "Labour of Love" story panel in-game (badge unlocked on website).
- Level 3: The Heritage Set (Full Meal)
  - Unlocks the final Heritage Story Panel in-game. (Badge unlocked on Website).

### Phase 3: AR Face Filter

1. Tap the "AR Filter" button on homepage.
2. The front camera will active. Position your face within the frame.
3. The interesting AR filter with title, toast and coffee will apply to your face.
4. You can take a screenshot to save the photo to your album!

### Phase 4: Website

1. Open index.html in a web browser
2. Log in using the **same credentials** created in the app.
3. Navigate to the user profile page.
4. This is the exclusive area where you can view you earned badges (kopi, toast, and full set legend).

## 5. Cheats, Hacks & Solutions

- Infinite Reset: If the AR object drifts or disappears, simply point the phone camera away from the marker and then point it back immediately to respawn the object.
- Progress Reset: To re-test the "locked" badge state on the website, just need to refresh the browser, or log out and re-login.
- Marker Solution: The game is strictly marker-based. If the camera does not detect the specific Ya Kun photo, the 3D models will not spawn. Ensure markers are well-lit and not glossy/reflective.

## 6. Limitation & Know Bugs

- Face Tracking: The AR Filter requires a well-lit environment to detect the user's face accurately. Dark rooms may cause the models to jitter.
- Physics Glitch: Rarely, if the user drags the knife too fast in the Toast level, it may pass through the bread collider without triggering the "spread" effect. Fix: Drag slowly.

## 7. Credits & References

### Code & Plugins:

- [Unity AR Foundation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.4/manual/index.html) & [Unity ARCore](https://docs.unity3d.com/Packages/com.unity.xr.arcore@3.0/manual/index.html): Used for image tracking and face tracking.
- [Google Firebase SDK](https://firebase.google.com/docs/firestore/client/libraries): Used for Authentication and Realtime Database management.

### Audio Assets:

- Background Music:
  - _Lost Signal Coffee_ by Pixabay [https://pixabay.com/music/beats-lost-signal-coffee-371869/]
- Sound Effects:
  - _Water Pouring_ by Pixabay [https://pixabay.com/sound-effects/water-pouring-405458/]
  - _Cream/Sizzle_ by Pixabay [https://pixabay.com/sound-effects/cream-90275/]
  - _Object Place_ by Pixabay [https://pixabay.com/sound-effects/put-things-on-table-415924/]
  - Victory by Pixabay [https://pixabay.com/sound-effects/orchestral-win-331233/]
  - Button Click by Pixabay [https://pixabay.com/sound-effects/bubble-pop-06-351337/]

### 3D Models

- Coffee Cup: _Coffee Cup_ by Sketchfab [https://sketchfab.com/3d-models/coffee-cup-3f6f89080498447ea3dc156b2f363aea]
- Kettle: _Polished Tea Kettle_ by Sketchfab [https://sketchfab.com/3d-models/polished-tea-kettle-7076495f9b5946c596dbd3475972a1db]
- Toast: _Bread Toast and Grilled Cheese_ by Sketchfab [https://sketchfab.com/3d-models/bread-toast-and-grilled-cheese-game-ready-2af8c059607d430f834aa8b4ee8e95dd]
- Knife: _Butter Knife_ by Sketchfab [https://sketchfab.com/3d-models/butter-knife-f44ca5eb44fe444d8ad1890cd5cfccf0]
- Tray: _Metal Tray_ by Sketchfab [https://sketchfab.com/3d-models/metal-tray-baf3242c524f4e4483d7f6df5f0d9848]

### Images & Textures:

- Ya Kun Heritage Photos: Sourced from official [Ya Kun Website](https://app.yakun.com/) (Educational purpose only).
- UI Textures:
  - _Paper Texture_: Pinterest [https://www.pinterest.com/pin/83316661854063196/]
  - _Menu Texture_: Pinterest [https://www.pinterest.com/pin/203858320628416519/]

### Special Thanks:

Ya Kun Kaya Toast: For the brand inspiration and historical context.
