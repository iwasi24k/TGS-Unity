using UnityEngine;

public class GameResultRecorder : MonoBehaviour
{
    [SerializeField]
    private GameTimer gameTimer;

    [SerializeField]
    private ComboManager comboManager;

    public void RecordResult()
    {
        GameData.Result.Time = gameTimer.CurrentTime;
        //GameData.Result.MaxCombo = comboManager.MaxCombo; MaxComboŽæ“¾
    }
}