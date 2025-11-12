using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    [Header("Configuración de sincronización")]
    [Tooltip("1 = sincroniza con la tasa de refresco del monitor")]
    public int vSyncCount = 1;

    [Header("FPS (solo si VSync está desactivado)")]
    [Tooltip("FPS máximo cuando VSync está desactivado")]
    public int targetFPS = 60;

    [Header("Mostrar contador de FPS")]
    public bool showFPS = true;
    private float deltaTime = 0.0f;

    private void Awake()
    {
        // 🔹 Activa VSync
        QualitySettings.vSyncCount = vSyncCount;

        // 🔹 Si el VSync está desactivado (vSyncCount = 0), fuerza manualmente los FPS
        if (vSyncCount == 0)
            Application.targetFrameRate = targetFPS;
        else
            Application.targetFrameRate = -1; // -1 significa “sin límite”
    }

    private void Update()
    {
        if (showFPS)
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        if (!showFPS) return;

        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();
        Rect rect = new Rect(10, 10, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 50;
        style.normal.textColor = Color.white;

        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.} FPS", fps);
        GUI.Label(rect, text, style);
    }
}
