using UnityEngine;

public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance { get; private set; }

    [SerializeField] private float comboResetTime = 3.0f;

    [SerializeField] private int comboCount = 0;
    [SerializeField] private float timer = 0.0f;

    [SerializeField] private float noBlownAwayResetDelay = 0.2f;
    private float noBlownAwayTimer = 0.0f;

    public int ComboCount => comboCount;
    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        CheckAllEnemiesNotBlownAway();

        if (comboCount > 0)
        {
            timer -= Time.deltaTime;

            if (timer <= 0.0f)
            {
                ResetCombo();
            }
        }
    }

    public void AddCombo(int addCount = 1)
    {
        comboCount += addCount;
        timer = comboResetTime;

        Debug.Log("Combo : " + comboCount);
    }

    public int GetComboCount()
    {
        return comboCount;
    }

    public void ResetCombo()
    {
        comboCount = 0;
        timer = 0.0f;

        Debug.Log("Combo Reset");
    }

    private void CheckAllEnemiesNotBlownAway()
    {
        if (comboCount <= 0) return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        bool anyBlownAway = false;

        foreach (GameObject enemy in enemies)
        {
            SC_EnemyStatusManager status = enemy.GetComponent<SC_EnemyStatusManager>();

            if (status != null && status.IsBlownAway())
            {
                anyBlownAway = true;
                break;
            }
        }

        if (anyBlownAway)
        {
            noBlownAwayTimer = 0.0f;
        }
        else
        {
            noBlownAwayTimer += Time.deltaTime;

            if (noBlownAwayTimer >= noBlownAwayResetDelay)
            {
                ResetCombo();
            }
        }
    }
}
