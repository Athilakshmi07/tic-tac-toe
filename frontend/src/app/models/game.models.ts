export interface CellCoordinate {
  row: number;
  column: number;
}

export interface MoveRecord {
  moveNumber: number;
  player: string;
  row: number;
  column: number;
  position: string;
  timestamp: string;
}

export interface Scoreboard {
  xWins: number;
  oWins: number;
  draws: number;
  totalGames?: number;
}

export interface GameStateResponse {
  id: string;
  board: string[][];
  currentPlayer: string;
  mode: 'TwoPlayer' | 'Computer';
  status: 'InProgress' | 'Won' | 'Draw';
  winner: string | null;
  winningCells: CellCoordinate[];
  moveHistory: MoveRecord[];
  scoreboard: Scoreboard;
}

export interface CreateGameRequest {
  mode: 'TwoPlayer' | 'Computer';
}

export interface MakeMoveRequest {
  player: string;
  row: number;
  column: number;
}

export interface ScoreboardResponse {
  xWins: number;
  oWins: number;
  draws: number;
  totalGames: number;
}
