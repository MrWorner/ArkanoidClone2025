using UnityEngine;
using System.Collections.Generic;

public class PowerUpPool : MonoBehaviour
{
    public static PowerUpPool Instance { get; private set; }

    [Header("Настройки")]
    [SerializeField] private PowerUp powerUpPrefab;
    [SerializeField] private int initialPoolSize = 5;

    // Список всех созданных бонусов (и активных, и неактивных)
    private List<PowerUp> _allPowerUps = new List<PowerUp>();

    void Awake()
    {
        Instance = this;
        // Создаем стартовый запас
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewPowerUp(false);
        }
    }

    public PowerUp GetPowerUp(Vector3 position)
    {
        foreach (var p in _allPowerUps)
        {
            if (!p.gameObject.activeSelf)
            {
                p.transform.position = position;
                p.gameObject.SetActive(true);
                p.ResetState(); // Сброс анимации и таймеров
                return p;
            }
        }

        // Если свободных нет - создаем новый
        PowerUp newP = CreateNewPowerUp(true);
        newP.transform.position = position;
        return newP;
    }

    public void ReturnPowerUp(PowerUp p)
    {
        p.gameObject.SetActive(false);
    }

    /// <summary>
    /// Убирает все бонусы со сцены (вызывается при смене уровня)
    /// </summary>
    public void ReturnAllActive()
    {
        foreach (var p in _allPowerUps)
        {
            if (p.gameObject.activeSelf)
            {
                p.gameObject.SetActive(false);
            }
        }
    }

    private PowerUp CreateNewPowerUp(bool isActive)
    {
        PowerUp newObj = Instantiate(powerUpPrefab, transform);
        _allPowerUps.Add(newObj);
        newObj.gameObject.SetActive(isActive);
        return newObj;
    }
}