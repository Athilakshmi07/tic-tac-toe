using TicTacToe.Api.Models;

namespace TicTacToe.Api.DTOs;

public class CreateGameRequest
{
    public string Mode { get; set; } = GameConstants.ModeTwoPlayer;
    public string Difficulty { get; set; } = GameConstants.DifficultyUnbeatable;
}

public class MakeMoveRequest
{
    public string Player { get; set; } = string.Empty;
    public int Row { get; set; }
    public int Column { get; set; }
}

public class GameStateResponse
{
    public Guid Id { get; set; }
    public string[][] Board { get; set; } = Array.Empty<string[]>();
    public string CurrentPlayer { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Winner { get; set; }
    public List<CellCoordinate> WinningCells { get; set; } = new();
    public List<MoveRecord> MoveHistory { get; set; } = new();
    public Scoreboard Scoreboard { get; set; } = new();

    public static GameStateResponse FromState(GameState state, Scoreboard scoreboard)
    {
        return new GameStateResponse
        {
            Id = state.Id,
            Board = state.Board.Select(row => (string[])row.Clone()).ToArray(),
            CurrentPlayer = state.CurrentPlayer,
            Mode = state.Mode,
            Difficulty = state.Difficulty,
            Status = state.Status,
            Winner = state.Winner,
            WinningCells = new List<CellCoordinate>(state.WinningCells),
            MoveHistory = new List<MoveRecord>(state.MoveHistory),
            Scoreboard = new Scoreboard
            {
                XWins = scoreboard.XWins,
                OWins = scoreboard.OWins,
                Draws = scoreboard.Draws
            }
        };
    }
}

public class ScoreboardResponse
{
    public int XWins { get; set; }
    public int OWins { get; set; }
    public int Draws { get; set; }
    public int TotalGames => XWins + OWins + Draws;
}
