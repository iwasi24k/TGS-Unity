using UnityEngine;

public class SC_Goal : MonoBehaviour
{
    private Renderer goalRenderer;

    private Vector3 defaultScale;

    private bool isActive = false;

    private SC_Field field;

    //--------------------------------
    // 初期化
    //--------------------------------
    public void Setup(SC_Field f)
    {
        field = f;

        goalRenderer =
            GetComponent<Renderer>();

        defaultScale =
            transform.localScale;

        CloseGoal();
    }

    //--------------------------------
    // ゴール解放
    //--------------------------------
    public void ActivateGoal()
    {
        if (isActive) return;

        isActive = true;

        Debug.Log("ゴール解放！");

        if (goalRenderer != null)
        {
            goalRenderer.material.color =
                Color.yellow;
        }

        transform.localScale =
            defaultScale * 1.3f;
    }

    //--------------------------------
    // ゴール閉じる
    //--------------------------------
    public void CloseGoal()
    {
        isActive = false;

        transform.localScale =
            defaultScale;

        if (goalRenderer != null)
        {
            goalRenderer.material.color =
                Color.gray;
        }
    }

    //--------------------------------
    // 接触判定
    //--------------------------------
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("何か触れた : " + other.name);

        if (!other.CompareTag("Player"))
        {
            Debug.Log("Playerじゃない");
            return;
        }

        if (!isActive)
        {
            Debug.Log("まだゴール閉鎖中");
            return;
        }

        Debug.Log("次ステージへ");

        field.NextStage();
    }
}