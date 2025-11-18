using UnityEngine;
using System.Collections.Generic;

public class PowerUp : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private float fallSpeed = 3f;
    [SerializeField] private int bonusPoints = 100;

    [Header("Анимация")]
    [SerializeField] private List<Sprite> animationFrames;
    [SerializeField] private float animationSpeed = 0.1f;

    [SerializeField]  public SpriteRenderer _sr;
    private int _currentFrame;
    private float _timer;

    /// <summary>
    /// Сбрасывает состояние при повторном использовании из пула
    /// </summary>
    public void ResetState()
    {
        _currentFrame = 0;
        _timer = 0;
        if (animationFrames.Count > 0)
        {
            _sr.sprite = animationFrames[0];
        }
    }

    void Update()
    {
        // 1. Падение
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        // 2. Анимация
        if (animationFrames.Count > 0)
        {
            _timer += Time.deltaTime;
            if (_timer >= animationSpeed)
            {
                _timer = 0;
                _currentFrame = (_currentFrame + 1) % animationFrames.Count;
                _sr.sprite = animationFrames[_currentFrame];
            }
        }

        // 3. Ушла за дно -> Возврат в пул
        if (transform.position.y < -10f)
        {
            PowerUpPool.Instance.ReturnPowerUp(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Paddle"))
        {
            ApplyBonus();
            // Вместо Destroy -> Возврат в пул
            PowerUpPool.Instance.ReturnPowerUp(this);
        }
    }

    private void ApplyBonus()
    {
        SoundManager.Instance.PlayOneShot(SoundType.PowerUpPickup);
        GameManager.Instance.AddScore(bonusPoints);
        GameManager.Instance.ActivateTripleBall();
    }
}