using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;

/// <summary>
/// Tool tự động setup Animator Controller cho top-down 4-direction movement
/// Menu: Tools > Setup Player Animator
/// </summary>
public class AnimatorSetupHelper : EditorWindow
{
    private AnimatorController controller;
    private string clipFolder = "Assets/Game/Animations";
    private bool showClipStatus = false;
    
    // Danh sách clips cần có (name → expected file name)
    private readonly (string stateName, string clipName, float x, float y)[] walkMotions =
    {
        ("WalkDown",  "WalkDown",  0f,  -1f),
        ("WalkUp",    "WalkUp",    0f,   1f),
        ("WalkLeft",  "WalkLeft", -1f,   0f),
        ("WalkRight", "WalkRight", 1f,   0f),
    };
    
    private readonly (string stateName, string clipName, float x, float y)[] idleMotions =
    {
        ("Idle",      "Idle",       0f,  -1f),
        ("IdleUp",    "IdleUp",     0f,   1f),
        ("IdleLeft",  "IdleLeft",  -1f,   0f),
        ("IdleRight", "IdleRight",  1f,   0f),
    };

    [MenuItem("Tools/Setup Player Animator")]
    static void Init()
    {
        var window = GetWindow<AnimatorSetupHelper>("Player Animator Setup");
        window.minSize = new Vector2(400, 500);
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Space(8);
        GUILayout.Label("Player Animator Controller Setup", EditorStyles.boldLabel);
        GUILayout.Label("Top-down 4-direction movement", EditorStyles.miniLabel);
        GUILayout.Space(8);

        controller = EditorGUILayout.ObjectField(
            "Animator Controller", controller,
            typeof(AnimatorController), false) as AnimatorController;
        
        clipFolder = EditorGUILayout.TextField("Clip Folder", clipFolder);

        GUILayout.Space(8);
        
        // Hiển thị trạng thái clips
        showClipStatus = EditorGUILayout.Foldout(showClipStatus, "Kiểm tra Animation Clips");
        if (showClipStatus)
        {
            EditorGUI.indentLevel++;
            GUILayout.Label("Walk animations:", EditorStyles.boldLabel);
            foreach (var m in walkMotions)
                DrawClipStatus(m.clipName);
            GUILayout.Space(4);
            GUILayout.Label("Idle animations:", EditorStyles.boldLabel);
            foreach (var m in idleMotions)
                DrawClipStatus(m.clipName);
            EditorGUI.indentLevel--;
        }

        GUILayout.Space(12);

        EditorGUILayout.HelpBox(
            "Cấu trúc cần có trong Clip Folder:\n" +
            "Walk:  WalkDown, WalkUp, WalkLeft, WalkRight\n" +
            "Idle:  Idle, IdleUp, IdleLeft, IdleRight\n\n" +
            "Bước 1: Tạo clips trong Animation window\n" +
            "Bước 2: Kéo Mia.controller vào ô trên\n" +
            "Bước 3: Click Setup",
            MessageType.Info);

        GUILayout.Space(8);
        
        GUI.enabled = controller != null;
        if (GUILayout.Button("SETUP ANIMATOR CONTROLLER", GUILayout.Height(36)))
            SetupAnimatorController();
        GUI.enabled = true;
        
        if (GUILayout.Button("Xóa toàn bộ states và setup lại", GUILayout.Height(28)))
        {
            if (controller != null &&
                EditorUtility.DisplayDialog("Xác nhận",
                    "Xóa toàn bộ states trong controller và tạo lại?", "Xóa & Setup", "Hủy"))
            {
                ClearAndSetup();
            }
        }
    }

    void DrawClipStatus(string clipName)
    {
        var clip = FindClip(clipName);
        var color = GUI.color;
        GUI.color = clip != null ? Color.green : Color.red;
        GUILayout.Label(clip != null ? $"  ✓ {clipName}" : $"  ✗ {clipName} (missing!)");
        GUI.color = color;
    }

    // ─── Setup chính ────────────────────────────────────────────────────────────

    void ClearAndSetup()
    {
        var sm = controller.layers[0].stateMachine;
        
        // Xóa tất cả states cũ
        foreach (var s in sm.states)
            sm.RemoveState(s.state);
        
        // Xóa tất cả parameters cũ
        while (controller.parameters.Length > 0)
            controller.RemoveParameter(0);
        
        SetupAnimatorController();
    }

