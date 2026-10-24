using System.Collections.Concurrent;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public interface IGameService
{
    GameState CreateGame(string? mode, string? difficulty = GameConstants.DifficultyUnbeatable);
    GameState? GetGame(Guid id);
    GameState MakeMove(Guid id, string player, int row, int column);
    GameState UndoMove(Guid id);
    GameState ResetGame(Guid id);
    Scoreboard GetScoreboard();
    Scoreboard ResetScoreboard();
}

public class GameService : IGameService
{
    private readonly IGameEngine _engine;
    private readonly ConcurrentDictionary<Guid, GameState> _games = new();
    private readonly object _scoreboardLock = new();
    private readonly Scoreboard _scoreboard = new();

    public GameService(IGameEngine engine)
    {
        _engine = engine;
    }

    public GameState CreateGame(string? mode, string? difficulty = GameConstants.DifficultyUnbeatable)
    {
        var gameMode = (mode?.Equals(GameConstants.ModeComputer, StringComparison.OrdinalIgnoreCase) == true)
            ? GameConstants.ModeComputer
            : GameConstants.ModeTwoPlayer;

        var gameDifficulty = (difficulty?.Equals(GameConstants.DifficultyStandard, StringComparison.OrdinalIgnoreCase) == true)
            ? GameConstants.DifficultyStandard
            : GameConstants.DifficultyUnbeatable;

        var game = new GameState
        {
            Id = Guid.NewGuid(),
            Mode = gameMode,
            Difficulty = gameDifficulty,
            CurrentPlayer = GameConstants.PlayerX,
            Status = GameConstants.StatusInProgress,
            Board = new string[3][]
            {
                new string[3] { "", "", "" },
                new string[3] { "", "", "" },
                new string[3] { "", "", "" }
            },
            MoveHistory = new List<MoveRecord>(),
            WinningCells = new List<CellCoordinate>(),
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };

        _games[game.Id] = game;
        return game;
    }

    public GameState? GetGame(Guid id)
    {
        _games.TryGetValue(id, out var game);
        return game;
    }

