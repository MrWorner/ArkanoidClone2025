using UnityEngine;

public class Brick : MonoBehaviour, IDamageable
{
    // --- НОВОЕ ---
    public static event System.Action OnAnyBrickDestroyed;
    // ---------------

    private BrickPool _pool;

    public void Init(BrickPool ownerPool)
    {
        _pool = ownerPool;
    }

    public void TakeDamage(int damageAmount)
    {
        // --- НОВОЕ ---
        OnAnyBrickDestroyed?.Invoke(); // Посылаем сигнал всем "слушателям"
        // ---------------

        if (_pool != null)
        {
            _pool.ReturnBrick(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}