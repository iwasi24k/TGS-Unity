using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ゲーム開始時に最初のメニューボタンを選択状態にするスクリプト。
/// </summary>
public class DefaultMenuSelection : MonoBehaviour
{
    // 最初に選択するボタン
    public GameObject firstButton;

    void Start()
    {
        EventSystem.current.SetSelectedGameObject(firstButton);
    }
}