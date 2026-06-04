using System.Collections.Generic;
using UnityEngine;

public class CameraVisionOccluder : MonoBehaviour
{
    [Header("Настройки луча")]
    public Transform target;          // Ссылка на твоего Ровера
    public LayerMask wallLayer;       // Слой стен (выбери тут Obscuring)
    public float fadeAlpha = 0.3f;    // До какой степени прозрачности сжимать стену (0.3 = 30%)
    public float fadeSpeed = 5f;      // Скорость исчезновения/появления

    // Список стен, которые прозрачные прямо сейчас
    private List<Renderer> currentlyFadedWalls = new List<Renderer>();
    // Список стен, которые попали под луч в текущем кадре
    private List<Renderer> hitRenderers = new List<Renderer>();
    // Хранилище оригинальных цветов стен
    private Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();

    void FixedUpdate()
    {
        if (target == null) return;

        hitRenderers.Clear();

        // Считаем направление от камеры к роверу
        Vector3 direction = target.position - transform.position;
        float distance = direction.magnitude;

        // Пускаем ВСЕ лучи от камеры к игроку, чтобы поймать даже несколько стен подряд (RaycastAll)
        RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, distance, wallLayer);

        foreach (RaycastHit hit in hits)
        {
            Renderer wallRenderer = hit.collider.GetComponent<Renderer>();
            if (wallRenderer != null)
            {
                hitRenderers.Add(wallRenderer);

                // Если мы видим эту стену впервые, запоминаем ее родной цвет
                if (!originalColors.ContainsKey(wallRenderer))
                {
                    originalColors.Add(wallRenderer, wallRenderer.material.color);

                    // ВАЖНО: Переключаем режим материала кодом на полупрозрачный, если он был непрозрачным
                    // Для URP материалов это делается изменением режима рендеринга
                    SetupMaterialToFade(wallRenderer.material);
                }

                // Плавно уменьшаем Альфа-канал (прозрачность) стены
                Color targetColor = originalColors[wallRenderer];
                targetColor.a = fadeAlpha;
                wallRenderer.material.color = Color.Lerp(wallRenderer.material.color, targetColor, Time.fixedDeltaTime * fadeSpeed);
            }
        }

        // Возвращаем прежний цвет тем стенам, из-за которых ровер уже уехал
        for (int i = currentlyFadedWalls.Count - 1; i >= 0; i--)
        {
            Renderer wall = currentlyFadedWalls[i];

            if (!hitRenderers.Contains(wall))
            {
                // Плавно возвращаем оригинальный цвет
                wall.material.color = Color.Lerp(wall.material.color, originalColors[wall], Time.fixedDeltaTime * fadeSpeed);

                // Если цвет почти вернулся в норму, удаляем из списков отслеживания
                if (Mathf.Abs(wall.material.color.a - originalColors[wall].a) < 0.01f)
                {
                    wall.material.color = originalColors[wall];
                    currentlyFadedWalls.RemoveAt(i);
                }
            }
        }

        // Обновляем список активных прозрачных стен
        foreach (Renderer rend in hitRenderers)
        {
            if (!currentlyFadedWalls.Contains(rend))
            {
                currentlyFadedWalls.Add(rend);
            }
        }
    }

    // Технический метод, который настраивает материал URP для поддержки прозрачности кодом
    void SetupMaterialToFade(Material mat)
    {
        // Настройки для стандартного Universal Render Pipeline (URP) Lit шейдера
        mat.SetFloat("_Surface", 1); // 1 = Transparent surface
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0); // Выключаем запись в Z-буфер, чтобы не ломать отрисовку сквозь стену
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }
}