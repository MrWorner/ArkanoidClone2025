using UnityEngine;
using NaughtyAttributes;

public class GameInstance : MonoBehaviour
{
    public static GameInstance Instance { get; private set; }

    [BoxGroup("Settings")]
    [Tooltip("Измените это число, чтобы полностью поменять генерацию всех уровней игры")]
    public int MasterSeed = 777; 

    [BoxGroup("Data"), ReadOnly]
    public int SelectedLevelIndex = 1;

    [BoxGroup("Data"), ReadOnly]
    public int CurrentLevelSeed = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetLevelData(int levelIndex)
    {
        SelectedLevelIndex = Mathf.Clamp(levelIndex, 1, 9999);

        // ФОРМУЛА ГЕНЕРАЦИИ:
        // Мы берем номер уровня, умножаем на произвольное число (для разброса)
        // и прибавляем ваш MasterSeed.
        // unchecked нужен, чтобы при переполнении числа игра не вылетала, а просто шла по кругу
        unchecked
        {
            CurrentLevelSeed = (SelectedLevelIndex * 1234567) + MasterSeed;
        }
    }
}