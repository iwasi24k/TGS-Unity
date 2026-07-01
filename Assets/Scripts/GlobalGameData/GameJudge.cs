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

    private bool isGameEnd = false;

    public void Start()
    {
        isGameEnd = false;
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

    public bool GetGameEnd()
    {
        return isGameEnd;
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
        isGameEnd = true;

        GameData.Result.Time = gameTimer.CurrentTime;
        GameData.Result.MaxCombo = ComboManager.Instance.GetMaxComboCount();
        GameData.Result.IsCleared = isCleared;
    }
}