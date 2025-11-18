using UnityEngine;

// Этот скрипт все еще "прибивает" себя к низу,
// но теперь он также работает как ТРИГГЕР
public class AnchorToBottom : MonoBehaviour
{
    private Camera _mainCamera;

    void Start()
    {
        ApplyPosition();
    }

    private void ApplyPosition()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null) return;

        Vector3 bottomEdgePos = _mainCamera.ViewportToWorldPoint(
            new Vector3(0.5f, 0, _mainCamera.nearClipPlane)
        );

        transform.position = new Vector3(
            bottomEdgePos.x,
            bottomEdgePos.y,
            transform.position.z
        );

        float screenWidth = _mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x -
                            _mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;

        // (Код для растягивания спрайта/коллайдера)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) { sr.size = new Vector2(screenWidth, sr.size.y); }
        else
        {
            BoxCollider2D bc = GetComponent<BoxCollider2D>();
            if (bc != null) { bc.size = new Vector2(screenWidth, bc.size.y); }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ball"))
        {
            BallController ball = other.GetComponent<BallController>();
            if (ball != null)
            {
                SoundManager.Instance.PlayOneShot(SoundType.BallLost);
                GameManager.Instance.HandleBallLost(ball);
            }
        }
    }
}