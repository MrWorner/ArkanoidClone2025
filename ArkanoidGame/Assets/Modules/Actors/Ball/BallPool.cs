using UnityEngine;
using System.Collections.Generic;

public class BallPool : MonoBehaviour
{
    public static BallPool Instance { get; private set; }

    [SerializeField] private BallController ballPrefab;
    [SerializeField] private int initialPoolSize = 5;

    private List<BallController> _allBalls = new List<BallController>();

    void Awake()
    {
        Instance = this;
        // Создаем стартовый запас
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewBall(false);
        }
    }

    public BallController GetBall()
    {
        foreach (var ball in _allBalls)
        {
            if (!ball.gameObject.activeSelf)
            {
                ball.gameObject.SetActive(true);
                return ball;
            }
        }
        return CreateNewBall(true);
    }

    public void ReturnBall(BallController ball)
    {
        ball.gameObject.SetActive(false);
    }

    // Метод для сброса всех мячей (при старте уровня)
    public void ReturnAllBalls()
    {
        foreach (var ball in _allBalls)
        {
            ball.gameObject.SetActive(false);
        }
    }

    // Получить список всех АКТИВНЫХ мячей (для клонирования)
    public List<BallController> GetActiveBalls()
    {
        List<BallController> active = new List<BallController>();
        foreach (var ball in _allBalls)
        {
            if (ball.gameObject.activeSelf) active.Add(ball);
        }
        return active;
    }

    private BallController CreateNewBall(bool isActive)
    {
        BallController newBall = Instantiate(ballPrefab, transform);
        _allBalls.Add(newBall);
        newBall.gameObject.SetActive(isActive);
        return newBall;
    }
}