    public GameState MakeMove(Guid id, string player, int row, int column)
    {
        if (!_games.TryGetValue(id, out var game))
        {
            throw new KeyNotFoundException($"Game with ID {id} was not found.");
        }

        lock (game)
        {
            if (game.Status != GameConstants.StatusInProgress)
            {
                throw new InvalidOperationException("Cannot make a move in a completed game.");
            }

            if (row < 0 || row > 2 || column < 0 || column > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(row), "Move position is outside the 3x3 board.");
            }

            if (!string.IsNullOrEmpty(game.Board[row][column]))
            {
                throw new InvalidOperationException($"Cell at Row {row + 1}, Column {column + 1} is already occupied.");
            }

            var normalizedPlayer = player?.Trim().ToUpperInvariant();
            if (normalizedPlayer != GameConstants.PlayerX && normalizedPlayer != GameConstants.PlayerO)
            {
                throw new ArgumentException("Player must be 'X' or 'O'.", nameof(player));
            }

            if (normalizedPlayer != game.CurrentPlayer)
            {
                throw new InvalidOperationException($"It is Player {game.CurrentPlayer}'s turn, but Player {normalizedPlayer} attempted to move.");
            }

            if (game.Mode == GameConstants.ModeComputer && normalizedPlayer != GameConstants.PlayerX)
            {
                throw new InvalidOperationException("In Computer Mode, human player is always X.");
            }

            // Apply player's move
            game.Board[row][column] = normalizedPlayer;
            game.MoveHistory.Add(new MoveRecord
            {
                MoveNumber = game.MoveHistory.Count + 1,
                Player = normalizedPlayer,
                Row = row,
                Column = column,
                Position = $"Row {row + 1}, Column {column + 1}",
                Timestamp = DateTime.UtcNow
            });
            game.LastUpdatedAt = DateTime.UtcNow;

            // Check game status after move
            var (isWin, winner, winningCells) = _engine.CheckWin(game.Board);
            if (isWin)
            {
                game.Status = GameConstants.StatusWon;
                game.Winner = winner;
                game.WinningCells = winningCells;
                UpdateScoreboardOnCompletion(game);
                return game;
            }

            if (_engine.IsBoardFull(game.Board))
            {
                game.Status = GameConstants.StatusDraw;
                game.Winner = null;
                UpdateScoreboardOnCompletion(game);
                return game;
            }

            // Switch turn
            game.CurrentPlayer = (normalizedPlayer == GameConstants.PlayerX) ? GameConstants.PlayerO : GameConstants.PlayerX;

            // If Computer Mode, execute computer's move with selected AI difficulty
            if (game.Mode == GameConstants.ModeComputer && game.CurrentPlayer == GameConstants.PlayerO)
            {
                var compMove = _engine.GetComputerMove(game.Board, game.Difficulty);
                if (compMove != null)
                {
                    game.Board[compMove.Row][compMove.Column] = GameConstants.PlayerO;
                    game.MoveHistory.Add(new MoveRecord
                    {
                        MoveNumber = game.MoveHistory.Count + 1,
                        Player = GameConstants.PlayerO,
                        Row = compMove.Row,
                        Column = compMove.Column,
                        Position = $"Row {compMove.Row + 1}, Column {compMove.Column + 1}",
                        Timestamp = DateTime.UtcNow
                    });
                    game.LastUpdatedAt = DateTime.UtcNow;

                    var (cIsWin, cWinner, cWinningCells) = _engine.CheckWin(game.Board);
                    if (cIsWin)
                    {
                        game.Status = GameConstants.StatusWon;
                        game.Winner = cWinner;
                        game.WinningCells = cWinningCells;
                        UpdateScoreboardOnCompletion(game);
                        return game;
                    }

                    if (_engine.IsBoardFull(game.Board))
                    {
                        game.Status = GameConstants.StatusDraw;
                        game.Winner = null;
                        UpdateScoreboardOnCompletion(game);
                        return game;
                    }

                    game.CurrentPlayer = GameConstants.PlayerX;
                }
            }

            return game;
        }
    }

    public GameState UndoMove(Guid id)
    {
        if (!_games.TryGetValue(id, out var game))
        {
            throw new KeyNotFoundException($"Game with ID {id} was not found.");
        }

        lock (game)
        {
            // Option A: Disable Undo After Completion
            if (game.Status != GameConstants.StatusInProgress)
            {
                throw new InvalidOperationException("Undo is disabled for completed games.");
            }

            if (game.MoveHistory.Count == 0)
            {
                throw new InvalidOperationException("No moves available to undo.");
            }

            if (game.Mode == GameConstants.ModeTwoPlayer)
            {
                // Remove only the most recent move
                var lastMove = game.MoveHistory[^1];
                game.MoveHistory.RemoveAt(game.MoveHistory.Count - 1);
                game.Board[lastMove.Row][lastMove.Column] = GameConstants.EmptyCell;
                game.CurrentPlayer = lastMove.Player;
            }
            else // Computer Mode
            {
                // Remove both computer's move and human's previous move together
                if (game.MoveHistory.Count > 0 && game.MoveHistory[^1].Player == GameConstants.PlayerO)
                {
                    var compMove = game.MoveHistory[^1];
                    game.MoveHistory.RemoveAt(game.MoveHistory.Count - 1);
                    game.Board[compMove.Row][compMove.Column] = GameConstants.EmptyCell;
                }

                if (game.MoveHistory.Count > 0 && game.MoveHistory[^1].Player == GameConstants.PlayerX)
                {
                    var humanMove = game.MoveHistory[^1];
                    game.MoveHistory.RemoveAt(game.MoveHistory.Count - 1);
                    game.Board[humanMove.Row][humanMove.Column] = GameConstants.EmptyCell;
                }

                game.CurrentPlayer = GameConstants.PlayerX;
            }

            game.Status = GameConstants.StatusInProgress;
            game.Winner = null;
            game.WinningCells.Clear();
            game.LastUpdatedAt = DateTime.UtcNow;

            return game;
        }
    }

    public GameState ResetGame(Guid id)
    {
        if (!_games.TryGetValue(id, out var game))
        {
            throw new KeyNotFoundException($"Game with ID {id} was not found.");
        }

        lock (game)
        {
            game.Board = new string[3][]
            {
                new string[3] { "", "", "" },
                new string[3] { "", "", "" },
                new string[3] { "", "", "" }
            };
            game.CurrentPlayer = GameConstants.PlayerX;
            game.Status = GameConstants.StatusInProgress;
            game.Winner = null;
            game.WinningCells.Clear();
            game.MoveHistory.Clear();
            game.ScoreboardUpdated = false;
            game.LastUpdatedAt = DateTime.UtcNow;

            return game;
        }
    }

    public Scoreboard GetScoreboard()
    {
        lock (_scoreboardLock)
        {
            return new Scoreboard
            {
                XWins = _scoreboard.XWins,
                OWins = _scoreboard.OWins,
                Draws = _scoreboard.Draws
            };
        }
    }

    public Scoreboard ResetScoreboard()
    {
        lock (_scoreboardLock)
        {
            _scoreboard.XWins = 0;
            _scoreboard.OWins = 0;
            _scoreboard.Draws = 0;

            return new Scoreboard
            {
                XWins = 0,
                OWins = 0,
                Draws = 0
            };
        }
    }

    private void UpdateScoreboardOnCompletion(GameState game)
    {
        if (game.ScoreboardUpdated) return;

        lock (_scoreboardLock)
        {
            if (game.ScoreboardUpdated) return;

            if (game.Status == GameConstants.StatusWon)
            {
                if (game.Winner == GameConstants.PlayerX)
                {
                    _scoreboard.XWins++;
                }
                else if (game.Winner == GameConstants.PlayerO)
                {
                    _scoreboard.OWins++;
                }
            }
            else if (game.Status == GameConstants.StatusDraw)
            {
                _scoreboard.Draws++;
            }

            game.ScoreboardUpdated = true;
        }
    }
}
