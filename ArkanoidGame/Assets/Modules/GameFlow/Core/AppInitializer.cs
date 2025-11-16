using UnityEngine;

public class AppInitializer : MonoBehaviour
{
    void Awake()
    {
        // 1. Отключаем V-Sync (вертикальную синхронизацию)
        // Это нужно, чтобы наш targetFrameRate работал
        QualitySettings.vSyncCount = 0;

        // 2. Устанавливаем желаемый FPS
        Application.targetFrameRate = 90;

        // Важно: Не вызывать OnDemandRendering
        // (это новая настройка, которая может мешать)
        // OnDemandRendering.renderFrameInterval = 1; // Устаревшее
    }
}