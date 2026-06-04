using System.Collections.Generic;
using UnityEngine;

public class WarehouseGenerator : MonoBehaviour
{
    [Header("Настройки формы склада")]
    public int maxRooms = 12;         // Максимальное количество комнат (чтобы оставались пустоты)
    public int gridRadius = 2;        // 2 означает сетку 5x5 (от -2 до 2). За края сетки комнаты не выйдут.
    public float roomSize = 30f;      // Размер комнаты
    [Range(0f, 1f)]
    public float spawnChance = 0.7f;  // Шанс (70%) что в соседней клетке появится комната
    public float openDoorChance = 0.5f; // Шанс, что соседние двери откроются между комнатами

    [Header("Префабы")]
    public GameObject centralShredderRoom;
    public List<GameObject> randomRoomPrefabs;

    // Словарь для хранения координат и комнат
    private Dictionary<Vector2Int, RoomNode> rooms = new Dictionary<Vector2Int, RoomNode>();
    private Queue<Vector2Int> queue = new Queue<Vector2Int>();

    // Направления: Север(0,1), Юг(0,-1), Восток(1,0), Запад(-1,0)
    private Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left };

    void Start()
    {
        Generate();
    }

    void Generate()
    {
        // 1. Спавним центр (Шредер)
        RoomNode centerRoom = SpawnRoom(centralShredderRoom, Vector2Int.zero);
        queue.Enqueue(Vector2Int.zero);

        // --- ДОБАВЛЕННЫЙ БЛОК: ГАРАНТИЯ ХОТЯ БЫ ОДНОГО ВЫХОДА ИЗ ЦЕНТРА ---
        // Выбираем случайное направление из 4 возможных
        Vector2Int forcedDir = directions[Random.Range(0, directions.Length)];
        GameObject firstPrefab = randomRoomPrefabs[Random.Range(0, randomRoomPrefabs.Count)];

        // Спавним там комнату в обход шансов
        RoomNode firstNeighbor = SpawnRoom(firstPrefab, forcedDir);

        // Жестко прорубаем проходы между центром и этой первой комнатой
        centerRoom.SetDoor(forcedDir, true);
        firstNeighbor.SetDoor(-forcedDir, true);

        // Добавляем её в очередь, чтобы лабиринт рос дальше от неё
        queue.Enqueue(forcedDir);

        // 2. Выращиваем лабиринт (Breadth-First Search)
        while (queue.Count > 0 && rooms.Count < maxRooms)
        {
            Vector2Int currentPos = queue.Dequeue();
            RoomNode currentRoom = rooms[currentPos];

            // Проверяем соседей вокруг текущей комнаты
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighborPos = currentPos + dir;

                // Если уже вышли за границы карты - пропускаем
                if (Mathf.Abs(neighborPos.x) > gridRadius || Mathf.Abs(neighborPos.y) > gridRadius)
                    continue;

                // Если в этой клетке комнаты еще нет
                if (!rooms.ContainsKey(neighborPos))
                {
                    // Бросаем кубик: спавнить ли тут комнату?
                    if (rooms.Count < maxRooms && Random.value < spawnChance)
                    {
                        // Выбираем случайную комнату
                        GameObject prefab = randomRoomPrefabs[Random.Range(0, randomRoomPrefabs.Count)];
                        RoomNode neighborRoom = SpawnRoom(prefab, neighborPos);

                        // Прорубаем проход между ними!
                        currentRoom.SetDoor(dir, true);         // Открываем стену у текущей к соседу
                        neighborRoom.SetDoor(-dir, true);       // Открываем стену у соседа (направление инвертируется)

                        // Добавляем соседа в очередь для дальнейшего роста
                        queue.Enqueue(neighborPos);
                    }
                }
                else
                {
                    // Если комната там УЖЕ ЕСТЬ, мы можем с шансом открыть между ними дополнительный проход (срез)
                    if (Random.value < openDoorChance) // шанс сделать кольцевой маршрут
                    {
                        currentRoom.SetDoor(dir, true);
                        rooms[neighborPos].SetDoor(-dir, true);
                    }
                }
            }
        }

        // 3. ФИНАЛЬНАЯ ПРОВЕРКА (Закрываем все проходы, которые ведут в пустоту)
        foreach (var kvp in rooms)
        {
            Vector2Int pos = kvp.Key;
            RoomNode room = kvp.Value;

            foreach (Vector2Int dir in directions)
            {
                // Если в направлении dir НЕТ комнаты, гарантированно закрываем стену!
                if (!rooms.ContainsKey(pos + dir))
                {
                    room.SetDoor(dir, false);
                }
            }
        }
    }

    RoomNode SpawnRoom(GameObject prefab, Vector2Int pos)
    {
        Vector3 worldPos = new Vector3(pos.x * roomSize, 0, pos.y * roomSize);
        GameObject obj = Instantiate(prefab, worldPos, Quaternion.identity, transform);
        RoomNode node = obj.GetComponent<RoomNode>();
        rooms.Add(pos, node);

        // --- БЛОК ДЛЯ СВЯЗИ С UI ---
        // Проверяем, есть ли в этой созданной комнате компонент Шредера
        TrashShredder shredder = obj.GetComponentInChildren<TrashShredder>();
        if (shredder != null)
        {
            // ИЗМЕНЕНО: Используем FindAnyObjectByType вместо устаревшего FindFirstObjectByType
            GameUIManager uiManager = Object.FindAnyObjectByType<GameUIManager>();
            if (uiManager != null)
            {
                // Передаем ссылку на шредер в UI
                uiManager.InitializeShredder(shredder);
            }
            else
            {
                Debug.LogError("🚨 Генератор не смог найти GameUIManager на сцене! Убедись, что скрипт висит на объекте UIManager.");
            }
        }
        // ---------------------------------------

        return node;
    }
}