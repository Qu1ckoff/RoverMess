using UnityEngine;

public class RoverEnergy : MonoBehaviour
{
    [Header("Настройки Энергии")]
    public float maxEnergy = 100f;
    public float currentEnergy;

    [Header("Расход в секунду")]
    public float idleDrain = 0.5f;       // Когда просто стоим на складе
    public float normalDrain = 3f;       // При обычном движении
    public float nitroDrain = 12f;       // При зажатом Shift

    private bool isDead = false;

    void Start()
    {
        currentEnergy = maxEnergy;
    }

    void Update()
    {
        if (isDead) return;

        // Если энергия упала до нуля
        if (currentEnergy <= 0)
        {
            currentEnergy = 0;
            GameOver();
        }
    }

    // Метод для уменьшения энергии, вызываемый из физического движка
    public void ConsumeEnergy(float amount)
    {
        if (isDead) return;
        currentEnergy -= amount * Time.fixedDeltaTime;
    }

    // Проверка для контроллера: осталась ли батарейка?
    public bool HasEnergy()
    {
        return currentEnergy > 0 && !isDead;
    }

    private void GameOver()
    {
        isDead = true;
        Debug.LogWarning("❌ БАТАРЕЯ СЕЛА! [ТУТ БУДЕТ ЭКРАН СМЕРТИ / РЕЖИМ НАБЛЮДАТЕЛЯ]");

        // TODO: В будущем здесь будет вызов UI GameOverMenu.Show() 
        // или переключение сетевого режима на Spectator
    }
}