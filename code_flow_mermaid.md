# Sơ đồ luồng Mermaid

Dán bất kỳ khối `mermaid` nào bên dưới vào [Mermaid Live Editor](https://mermaid.live/) để hiển thị sơ đồ.

## 1. Bệnh viện: Lễ tân, mất điện, câu đố điện, mở khóa thang máy

```mermaid
flowchart TD
    A[ReceptionistQuest.Start] --> B{GameState flags}
    B -->|ReceptionistQuestComplete| C[Hoàn thành: NPC giai đoạn 2]
    B -->|ElectricityOut hoặc WelcomeComplete| D[Nhiệm vụ điện: NPC giai đoạn 1]
    B -->|ngược lại| E[Chào mừng: NPC giai đoạn 0]

    E --> F[OnFinishTalking]
    F --> G[CompleteReceptionistWelcome]
    G --> H[receptionistWelcomeComplete = true]

    H --> I[Người chơi nhấp vào Thang máy]
    I --> J{Đã hoàn thành phần chào mừng?}
    J -->|không| K[Không làm gì]
    J -->|có và điện đang bật| L[GameState.TriggerBlackout]
    L --> M[electricityOut = true\nelevatorUnlocked = false\nSaveGame]
    M --> N[Cường độ Global Light = 0.02]
    M --> O[MonsterAI nhận biết mất điện]
    M --> P[ElectricBoxInteractable khả dụng]

    P --> Q{Player in range + has tool_box\n+ MonsterAI.HasMonsterAppeared?}
    Q -->|không| R[Ghi lại điều kiện và ở lại bệnh viện]
    Q -->|có| S[Đặt cảnh hiện tại làm PreviousScene]
    S --> T[Load ElectricPuzzle]

    T --> U[ElectricBoardManager.CheckCircuit]
    U --> V{targetWire.isFilled\nand hasUserInteracted?}
    V -->|không| W[Tiếp tục xoay dây]
    V -->|có| X[RestorePowerAndUnlockElevator]
    X --> Y[electricityOut = false\nelevatorUnlocked = true\nSaveGame]
    Y --> Z[Hiện Người chơi và UI]
    Z --> AA[Tải PreviousScene hoặc Reception]
    Y --> AB[ReceptionistQuest.OnFinishTalking]
    AB --> AC[CompleteReceptionistQuest\nNPC stage 2]
```

## 2. Câu đố điện: xoay dây và kiểm tra mạch theo chiều rộng

```mermaid
flowchart TD
    A[ElectricBoardManager.Start] --> B[Ẩn Người chơi và UI]
    B --> C[Dọn dẹp EventSystem trùng lặp]
    C --> D[GeneratePuzzle]
    D --> E[Đọc puzzleMap 5 x 5]
    E --> F[Khởi tạo Wire cho từng ô khác 0]
    F --> G[Đặt sourceWire và targetWire bị khóa]
    G --> H[UpdateCircuitDelay]
    H --> I[CheckCircuit]

    J[Người chơi nhấp chuột trái vào Wire] --> K{isSolved?}
    K -->|có| L[Bỏ qua thao tác nhập]
    K -->|no| M[Wire.UpdateInput]
    M --> N{wire.isLocked?}
    N -->|có| O[Không xoay]
    N -->|không| P[Xoay transform -90 độ]
    P --> Q[hasUserInteracted = true]
    Q --> H

    I --> R[Đặt isFilled = false cho mọi gridWires]
    R --> S[Đưa sourceWire vào hàng đợi và đánh dấu đã thăm]
    S --> T{Hàng đợi còn Wire?}
    T -->|có| U[Lấy current khỏi hàng đợi\ncurrent.isFilled = true]
    U --> V[Wire.GetConnectedWires]
    V --> W[Dùng raycast connection-box\nvà kiểm tra khoảng cách hai chiều]
    W --> X[Đưa các dây kết nối chưa thăm vào hàng đợi]
    X --> T
    T -->|không| Y[Wire.UpdateColor cho mọi ô]
    Y --> Z{targetWire.isFilled\nand hasUserInteracted?}
    Z -->|không| AA[Chờ lần xoay tiếp theo]
    Z -->|có| AB[isSolved = true]
    AB --> AC[GameState.RestorePowerAndUnlockElevator]
    AC --> AD[Trở về PreviousScene / Reception]
```

## 3. MonsterAI: xuất hiện khi mất điện, tuần tra, truy đuổi, choáng, gây sát thương

```mermaid
flowchart TD
    A[MonsterAI.Start] --> B{onlySpawnDuringBlackout?}
    B -->|có| C[SetMonsterVisible false]
    B -->|không| D[InitAfterNavMesh rồi tuần tra]

    E[MonsterAI.Update mỗi khung hình] --> F[Tìm Player và FlashlightController khi cần]
    F --> G{GameState.ElectricityOut\nand not hasSpawned?}
    G -->|có| H[Tăng spawnTimer]
    H --> I{spawnTimer >= spawnDelay?}
    I -->|có| J[SpawnMonster]
    J --> K[hasSpawned = true\nHasMonsterAppeared = true\nSetMonsterVisible true]
    K --> D
    I -->|không| L[Chờ]
    G -->|điện đã khôi phục và đã xuất hiện| M[DespawnMonster]
    M --> N[hasSpawned = false\nHasMonsterAppeared = false\nSetMonsterVisible false]

    D --> O{Agent đang hoạt động, ở trên NavMesh,\nvà đã tìm thấy người chơi?}
    O -->|không| E
    O -->|có| P{isStunned?}
    P -->|có| Q[Giảm stunTimer\ndừng agent đến khi hết thời gian]
    P -->|không| R[CheckPlayerDetection]
    R --> S{distance <= detectionRadius?}
    S -->|có| T[isChasing = true\ndừng coroutine mất hứng thú]
    S -->|không và khoảng cách > loseInterestDistance| U[Khởi động LoseInterestTimer]
    U --> V[Sau loseInterestTime:\nisChasing = false, GoToNextPatrol]
    T --> W[Đặt chaseSpeed và đích đến = player]
    V --> X[Điểm tuần tra và WaitThenPatrol]
    W --> Y[UpdateAnimation]
    X --> Y

    Z[Trigger của Flashlight đi vào] --> AA[TriggerStun]
    AA --> AB[isStunned = true\nstunTimer = stunDuration\nstop agent]
    AC[Trigger của Player đi vào/rời đi] --> AD[playerInDamageZone = true/false]
    AD --> AE{Ở vùng sát thương và không bị choáng\ntrong damageInterval?}
    AE -->|có| AF[PlayerHealth.TakeDamage damageAmount]
```

## 4. Chương 2: Memory Shard và NightmarePuzzle

```mermaid
flowchart TD
    A[Electric puzzle solved] --> B[GameState.elevatorUnlocked = true]
    B --> C[MemoryShard.Start / Update]
    C --> D{ElevatorUnlocked\nvà người chơi chưa có shard?}
    D -->|không| E[Ẩn Memory Shard]
    D -->|có| F[Kích hoạt Memory Shard]
    F --> G{Người chơi trong interactRange\nvà nhấn F?}
    G -->|không| H[Hiện hoặc ẩn hintUI theo khoảng cách]
    G -->|có| I[PickupShard]
    I --> J[InventoryManager.AddItem memoryShardData]
    J --> K[GameState.AddItem itemID]
    K --> L[Hide hintUI and shard]
    L --> M[Load CutsceneChap2]

    N[NightmarePuzzle: PuzzleManager.Start] --> O{Câu đố đã lưu tồn tại và chưa hoàn thành?}
    O -->|có| P[RestoreFromSave]
    O -->|không| Q[StartGame]
    Q --> R[CreateJigsawPieces rồi phân tán]
    P --> S[Người chơi kéo một mảnh]
    R --> S
    S --> T[Khi nhả chuột: SnapAndDisableIfCorrect]
    T --> U{Khoảng cách đến đích < width / 2?}
    U -->|không| V[Giữ mảnh có thể di chuyển]
    U -->|có| W[Snap localPosition\ntắt collider của mảnh\npiecesCorrect++]
    W --> X{piecesCorrect == pieces.Count?}
    X -->|không| S
    X -->|có| Y[isPuzzleCompleted = true]
    Y --> Z[DeletePuzzleSave]
    Z --> AA[Chờ completionDelay]
    AA --> AB[SceneTransition.SceneChange]

    AC[Người chơi chọn GoBack] --> AD{isPuzzleCompleted?}
    AD -->|có| Z
    AD -->|không| AE[SavePuzzleState: vị trí các mảnh\npiecesCorrect và isCompleted]
    AE --> AF[GameState.SavePuzzleToJSON]
    AF --> AG[TransitionToScene NightmareScene]
```

## 5. Chương 3: nhiệm vụ Mr. Graves và căn phòng bị khóa

```mermaid
flowchart TD
    A[MrGravesQuest.Start] --> B[Get NPC and GameState]
    B --> C{MrGravesQuestComplete?}
    C -->|có| D[currentState = Completed\nNPC giai đoạn 3]
    C -->|không| E{Inventory có medicine01?}
    E -->|có| F[currentState = MedicineCollected\nNPC giai đoạn 2]
    E -->|không| G[currentState = Intro\nNPC giai đoạn 0]

    G --> H[Sự kiện hội thoại: OnFinishTalking]
    H --> I[currentState = NeedMedicine\nNPC giai đoạn 1]
    I --> J[Update kiểm tra InventoryManager.HasItem medicine01]
    J --> K{Đã lấy thuốc?}
    K -->|không| L[Giữ trạng thái NeedMedicine]
    K -->|có| M[currentState = MedicineCollected\nNPC giai đoạn 2]
    M --> N[Sự kiện hội thoại: OnFinishTalking]
    N --> O[ConsumeItem medicine01]
    O --> P[GameState.CompleteMrGravesQuest]
    P --> Q[mrGravesQuestComplete = true\nSaveGame]
    Q --> R[currentState = Completed\nNPC stage 3]

    S[Người chơi nhấp vào LockedDoor] --> T{requireMrGravesQuest?}
    T -->|có| U{MrGravesQuestComplete?}
    U -->|không| V[Hiện lockedHint và ở lại]
    U -->|có| W[UnlockDoor]
    T -->|không| W
    W --> Z[SceneManager.LoadScene nextScene]
```

## Biến trạng thái được tham chiếu trong sơ đồ

| Hệ thống | Biến / trường được lưu |
| --- | --- |
| Bệnh viện và câu đố điện | `receptionistWelcomeComplete`, `receptionistQuestComplete`, `electricityOut`, `elevatorUnlocked`, `PreviousScene`, `isSolved`, `_hasUserInteracted`, `sourceWire`, `targetWire` |
| MonsterAI | `hasSpawned`, `HasMonsterAppeared`, `spawnTimer`, `isChasing`, `isStunned`, `stunTimer`, `playerInDamageZone`, `damageTimer` |
| Mảnh ký ức | `memoryShardData.itemID`, `interactRange`, `cutsceneSceneName` |
| NightmarePuzzle | `pieces`, `draggingPiece`, `piecesCorrect`, `isPuzzleCompleted`, `PuzzleSaveData` |
| Mr. Graves và cánh cửa | `currentState`, `itemID`, `mrGravesQuestComplete`, `requireMrGravesQuest`, `nextScene` |
