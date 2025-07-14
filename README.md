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
