import { TestBed } from '@angular/core/testing';
import { AppComponent } from './app.component';
import { provideHttpClient } from '@angular/common/http';
import { TicTacToeService } from './services/tic-tac-toe.service';
import { of } from 'rxjs';
import { GameStateResponse } from './models/game.models';

describe('AppComponent', () => {
  const mockGameState: GameStateResponse = {
    id: '11111111-1111-1111-1111-111111111111',
    board: [
      ['', '', ''],
      ['', '', ''],
      ['', '', '']
    ],
    currentPlayer: 'X',
    mode: 'TwoPlayer',
    status: 'InProgress',
    winner: null,
    winningCells: [],
    moveHistory: [],
    scoreboard: { xWins: 0, oWins: 0, draws: 0, totalGames: 0 }
  };

  let mockService: jasmine.SpyObj<TicTacToeService>;

  beforeEach(async () => {
    mockService = jasmine.createSpyObj('TicTacToeService', [
      'createGame',
      'getGame',
      'makeMove',
      'undoMove',
      'resetGame',
      'getScoreboard',
      'resetScoreboard'
    ]);

    mockService.createGame.and.returnValue(of(mockGameState));
    mockService.resetScoreboard.and.returnValue(of({ xWins: 0, oWins: 0, draws: 0, totalGames: 0 }));

    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [
        provideHttpClient(),
        { provide: TicTacToeService, useValue: mockService }
      ]
    }).compileComponents();
  });

  it('should create the app component', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should initialize with a two-player game', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    const app = fixture.componentInstance;
    expect(mockService.createGame).toHaveBeenCalledWith('TwoPlayer');
    expect(app.game).toEqual(mockGameState);
    expect(app.selectedMode).toBe('TwoPlayer');
  });

  it('should render the brand title', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.brand-title')?.textContent).toContain('TIC TAC TOE');
  });

  it('should render all 9 cells on the board', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const cells = compiled.querySelectorAll('.board-cell');
    expect(cells.length).toBe(9);
  });
});
