using UnityEngine;

public class MultiPlayController : MonoBehaviour
{
    public int playerID = 1;
    public float speed = 5f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float moveX = 0f;

        if (playerID == 1)
        {
            // Player 1 は A/D キーで操作
            moveX = Input.GetAxis("Horizontal_P1"); // 後でInput Managerで設定
        }
        else if (playerID == 2)
        {
            // Player 2 は ←/→ キーで操作
            moveX = Input.GetAxis("Horizontal_P2"); // 後でInput Managerで設定
        }
        
        rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);
    }
}