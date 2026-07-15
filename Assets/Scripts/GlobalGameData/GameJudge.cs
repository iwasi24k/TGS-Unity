using UnityEngine;
using UnityEngine.SceneManagement;

public class GameJudge : MonoBehaviour
{
    [SerializeField]
    private GameTimer gameTimer;

    [SerializeField]
    private ComboManager comboManager;

    [SerializeField]
    private GameObject player = null;

    public void Start()
    {
        
    }

    public void Update()
    {
        if (!player) return;

        // HPが0になったらゲームオーバー
        if (player.GetComponent<SC_PlayerHP>().GetCurrentHP() <= 0)
        {
            GameOver();
        }

        // ボスを倒したらゲームクリア
        if (SC_Field.Instance != null && SC_Field.Instance.IsBossDefeated())
        {
            GameClear();
        }

    }

    public void GameClear()
    {
        SaveResult(true);
    }

    public void GameOver()
    {
        SaveResult(false);
    }

    private void SaveResult(bool isCleared)
    {
        GameData.Result.IsGameEnd = true;

        GameData.Result.Time = gameTimer.CurrentTime;
        GameData.Result.MaxCombo = ComboManager.Instance.GetMaxComboCount();
        GameData.Result.IsCleared = isCleared;

        Debug.Log($"Game End! Cleared: {isCleared}, Time: {GameData.Result.Time}, Max Combo: {GameData.Result.MaxCombo}");
    }
}