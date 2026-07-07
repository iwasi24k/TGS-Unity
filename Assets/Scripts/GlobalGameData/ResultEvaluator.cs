public enum Rank
{
    D,
    C,
    B,
    A,
    S
}

public static class ResultEvaluator
{
    /// <summary>
    /// ランクを計算する
    /// </summary>
    public static Rank Evaluate()
    {
        int star = 0;


        // ★5分以上残す
        if (GameData.Result.Time >= 300f)
        {
            star++;
        }

        // ★3連鎖以上
        if (GameData.Result.MaxCombo >= 3)
        {
            star++;
        }

        // ★5連鎖以上
        if (GameData.Result.MaxCombo >= 5)
        {
            star++;
        }

        // ★ゲームクリア
        if (GameData.Result.IsCleared)
        {
            star++;
        }
        else
        {
            star = 0;
        }

            return star switch
            {
                4 => Rank.S,
                3 => Rank.A,
                2 => Rank.B,
                1 => Rank.C,
                _ => Rank.D
            };
    }
}