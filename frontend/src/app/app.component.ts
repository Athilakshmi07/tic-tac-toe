import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TicTacToeService } from './services/tic-tac-toe.service';
import { SoundService } from './services/sound.service';
import { GameStateResponse } from './models/game.models';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  private ticTacToeService = inject(TicTacToeService);
  public soundService = inject(SoundService);

  game: GameStateResponse | null = null;
  selectedMode: 'TwoPlayer' | 'Computer' = 'TwoPlayer';
  selectedDifficulty: 'Unbeatable' | 'Standard' = 'Unbeatable';
  isLoading = false;
  errorMessage: string | null = null;
  isApiConnected = true;

  // 24 procedural celebration confetti pieces
  confettiPieces = Array.from({ length: 24 }, (_, i) => ({
    id: i,
    left: Math.random() * 100,
    delay: Math.random() * 0.8,
    duration: 1.8 + Math.random() * 1.5,
    size: 6 + Math.random() * 8,
    color: ['#00f0ff', '#ff007f', '#ffbe0b', '#10b981', '#a800ff'][i % 5],
    rotation: Math.random() * 360
  }));

  ngOnInit() {
    this.startNewGame(this.selectedMode, this.selectedDifficulty);
  }

  startNewGame(
    mode: 'TwoPlayer' | 'Computer' = this.selectedMode,
    difficulty: 'Unbeatable' | 'Standard' = this.selectedDifficulty
  ) {
    this.selectedMode = mode;
    this.selectedDifficulty = difficulty;
    this.isLoading = true;
    this.errorMessage = null;

    this.ticTacToeService.createGame(mode, difficulty).subscribe({
      next: (state) => {
        this.game = state;
        this.isLoading = false;
        this.isApiConnected = true;
        this.soundService.playReset();
      },
      error: (err) => {
        this.handleError(err);
        this.isLoading = false;
        this.isApiConnected = false;
      }
    });
  }

  switchDifficulty(difficulty: 'Unbeatable' | 'Standard') {
    if (this.selectedDifficulty === difficulty) return;
    this.startNewGame(this.selectedMode, difficulty);
  }

  onCellClick(row: number, col: number) {
    if (!this.game || this.isLoading) return;
    if (this.game.status !== 'InProgress') return;
    if (this.game.board[row][col] !== '') return;

    if (this.game.mode === 'Computer' && this.game.currentPlayer !== 'X') return;

    const playerMakingMove = this.game.currentPlayer;
    this.isLoading = true;
    this.errorMessage = null;

    this.ticTacToeService.makeMove(this.game.id, playerMakingMove, row, col).subscribe({
      next: (state) => {
        this.game = state;
        this.isLoading = false;
        this.isApiConnected = true;

        if (state.status === 'Won') {
          this.soundService.playWin();
        } else if (state.status === 'Draw') {
          this.soundService.playDraw();
        } else {
          this.soundService.playMove(playerMakingMove === 'X');
        }
      },
      error: (err) => {
        this.handleError(err);
        this.isLoading = false;
      }
    });
  }

  undo() {
    if (!this.game || !this.canUndo() || this.isLoading) return;

    this.isLoading = true;
    this.errorMessage = null;

    this.ticTacToeService.undoMove(this.game.id).subscribe({
      next: (state) => {
        this.game = state;
        this.isLoading = false;
        this.isApiConnected = true;
        this.soundService.playUndo();
      },
      error: (err) => {
        this.handleError(err);
        this.isLoading = false;
      }
    });
  }

  resetCurrentGame() {
    if (!this.game || this.isLoading) return;

    this.isLoading = true;
    this.errorMessage = null;

    this.ticTacToeService.resetGame(this.game.id).subscribe({
      next: (state) => {
        this.game = state;
        this.isLoading = false;
        this.isApiConnected = true;
        this.soundService.playReset();
      },
      error: (err) => {
        this.handleError(err);
        this.isLoading = false;
      }
    });
  }

  resetScoreboard() {
    if (this.isLoading) return;

    this.isLoading = true;
    this.errorMessage = null;

    this.ticTacToeService.resetScoreboard().subscribe({
      next: (scoreboardRes) => {
        if (this.game) {
          this.game.scoreboard = scoreboardRes;
        }
        this.isLoading = false;
        this.isApiConnected = true;
        this.soundService.playReset();
      },
      error: (err) => {
        this.handleError(err);
        this.isLoading = false;
      }
    });
  }

  canUndo(): boolean {
    if (!this.game) return false;
    if (this.game.status !== 'InProgress') return false;
    return (this.game.moveHistory?.length ?? 0) > 0;
  }

  isWinningCell(row: number, col: number): boolean {
    if (!this.game || this.game.status !== 'Won' || !this.game.winningCells) return false;
    return this.game.winningCells.some(c => c.row === row && c.column === col);
  }

  isLatestMove(moveNumber: number): boolean {
    if (!this.game?.moveHistory || this.game.moveHistory.length === 0) return false;
    return this.game.moveHistory[this.game.moveHistory.length - 1].moveNumber === moveNumber;
  }

  toggleSound() {
    this.soundService.toggleMute();
  }

  private handleError(err: any) {
    this.errorMessage = err?.message || 'A network error occurred. Ensure .NET backend is running.';
    setTimeout(() => {
      this.errorMessage = null;
    }, 4500);
  }
}
