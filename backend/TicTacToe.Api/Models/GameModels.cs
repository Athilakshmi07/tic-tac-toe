namespace TicTacToe.Api.Models;

public static class GameConstants
{
    public const string PlayerX = "X";
    public const string PlayerO = "O";
    public const string EmptyCell = "";

    public const string ModeTwoPlayer = "TwoPlayer";
    public const string ModeComputer = "Computer";

    public const string StatusInProgress = "InProgress";
    public const string StatusWon = "Won";
    public const string StatusDraw = "Draw";
}

public class CellCoordinate
{
    public int Row { get; set; }
    public int Column { get; set; }

    public CellCoordinate() { }

    public CellCoordinate(int row, int column)
    {
        Row = row;
        Column = column;
    }
}

public class MoveRecord
{
    public int MoveNumber { get; set; }
    public string Player { get; set; } = string.Empty;
    public int Row { get; set; }
    public int Column { get; set; }
    public string Position { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class Scoreboard
{
    public int XWins { get; set; }
    public int OWins { get; set; }
    public int Draws { get; set; }
    public int TotalGames => XWins + OWins + Draws;
}

public class GameState
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string[][] Board { get; set; } = new string[3][]
    {
        new string[3] { "", "", "" },
        new string[3] { "", "", "" },
        new string[3] { "", "", "" }
    };
    public string CurrentPlayer { get; set; } = GameConstants.PlayerX;
    public string Mode { get; set; } = GameConstants.ModeTwoPlayer;
    public string Status { get; set; } = GameConstants.StatusInProgress;
    public string? Winner { get; set; }
    public List<CellCoordinate> WinningCells { get; set; } = new();
    public List<MoveRecord> MoveHistory { get; set; } = new();
    public bool ScoreboardUpdated { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}
