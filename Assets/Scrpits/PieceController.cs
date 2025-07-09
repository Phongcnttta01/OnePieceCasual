using UnityEngine;

public class PieceController : MonoBehaviour
{
    private Vector3 targetPos;
    private bool isMoving = false;
    private float speed = 10f;

    public void MoveTo(Vector3 pos)
    {
        targetPos = pos;
        isMoving = true;
    }

    private void Update()
    {
        if (isMoving)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * speed);

            if (Vector3.Distance(transform.localPosition, targetPos) < 0.01f)
            {
                transform.localPosition = targetPos;
                isMoving = false;
            }
        }
    }

    public bool IsMoving()
    {
        return isMoving;
    }
}