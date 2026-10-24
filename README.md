# 🎮 Tic Tac Toe - Full Stack Web Application

A browser-based Tic Tac Toe application built with an **Angular** frontend and a **.NET 8 Web API** backend running locally. Supports two-player local multiplayer, smart AI computer opponent mode, move history tracking, undo functionality, and a session-level scoreboard.

---

## 📋 Table of Contents
1. [Project Overview](#1-project-overview)
2. [Tech Stack](#2-tech-stack)
3. [Features Implemented](#3-features-implemented)
4. [How to Run the Backend Locally](#4-how-to-run-the-backend-locally)
5. [How to Run the Frontend Locally](#5-how-to-run-the-frontend-locally)
6. [API Endpoint Summary](#6-api-endpoint-summary)
7. [How to Run Tests](#7-how-to-run-tests)
8. [AI Tools and Prompt Summary](#8-ai-tools-and-prompt-summary)
9. [Design Decisions](#9-design-decisions)
10. [Clarifications and Assumptions](#10-clarifications-and-assumptions)
11. [Known Limitations](#11-known-limitations)
12. [Future Improvements](#12-future-improvements)

---

## 1. Project Overview
This project is an enterprise-grade full-stack implementation of the classic Tic Tac Toe game designed to be easy for review and demonstration. The architecture adheres to clean separation of concerns:
- **Backend as Source of Truth**: The .NET Web API manages all game state, board state, turn alternation, move validation, win/draw detection, computer opponent AI calculation, and scoreboard tracking.
- **Modern Angular Frontend**: The Angular single-page application interacts with the backend strictly via REST APIs and renders the UI reactively with glassmorphism aesthetics, neon glowing indicators, smooth animations, move history, and audio feedback.

---

## 2. Tech Stack

### Frontend
- **Framework**: Angular 18 (Standalone Components)
- **Language**: TypeScript
- **Styling**: Vanilla CSS with custom Design System tokens (Glassmorphism, Neon glow accents, Google Fonts: *Outfit* & *Plus Jakarta Sans*)
- **HTTP Client**: Angular `HttpClient` (`provideHttpClient`) with RxJS observables
- **Sound Effects**: Synthesized dynamic audio via native browser **Web Audio API** (zero external asset dependencies)
- **Testing**: Jasmine & Karma with ChromeHeadless

### Backend
- **Framework**: ASP.NET Core (.NET 8 Web API)
- **Language**: C#
- **API Style**: RESTful JSON API
- **State Storage**: In-Memory Thread-Safe Service (`ConcurrentDictionary` and synchronization primitives)
- **API Documentation**: OpenAPI / Swagger UI
- **Testing**: xUnit with 28 comprehensive unit tests covering all rules, validations, and edge cases

---

## 3. Features Implemented

### 🎯 Core Gameplay
- **Standard 3×3 Board**: Interactive grid with cells that display `X` or `O`. Empty cells feature ghost hover indicators.
- **Turn Alternation**: Alternates turns between Player X and Player O. Displays active turn status with animated pulsing ring.
- **Validation**: Rejects invalid moves (out of bounds, cell already occupied, wrong player's turn, moves after game completion).
- **Win Detection**: Automatically checks all 3 rows, 3 columns, and 2 diagonals. When a win occurs:
  - Highlights the 3 winning cells with an animated emerald pulse.
  - Displays a victory banner.
  - Automatically locks the board against further moves.
  - Increments the winner's score on the session scoreboard.
- **Draw Detection**: Accurately detects when all 9 cells are filled without a winner, locks the board, displays a draw banner, and increments the draw counter.

### 🤖 Smart Computer Opponent Mode (with Invincible Minimax & Standard AI)
- Selectable game modes: **Two Player Mode** and **Vs Computer Mode**.
- In Computer Mode, players can toggle between two AI engine levels:
  - 🧠 **Master AI (Invincible Minimax)**: Evaluates the complete game-state search tree with recursive depth evaluation. Mathematically guaranteed **never to lose** — will always seize winning opportunities or force a draw against perfect play!
  - 🎯 **Standard AI (Heuristic)**: Follows the original 5-tier rule priority specification.
- Selectable game modes: **Two Player Mode** and **Vs Computer Mode**.
- In Computer Mode:
  - Human player controls **X**; Computer automatically controls **O**.
  - Computer automatically calculates and executes its move immediately following the human's move.
  - Move selection follows strict strategic priority:
    1. **Win**: If O can win in this move, take the winning cell.
    2. **Block**: If X can win on the next move, block X's winning cell.
    3. **Center**: Take center cell `(1, 1)` if available.
    4. **Corner**: Take an available corner cell (`(0,0)`, `(0,2)`, `(2,0)`, `(2,2)`).
    5. **Any**: Take any remaining available edge cell (`(0,1)`, `(1,0)`, `(1,2)`, `(2,1)`).

### 📜 Move History & Undo
- **Move History**: Real-time table logging Move #, Player, Position (e.g. `Row 1, Column 1`), and timestamp.
- **Undo Last Move**:
  - **Two Player Mode**: Undoes only the most recent move and restores the previous player's turn.
  - **Computer Mode**: Undoes both the computer's move and the player's previous move together, restoring Player X's turn.
  - **Option A Implementation**: Undo is disabled once a game is won or drawn to ensure finality of the completed game and scoreboard integrity.
  - Disabled when no moves are available.

### 🏆 Scoreboard
- Tracks session-level stats: **Player X Wins**, **Player O Wins**, **Draws**, and Total Games.
- Updates once upon game completion.
- **Reset Game**: Clears the board, moves, and win status while preserving the scoreboard.
- **Reset Scoreboard**: Dedicated button to reset win/draw tallies to zero.

### 🔊 Audio & UX Polish
- Dynamic synthesizer sounds for moves, wins, draws, undos, and resets.
- Sound toggle button (Mute / Unmute).
- Real-time backend connectivity badge.

---

## 4. How to Run the Backend Locally

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Steps
1. Open a terminal and navigate to the backend directory:
   ```bash
   cd backend/TicTacToe.Api
   ```
2. Restore and run the Web API:
   ```bash
   dotnet run --urls "http://localhost:5000"
   ```
3. The backend will start listening at:
   - **API Base URL**: `http://localhost:5000`
   - **Swagger UI**: `http://localhost:5000/swagger`

---

## 5. How to Run the Frontend Locally

### Prerequisites
- [Node.js](https://nodejs.org/) (v18+ or v20+ recommended)
- `npm` (installed with Node.js)

### Steps
1. Open a second terminal and navigate to the frontend directory:
   ```bash
   cd frontend
   ```
2. Install dependencies (if not already installed):
   ```bash
   npm install
   ```
3. Start the Angular development server:
   ```bash
   npm start
   ```
   *(Or run: `npx ng serve --port 4200`)*
4. Open your browser and navigate to:
   ```
   http://localhost:4200
   ```

---

## 6. API Endpoint Summary

| Method | Endpoint | Description | Request Body | Response |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/games` | Creates a new game session | `{ "mode": "TwoPlayer" \| "Computer" }` | `GameStateResponse` |
| `GET` | `/api/games/{id}` | Fetches current game state | *None* | `GameStateResponse` |
| `POST` | `/api/games/{id}/moves` | Submits a player move | `{ "player": "X", "row": 0, "column": 0 }` | `GameStateResponse` |
| `POST` | `/api/games/{id}/undo` | Undoes the last move / pair | *None* | `GameStateResponse` |
| `POST` | `/api/games/{id}/reset` | Resets current board & history | *None* | `GameStateResponse` |
| `GET` | `/api/scoreboard` | Retrieves scoreboard counts | *None* | `ScoreboardResponse` |
| `POST` | `/api/scoreboard/reset` | Resets scoreboard to zero | *None* | `ScoreboardResponse` |

### Sample `GameStateResponse` Payload
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "board": [
    ["X", "O", ""],
    ["", "X", ""],
    ["", "", ""]
  ],
  "currentPlayer": "O",
  "mode": "TwoPlayer",
  "status": "InProgress",
  "winner": null,
  "winningCells": [],
  "moveHistory": [
    {
      "moveNumber": 1,
      "player": "X",
      "row": 0,
      "column": 0,
      "position": "Row 1, Column 1",
      "timestamp": "2026-10-24T18:00:00Z"
    },
    {
      "moveNumber": 2,
      "player": "O",
      "row": 0,
      "column": 1,
      "position": "Row 1, Column 2",
      "timestamp": "2026-10-24T18:00:02Z"
    },
    {
      "moveNumber": 3,
      "player": "X",
      "row": 1,
      "column": 1,
      "position": "Row 2, Column 2",
      "timestamp": "2026-10-24T18:00:05Z"
    }
  ],
  "scoreboard": {
    "xWins": 1,
    "oWins": 0,
    "draws": 0,
    "totalGames": 1
  }
}
```

---

## 7. How to Run Tests

### Backend Unit Tests (xUnit)
Run all 28 backend unit tests from the repository root:
```bash
dotnet test backend/TicTacToe.Tests/TicTacToe.Tests.csproj
```
**Test Coverage Includes:**
- Valid move processing & turn alternation
- Invalid move handling (occupied cells, out of bounds, wrong player's turn)
- Prevention of moves after game completion
- Row, column, and diagonal win detection
- Winning coordinate calculation
- Draw detection upon full board
- Reset game behavior (preserves scoreboard)
- Two-player undo (single move rollback)
- Computer mode undo (two-move atomic rollback)
- Undo guard on completed games (Option A)
- Undo guard when move history is empty
- Scoreboard increment and scoreboard reset
- Complete computer AI decision priorities (1: Win, 2: Block, 3: Center, 4: Corner, 5: Edge)

### Frontend Unit Tests (Karma / Jasmine)
Run frontend unit tests using headless Chrome:
```bash
cd frontend
npm test -- --watch=false --browsers=ChromeHeadless
```

---

## 8. AI Tools and Prompt Summary

### Workflow & Conversion of Requirements
1. **Requirements Analysis**: Extracted key functional requirements (game rules, turn mechanics, undo rules, AI priority, scoreboard persistence, and API contracts).
2. **Architecture Breakdown**:
   - Designed domain models (`GameState`, `MoveRecord`, `Scoreboard`, `CellCoordinate`).
   - Defined REST contract ensuring the backend remains the authoritative source of truth.
   - Designed frontend components and responsive UI layout.
3. **AI Generation & Manual Engineering**:
   - *AI-Assisted*: Initial scaffolding of Web API controllers, model structures, and xUnit test matrices.
   - *Manual Review & Customization*:
     - Refined computer AI priority logic to ensure edge cases (e.g., dual threats, corner traps) strictly follow the requested 5-tier precedence.
     - Built atomic two-move undo in computer mode to guarantee the board and history never desynchronize.
     - Implemented Web Audio API synthesis for auditory interaction without third-party audio files.
     - Adjusted Angular budget thresholds in `angular.json` to accommodate high-quality CSS visual styles.

---

## 9. Design Decisions

1. **Backend as Source of Truth**:
   The frontend performs optimistic UI rendering but immediately syncs and validates every move against the backend REST API response. This prevents client-side state drift or rule tampering.
2. **Scoreboard & Undo Alignment (Option A Selected)**:
   In accordance with the specification clarification, **Option A: Disable Undo After Completion** was implemented. Once a game reaches `Won` or `Draw`, Undo is locked. This ensures scoreboard statistics represent completed match outcomes and prevents ambiguous score rollback logic.
3. **Atomic Undo in Computer Mode**:
   In Play Against Computer mode, the user always plays X and the computer plays O automatically. Undoing a move rolls back both O's automated response and X's prior move simultaneously, returning the game directly to the player's turn.
4. **Embedded Scoreboard in GameStateResponse**:
   The `GameStateResponse` embeds the latest scoreboard snapshot alongside the board state. This eliminates redundant roundtrips while keeping dedicated `/api/scoreboard` endpoints available.
5. **Zero External Media Assets**:
   Sound effects are dynamically synthesized on the client using the browser's native `AudioContext`. This ensures fast load times and zero broken media links.

---

## 10. Clarifications and Assumptions

- **Human Player Identity**: In Computer Mode, the human player is always assigned Player X, and the computer is always Player O.
- **Session-Level Scope**: The scoreboard maintains session-level tracking across all games played within the application lifecycle.
- **Board Indexing**: API models use 0-indexed coordinates (`0, 1, 2`), while the user-facing Move History formats them as 1-indexed (`Row 1, Column 1`) for natural human reading.

---

## 11. Known Limitations

- **In-Memory Volatility**: The backend uses an in-memory thread-safe state store. Restarting the backend server resets active game sessions and scoreboard tallies.
- **Single Active Game per UI Session**: The frontend UI is designed to focus on one active game session at a time.

---

## 12. Future Improvements

- **Database Persistence**: Integrate SQLite or PostgreSQL via Entity Framework Core to persist games, user accounts, and historical match stats across server restarts.
- **Real-Time Multiplayer**: Introduce SignalR (WebSockets) for remote head-to-head multiplayer across different browser windows or devices.
- **Difficulty Settings**: Add selectable AI difficulty levels (e.g., Easy/Random, Medium/Heuristic, and Hard/Minimax algorithm).
- **Expandable Board Dimensions**: Support 4×4 or 5×5 grid variants with customizable win lengths (e.g., Connect 4 style).

