using UnityEngine;
using UnityEngine.Playables;

public class CutsceneDialogueController : MonoBehaviour
{
     public PlayableDirector timelineDirector;
    // Tham chiếu đến script quản lý DialoguePanel của bạn
    // public DialogueManager dialogueManager; 
    // Hàm này sẽ được Signal Receiver gọi
    public void TriggerMiaDialogue() 
    {
        // 1. Tạm dừng Timeline để chờ người chơi đọc
        timelineDirector.Pause();
        // 2. Gọi logic hiện hội thoại của NPC
        // Ví dụ: dialogueManager.StartDialogue(miaDialogueData);
        // ... (bật DialoguePanel, set text cho DialogueText, v.v.)
    }
    // Bạn phải gọi hàm này khi người chơi bấm nút "Next" và ĐÃ HẾT CÂU THOẠI
    public void OnDialogueFinished()
    {
        // Tắt DialoguePanel
        // ... 
        // 3. Cho Timeline chạy tiếp
        timelineDirector.Play();
    }
}
