# 🎮 Projet Unity Multijoueur – Collaboration via Photon 2

Bienvenue dans notre projet **Unity** multijoueur réalisé avec **Photon 2**, qui met l'accent sur la **collaboration asymétrique** entre deux rôles joueurs dans un environnement 3D interactif.

## 🎯 Objectif du Projet

Développer une scène Unity où **deux joueurs** (👤 Guider et 👣 Mover) coopèrent pour trier des **cubes colorés** dans les zones correctes (`Sales`, `Marketing`, `HR`) avant la fin d’un **compte à rebours de 2 minutes**.

---

## 🧱 Fonctionnalités Clés

### 👥 Système de Rôles Asymétriques
- **Guider** : Voit les métadonnées (catégorie des cubes et zones).
- **Mover** : Peut déplacer et déposer les cubes, mais ne voit pas les métadonnées.

### 🕹️ Mécaniques de Jeu
- **Déplacement libre** dans la scène.
- **Interaction avec les cubes** : drag, drop, throw via clics souris.
- **Objectif** : Placer tous les cubes correctement avant la fin du chronomètre.

### ⏱️ Game Manager
- Gère le **temps**, le **score**, les **états de jeu** (victoire, défaite).
- Synchronisation via `PhotonView` pour une **expérience multijoueur fluide**.

---

## 🛠️ Technologies Utilisées

| Outil              | Usage                                 |
|--------------------|----------------------------------------|
| Unity              | Développement 3D                      |
| Photon 2           | Système multijoueur                   |
| Visual Studio      | Programmation C#                      |
| Unity Asset Store  | Import de scène 3D de base            |

---

## 📜 Scripts Principaux

- `GameManager.cs` – Gestion du temps, score, état du jeu.
- `FPSController.cs` – Contrôle du Mover : déplacement, drag, drop.
- `PlayerSpawner.cs` – Attribution des rôles automatiquement.
- `CubeSpawner.cs` – Génération aléatoire des cubes avec métadonnées.
- `CubeMetadata.cs` – Attribut les catégories et couleurs aux cubes.
- `CategoryZone.cs` – Détection et validation des cubes bien placés.
- `PlayerCameraEnabler.cs` – Gestion des caméras selon le rôle.

---

## 🔗 Implémentation Photon 2

- Ajout de l’**App ID** depuis Photon Dashboard.
- Deux scènes : `BuilderScene` (accueil), `GameScene` (jeu multijoueur).
- Connexion automatique et attribution des rôles.

---

## 🎮 Déroulement du Jeu

1. Deux joueurs se connectent.
2. Le **compte à rebours** de 2 minutes commence.
3. Le **Guider** oriente le **Mover** pour bien trier les cubes.
4. Conditions de fin :
   - ✅ Victoire : Tous les cubes placés correctement à temps.
   - ❌ Défaite : Temps écoulé sans réussir.

---

## 🧠 Système d’Information Asymétrique

- Les **métadonnées** (catégories) sont **invisibles au Mover**.
- Gestion via des **layers spécifiques** exclus du champ de vision.

---

## 📦 Livrables

- 🎮 Projet Unity complet (scènes, scripts, assets)
- 🎥 Vidéo démonstrative
- 📄 Document explicatif
- 📁 Ce fichier `README.md`

---

## 👨‍💻 Contribué par : Groupe 3
Ce projet a été développé dans un cadre pédagogique pour expérimenter **la coopération multijoueur dans Unity**, en utilisant les principes du **game design asymétrique**.

---

🧠 *"Collaboration is the key to success!"*
<img width="769" height="709" alt="image" src="https://github.com/user-attachments/assets/2a0b304c-85e1-4daf-9402-75f30ed55f5c" />
<img width="291" height="175" alt="image" src="https://github.com/user-attachments/assets/c20c149c-1b0d-46e0-bd96-9f1a75df4ffe" />
<img width="766" height="630" alt="image" src="https://github.com/user-attachments/assets/18778148-d638-483c-86e8-3e00393d7d02" />
<img width="770" height="802" alt="image" src="https://github.com/user-attachments/assets/3c9041f4-5043-4003-9863-27d93c85138d" />
<img width="660" height="765" alt="image" src="https://github.com/user-attachments/assets/e27e7ad3-5de9-4144-b0c9-fdeed8f68bf4" />
<img width="594" height="344" alt="image" src="https://github.com/user-attachments/assets/61d5367a-da2e-40a1-90ea-39de82332f98" />
<img width="566" height="273" alt="image" src="https://github.com/user-attachments/assets/d83d3c88-aeb2-4c6c-8da3-d973987fd4a5" />
<img width="546" height="366" alt="image" src="https://github.com/user-attachments/assets/2f33c42f-357b-496f-aef4-754117e1c13e" />
<img width="584" height="610" alt="image" src="https://github.com/user-attachments/assets/456c43c3-ed52-450f-b82d-7c474e02555c" />
<img width="633" height="600" alt="image" src="https://github.com/user-attachments/assets/f5390db3-6d1a-4fd1-b1b2-45c75e574602" />
<img width="510" height="565" alt="image" src="https://github.com/user-attachments/assets/0f7ea11c-89e7-4081-81a3-bd749479e68a" />