    void SetupAnimatorController()
    {
        // ── Parameters ──────────────────────────────────────────────────────────
        EnsureParameter("MoveX",  AnimatorControllerParameterType.Float, 0f);
        EnsureParameter("MoveY",  AnimatorControllerParameterType.Float, -1f);
        EnsureParameter("speed",  AnimatorControllerParameterType.Float, 0f);
        Debug.Log("[AnimatorSetup] ✓ Parameters: MoveX, MoveY, speed");

        var sm = controller.layers[0].stateMachine;

        // ── Idle Blend Tree state ────────────────────────────────────────────────
        AnimatorState idleState = FindOrCreateState(sm, "Idle", new Vector3(250, 0));
        BlendTree idleTree = CreateBlendTree("IdleTree", idleMotions);
        idleState.motion = idleTree;
        sm.defaultState = idleState;

        // ── Walk Blend Tree state ────────────────────────────────────────────────
        AnimatorState walkState = FindOrCreateState(sm, "Walk", new Vector3(550, 0));
        BlendTree walkTree = CreateBlendTree("WalkTree", walkMotions);
        walkState.motion = walkTree;

        // ── Transitions ──────────────────────────────────────────────────────────
        // Idle → Walk khi speed > 0.01
        MakeTransition(idleState, walkState, "speed", AnimatorConditionMode.Greater, 0.01f);
        // Walk → Idle khi speed < 0.01
        MakeTransition(walkState, idleState, "speed", AnimatorConditionMode.Less, 0.01f);

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log("[AnimatorSetup] ✓ Hoàn tất! Kiểm tra Animator window.");
    }

    // ─── Blend Tree factory ─────────────────────────────────────────────────────

    BlendTree CreateBlendTree(string treeName,
        (string stateName, string clipName, float x, float y)[] motions)
    {
        BlendTree tree;
        controller.CreateBlendTreeInController(treeName, out tree);
        tree.blendType           = BlendTreeType.SimpleDirectional2D;
        tree.blendParameter      = "MoveX";
        tree.blendParameterY     = "MoveY";
        tree.useAutomaticThresholds = false;

        foreach (var m in motions)
        {
            var clip = FindClip(m.clipName);
            if (clip != null)
            {
                tree.AddChild(clip, new Vector2(m.x, m.y));
                Debug.Log($"[AnimatorSetup]   + {m.clipName} ({m.x}, {m.y})");
            }
            else
            {
                Debug.LogWarning($"[AnimatorSetup]   ⚠ Clip '{m.clipName}' không tìm thấy!");
            }
        }
        return tree;
    }

    // ─── Helpers ────────────────────────────────────────────────────────────────

    AnimationClip FindClip(string clipName)
    {
        // Tìm trong clipFolder trước, rồi toàn project
        string[] guids = AssetDatabase.FindAssets($"{clipName} t:AnimationClip", new[] { clipFolder });
        if (guids.Length == 0)
            guids = AssetDatabase.FindAssets($"{clipName} t:AnimationClip");
        if (guids.Length == 0) return null;
        return AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }

    void EnsureParameter(string name, AnimatorControllerParameterType type, float defaultVal)
    {
        foreach (var p in controller.parameters)
            if (p.name == name) return;
        controller.AddParameter(name, type);
        // Set default value trực tiếp vào serialized data
        var so = new SerializedObject(controller);
        var paramsArr = so.FindProperty("m_AnimatorParameters");
        for (int i = 0; i < paramsArr.arraySize; i++)
        {
            var el = paramsArr.GetArrayElementAtIndex(i);
            if (el.FindPropertyRelative("m_Name").stringValue == name)
            {
                el.FindPropertyRelative("m_DefaultFloat").floatValue = defaultVal;
                break;
            }
        }
        so.ApplyModifiedProperties();
    }

    AnimatorState FindOrCreateState(AnimatorStateMachine sm, string name, Vector3 pos)
    {
        foreach (var cs in sm.states)
            if (cs.state.name == name) return cs.state;
        return sm.AddState(name, pos);
    }

    void MakeTransition(AnimatorState from, AnimatorState to,
        string param, AnimatorConditionMode mode, float threshold)
    {
        var t = from.AddTransition(to);
        t.AddCondition(mode, threshold, param);
        t.hasExitTime = false;
        t.duration    = 0.05f;
    }
}
