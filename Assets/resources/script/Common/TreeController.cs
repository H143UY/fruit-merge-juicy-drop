using UnityEngine;

public class TreeController : MoveController
{
    private Vector3 tree;

    protected override void Move(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            tree = direction;
        }

        float smoothSpeed = 10f;
        transform.position = Vector3.Lerp(transform.position, transform.position + direction, Time.deltaTime * smoothSpeed);
    }
}
