using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    float moveSpeed = 5f;

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        var move = transform.right * moveX + transform.forward * moveZ;
        transform.Translate(moveSpeed * Time.deltaTime * move, Space.World);
    }
}
