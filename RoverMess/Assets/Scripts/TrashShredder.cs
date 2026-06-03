using UnityEngine;

public class TrashShredder : MonoBehaviour
{
    [Header("Параметры Квоты")]
    public float targetQuota = 500f;   // Сколько нужно собрать за раунд
    public float currentCollected = 0f; // Сколько уже собрали
    public bool checkQuotaCollected = false;

    void OnTriggerEnter(Collider other)
    {
        // Проверяем, есть ли у вошедшего объекта скрипт мусора
        TrashItem trash = other.GetComponent<TrashItem>();

        if (trash != null && !trash.isDestroyed)
        {
            ShredTrash(trash);
        }
    }

    void ShredTrash(TrashItem trash)
    {
        trash.isDestroyed = true;

        // Добавляем вес к текущей квоте
        currentCollected += trash.weight;

        Debug.Log($"♻️ Уничтожено: {trash.itemName} ({trash.weight} кг). Квота: {currentCollected}/{targetQuota}");

        // Проверяем выполнение квоты
        if (currentCollected >= targetQuota && checkQuotaCollected == false)
        {
            Debug.LogWarning("🎉 КВОТА ВЫПОЛНЕНА! Вы заработали излишки!");
            checkQuotaCollected = true;
        }

        // Эффектное уничтожение (пока просто удаляем объект со сцены)
        // В будущем тут добавим запуск частиц (искр/дыма) и уменьшение масштаба коробки
        Destroy(trash.gameObject);
    }
}