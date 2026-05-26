using UnityEngine;

public class Rock : MonoBehaviour
{
    [SerializeField] private int hp = 3;
    [SerializeField] private float blowPower = 10f;
    [SerializeField] private float scaleDown = 0.2f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // çUåÇÇ™ìñÇΩÇ¡ÇΩéûÇ…åƒÇ‘
    public void Damage(Vector3 attackPos)
    {
        hp--;

        //========================
        // êÅÇ¡îÚÇ—
        //========================

        Vector3 dir =
            (transform.position - attackPos).normalized;

        dir.y = 0.3f;

        rb.AddForce(dir * blowPower,
                    ForceMode.Impulse);

        //========================
        // è¨Ç≥Ç≠Ç∑ÇÈ
        //========================

        transform.localScale *= (1.0f - scaleDown);

        //========================
        // HP0Ç≈è¡Ç¶ÇÈ
        //========================

        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}