public static class GameData
{
    public static ResultData Result { get; } = new();
}

public class ResultData
{
    public float Time { get; set; }
    public int MaxCombo { get; set; }
    public bool IsCleared { get; set; }

    public void Reset()
    {
        Time = 0;
        MaxCombo = 0;
        IsCleared = false;
    }
}