import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TicTacToeService } from './services/tic-tac-toe.service';
import { SoundService } from './services/sound.service';
import { GameStateResponse, Scoreboard } from './models/game.models';

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
  isLoading = false;
  errorMessage: string | null = null;
  isApiConnected = true;

  ngOnInit() {
    this.startNewGame(this.selectedMode);
  }

  startNewGame(mode: 'TwoPlayer' | 'Computer' = this.selectedMode) {
    this.selectedMode = mode;
    this.isLoading = true;
    this.errorMessage = null;

    this.ticTacToeService.createGame(mode).subscribe({
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

  onCellClick(row: number, col: number) {
    if (!this.game || this.isLoading) return;
    if (this.game.status !== 'InProgress') return;
    if (this.game.board[row][col] !== '') return;

    // In computer mode, human is always X
    if (this.game.mode === 'Computer' && this.game.currentPlayer !== 'X') return;

    const previousStatus = this.game.status;
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
    // Option A: Undo is disabled after completion
    if (this.game.status !== 'InProgress') return false;
    return (this.game.moveHistory?.length ?? 0) > 0;
  }

  isWinningCell(row: number, col: number): boolean {
    if (!this.game || this.game.status !== 'Won' || !this.game.winningCells) return false;
    return this.game.winningCells.some(c => c.row === row && c.column === col);
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
