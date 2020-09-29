using UnityEngine;
public class movement : MonoBehaviour
{

    public float speed = 10.0f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector2 offset = Vector2.zero;
        if (Input.GetKey(KeyCode.A)) offset.x -= speed * Time.deltaTime;
        if (Input.GetKey(KeyCode.D)) offset.x += speed * Time.deltaTime;
        if (Input.GetKey(KeyCode.W)) offset.y += speed * Time.deltaTime;
        if (Input.GetKey(KeyCode.S)) offset.y -= speed * Time.deltaTime;

        Vector2 pos = new Vector2(transform.position.x, transform.position.y);
        pos += offset;
        transform.position = pos;
    }
}
