using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    [Header("Ссылки на логику игры")]
    public RoverEnergy playerEnergy;
    public TrashShredder warehouseShredder;

    [Header("Элементы UI")]
    public Slider energySlider;
    public TextMeshProUGUI quotaText;
    public Image sliderFillImage;
    public TextMeshProUGUI energyPercentageText; // Новый слот под текст процентов внутри батарейки

    [Header("Настройки цвета")]
    public Gradient energyGradient;

    void Start()
    {
        if (playerEnergy != null && energySlider != null)
        {
            energySlider.maxValue = playerEnergy.maxEnergy;
            energySlider.value = playerEnergy.currentEnergy;
        }
    }

    void Update()
    {
        UpdateEnergyUI();
        UpdateQuotaUI();
    }

    void UpdateEnergyUI()
    {
        if (playerEnergy != null && energySlider != null)
        {
            energySlider.value = playerEnergy.currentEnergy;

            // Вычисляем процент (от 0 до 100)
            float normalizedEnergy = playerEnergy.currentEnergy / playerEnergy.maxEnergy;
            int percentage = Mathf.RoundToInt(normalizedEnergy * 100f);

            // Обновляем текст процентов
            if (energyPercentageText != null)
            {
                energyPercentageText.text = $"{percentage}%";
            }

            // Меняем цвет полоски
            if (sliderFillImage != null && energyGradient != null)
            {
                sliderFillImage.color = energyGradient.Evaluate(normalizedEnergy);
            }
        }
    }

    void UpdateQuotaUI()
    {
        if (warehouseShredder != null && quotaText != null)
        {
            int collected = Mathf.RoundToInt(warehouseShredder.currentCollected);
            int target = Mathf.RoundToInt(warehouseShredder.targetQuota);

            quotaText.text = $"КВОТА: {collected} / {target} кг";

            if (warehouseShredder.checkQuotaCollected)
            {
                quotaText.color = Color.green;
            }
        }
    }
}