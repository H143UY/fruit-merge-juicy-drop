using UnityEngine;

public class PlayerController : TreeController
{
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            MovementTree();
        }
    }

    private void MovementTree()
    {
        float minX = -4.5f, maxX = 4.5f;
        float mouseX = Input.mousePosition.x;
        float worldX = (mouseX / Screen.width) * (maxX - minX) + minX;
        worldX = Mathf.Clamp(worldX, minX, maxX);
        Vector3 targetPos = new Vector3(worldX, transform.position.y, transform.position.z);
        Move(targetPos - transform.position);
    }
}
