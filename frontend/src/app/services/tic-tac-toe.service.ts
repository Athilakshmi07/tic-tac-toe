import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { GameStateResponse, CreateGameRequest, MakeMoveRequest, ScoreboardResponse } from '../models/game.models';

@Injectable({
  providedIn: 'root'
})
export class TicTacToeService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5000/api';

  createGame(mode: 'TwoPlayer' | 'Computer', difficulty: 'Unbeatable' | 'Standard' = 'Unbeatable'): Observable<GameStateResponse> {
    const payload: CreateGameRequest = { mode, difficulty };
    return this.http.post<GameStateResponse>(`${this.apiUrl}/games`, payload).pipe(
      catchError(this.handleError)
    );
  }

  getGame(id: string): Observable<GameStateResponse> {
    return this.http.get<GameStateResponse>(`${this.apiUrl}/games/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  makeMove(id: string, player: string, row: number, column: number): Observable<GameStateResponse> {
    const payload: MakeMoveRequest = { player, row, column };
    return this.http.post<GameStateResponse>(`${this.apiUrl}/games/${id}/moves`, payload).pipe(
      catchError(this.handleError)
    );
  }

  undoMove(id: string): Observable<GameStateResponse> {
    return this.http.post<GameStateResponse>(`${this.apiUrl}/games/${id}/undo`, {}).pipe(
      catchError(this.handleError)
    );
  }

  resetGame(id: string): Observable<GameStateResponse> {
    return this.http.post<GameStateResponse>(`${this.apiUrl}/games/${id}/reset`, {}).pipe(
      catchError(this.handleError)
    );
  }

  getScoreboard(): Observable<ScoreboardResponse> {
    return this.http.get<ScoreboardResponse>(`${this.apiUrl}/scoreboard`).pipe(
      catchError(this.handleError)
    );
  }

  resetScoreboard(): Observable<ScoreboardResponse> {
    return this.http.post<ScoreboardResponse>(`${this.apiUrl}/scoreboard/reset`, {}).pipe(
      catchError(this.handleError)
    );
  }

  private handleError(error: HttpErrorResponse) {
    let message = 'An unexpected error occurred.';
    if (error.error && typeof error.error === 'object' && error.error.error) {
      message = error.error.error;
    } else if (typeof error.error === 'string') {
      message = error.error;
    } else if (error.message) {
      message = error.message;
    }
    return throwError(() => new Error(message));
  }
}
