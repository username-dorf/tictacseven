# Tic Tac Seven

<p align="center">
    <img src="https://github.com/username-dorf/tictacseven/wiki/images/img_logo_500x500.png" width="29%" />
</p>

Tic Tac Seven is a **small pet project** built to experiment with:

- how to **integrate neural networks into Unity** (as a game bot), and  
- how to **create and train a custom neural network** (AlphaZero-style) on a non-trivial board game.

On top of that, it’s also a fully playable extended Tic-Tac-Toe game with bots, local network play, and a styled UI.

> ⚠️ **Note:** The actual **neural network bot code** (AlphaZero-style training and inference) is **not included in this repository** – it lives in a separate private project.  
> This repo contains the game logic, integration points, and environment used by that bot.

---

## Screenshots

- Gameplay vs Bot  
<p align="center">
  <img src="https://github.com/username-dorf/tictacseven/wiki/images/img_menu.png" width="29%" />
  <img src="https://github.com/username-dorf/tictacseven/wiki/images/img_gameplay.png" width="29%" />
  <img src="https://github.com/username-dorf/tictacseven/wiki/images/img_round_result.png" width="29%" />
</p>


- Local Network Match (FishNet)  
<p align="center">
  <img src="https://github.com/username-dorf/tictacseven/wiki/images/img_lan_menu.png" width="29%" />
  <img src="https://github.com/username-dorf/tictacseven/wiki/images/img_connecting_to.png" width="29%" />
  <img src="https://github.com/username-dorf/tictacseven/wiki/images/img_receive_connection.png" width="29%" />
</p>

- Skins & Avatars / UI  
<p align="center">
    <img src="https://github.com/username-dorf/tictacseven/wiki/images/img_profile_settings.png" width="29%" />
</p>

---

## Key Features

- 🤖 **AI vs Player**
  - Bot powered by an **AlphaZero-style neural network** (self-play + MCTS conceptually).

- 🌐 **Network Play (Local Wi-Fi)**
  - Local multiplayer over Wi-Fi using **FishNet**.
  - One device hosts, others join for LAN matches.

- 🎮 **Game Experience & Presentation**
  - **UI system** with multiple windows/screens and overlays.
  - **Particles and visual effects** for moves, wins, and feedback.
  - **Sound system** for UI actions and gameplay events.

- 🧑‍🎨 **Customization**
  - **Skins and avatars** for users to personalize their appearance.

---

## Status

### ✅ Implemented / Working

- [x] Extended Tic Tac Toe rule set (**Tic Tac Seven**).
- [x] Core game loop (turns, win/draw detection, board state handling).
- [x] **AlphaZero-style neural bot integration**
  - Game environment and interfaces that the neural bot uses.
  - Bot runs using an external model (not stored in this repo).
- [x] **Play vs Bot** mode.
- [x] **Local network multiplayer via Wi-Fi** using FishNet.
- [x] **UI / window system** for screens, dialogs, overlays.
- [x] **Particles & visual effects** for gameplay and UI.
- [x] **Skins and avatars** for players.
- [x] **Sound system** (SFX for moves, clicks, victory, etc.).

---

### 🧩 Planned / To Do

- 🎯 **Arcade Mode**
  - [ ] Board **modifiers / power-ups** that change rules or add effects.
  - [ ] Separate arcade-style scoring / progression.

- 🌍 **Online Play via Dedicated Server**
  - [ ] Online matches using a **separate dedicated server**, not limited to the same Wi-Fi network.
  - [ ] Basic matchmaking / lobbies (long-term).

- 🛒 **Monetization / Meta**
  - [ ] **Purchases** for skins or avatars.

- 📊 **Analytics**
  - [ ] **Firebase Analytics** integration:
    - Session tracking.

- 📺 **Ads**
  - [ ] **AdMob** integration:
    - Rewarded ads (optional bonuses).
    - Other ad formats (interstitial / banner).

