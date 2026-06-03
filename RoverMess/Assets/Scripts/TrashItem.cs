using UnityEngine;

public class TrashItem : MonoBehaviour
{
    [Header("Параметры мусора")]
    public string itemName = "Коробка";
    public float weight = 15f; // Вес в кг, который пойдет в квоту

    [HideInInspector]
    public bool isDestroyed = false; // Чтобы избежать двойного засчета
}