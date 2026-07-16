using UnityEngine;

public class SC_ResultEffect : MonoBehaviour
{
    [SerializeField] private Transform BossTrans;
    [SerializeField] private Animator BossAnim;
    [SerializeField] private Animator PlayerAnim;
    [SerializeField] private GameObject ResUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(BossTrans == null)
        {
            Debug.LogError("BossTrans is not assigned in SC_ResultEffect.");
        }
        if(BossAnim == null)
        {
            Debug.LogError("BossAnim is not assigned in SC_ResultEffect.");
        }
        if(PlayerAnim == null)
        {
            Debug.LogError("PlayerAnim is not assigned in SC_ResultEffect.");
        }
        if(ResUI == null)
        {
            Debug.LogError("ResUI is not assigned in SC_ResultEffect.");
        }

        ResUI.SetActive(false);

        SC_EffectManager.Instance.PlayEffect("Explosion", BossTrans.position, Quaternion.identity);
        BossAnim.SetBool("bShieldBreak", true);
        PlayerAnim.SetBool("bWinPose", true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void OnEndAnimation()
    {
        ResUI.SetActive(true);
    }
}
