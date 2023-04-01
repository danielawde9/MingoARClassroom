# Mingo AR Classroom Design Document

## Overview

Mingo AR Classroom is an AR educational app inspired by Kahoot. It aims to gamify the learning experience for students and teachers by combining the power of AR with engaging gameplay, creating an immersive and interactive learning environment.

## App Architecture

The app's architecture is designed using a modular approach, focusing on clear separation of responsibilities and making it easy to extend and maintain.

### Core Modules

1. **AuthManager**: Handles user authentication, registration, and account management using Firebase Authentication.
2. **UserManager**: Manages user profiles, including user roles (teacher, student, or personal) and other related information.
3. **DatabaseManager**: Manages communication with the Firebase Realtime Database for storing and retrieving quiz data, questions, and user scores.
4. **ContentManager**: Handles the creation, modification, and deletion of quizzes and questions, as well as managing quiz access codes.
5. **QuestionManager**: Manages the display and progression of questions in the app, as well as user interactions with the questions.
6. **QuizManager**: Manages quiz sessions, including creating new quizzes, joining existing quizzes, and managing game flow and scoring.
7. **ARManager**: Handles AR functionalities, such as detecting image targets and displaying 3D content.
8. **MultiplayerManager**: Manages real-time multiplayer support, synchronizing game state and user interactions between devices.
9. **UIManager**: Manages the user interface elements in the app, including updating progress bars, score displays, and other UI elements.
10. **SceneSwitcher**: Provides functionality for loading different Unity scenes using SceneManager.
11. **LeaderboardManager**: Manages leaderboards, displaying top-scoring users and allowing users to compare their scores with others.
12. **AnalyticsManager**: Tracks user interactions and performance, providing insights into user behavior and app usage.

### Additional Features (Optional)

1. **MessagingManager**: Handles in-app messaging between users, such as teacher-student communication or group chat.
2. **ExternalContentIntegration**: Provides functionality to import quizzes and questions from external content sources (e.g., an API or other educational platforms).

## Development Plan

1. **Pre-production**: Define app requirements and features, research appropriate technologies, and plan the app's architecture.
2. **Core development**: Implement the core modules outlined above, ensuring that each module is developed and tested independently.
3. **Polish and additional features**: Implement any additional features or improvements, such as improved UI transitions or integration with external content sources.
4. **Testing and optimization**: Perform thorough testing of the app, addressing any bugs or issues discovered during testing, and optimize the app based on test results and user feedback.
5. **Deployment and post-release**: Prepare the app for deployment to app stores, create marketing materials, and plan for post-release updates and support.

This design document should serve as a starting point for the development of the Mingo AR Classroom app. You can refine and expand upon this document as needed throughout the development process.

### Required Files

1.  UserManager:
    - Handles user registration, login, and authentication.
    - Manages user profiles (including avatars, roles, and account details).
2.  QuizManager:
    - Creates and manages quizzes (including questions, answers, and metadata).
    - Handles quiz code generation and sharing.
    - Manages quiz session state (lobby, in-progress, and completed).
3.  QuestionManager:
    - Manages individual question types (multiple choice, true/false, etc.).
    - Handles question validation and scoring.
    - Manages question timers.
4.  ARManager:
    - Handles AR session initialization and management.
    - Manages AR content (placement, interaction, and visualization).
    - Handles AR-specific game mechanics.
5.  MultiplayerManager:
    - Manages real-time multiplayer sessions (connecting players, synchronizing game states, etc.).
    - Handles player communication (in-app messaging).
6.  LeaderboardManager:
    - Manages player scores and rankings.
    - Handles data storage and retrieval for leaderboards.
7.  AnalyticsManager:

    - Collects and processes data on player performance and app usage.
    - Provides insights to teachers and players.

8.  ContentManager:
    - Manages the creation and storage of custom quizzes.
    - Handles integration with external content sources (e.g., LMS, educational platforms).
9.  UIManager:
    - Handles user interface elements, such as menus, buttons, and progress bars.
    - Manages UI transitions, animations, and visual feedback.
10. SceneSwitcher:

    - Provides functionality for loading different Unity scenes using SceneManager.

11. DatabaseManager:

    - Handles communication with the Firebase Realtime Database for storing and retrieving data.

12. AuthManager:

    - Manages user authentication, including sign-up, login, and password reset.

## Structure

```
Assets
├── Mingo
│ ├── Scenes
│ │ ├── MainMenu
│ │ ├── GameScene
│ │ └── QuizCreator
│ ├── Scripts
│ │ ├── Managers
│ │ │ ├── AuthManager.cs
│ │ │ ├── UserManager.cs
│ │ │ ├── GameManager.cs
│ │ │ └── DatabaseManager.cs
│ │ ├── UI
│ │ │ ├── UIManager.cs
│ │ │ ├── SceneSwitcher.cs
│ │ │ └── LoadingScreenManager.cs
│ │ ├── Gameplay
│ │ │ ├── QuestionManager.cs
│ │ │ ├── Question.cs
│ │ │ └── QuestionData.cs
│ │ ├── Account
│ │ │ ├── Login.cs
│ │ │ ├── Signup.cs
│ │ │ └── ResetPassword.cs
│ │ └── Utils
│ │ └── GameData.cs
│ ├── Prefabs
│ │ ├── PlayerAvatar
│ │ ├── UIElements
│ │ └── ARObjects
│ ├── Materials
│ ├── Textures
│ └── Audio
└── Plugins
├── Firebase
└── OtherThirdPartyPlugins
```
