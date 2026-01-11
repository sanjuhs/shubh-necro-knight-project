using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

/// <summary>
/// Editor tool to automatically generate the Warrior Animator Controller
/// with all states, transitions, and parameters properly configured.
/// </summary>
public class WarriorAnimatorGenerator : EditorWindow
{
    // Paths
    private const string ANIMATOR_OUTPUT_PATH = "Assets/Animations/WarriorAnimator.controller";
    private const string ANIMATIONS_FOLDER = "Assets/Animations";
    private const string WARRIOR_SPRITES_PATH = "Assets/Tiny Swords (Free Pack)/Units/Blue Units/Warrior";
    
    // Animation clip references (will be found or created)
    private AnimationClip idleClip;
    private AnimationClip runClip;
    private AnimationClip attack1Clip;
    private AnimationClip attack2Clip;
    private AnimationClip guardClip;
    
    // Settings
    private float animationFrameRate = 12f;
    private bool createAnimationClips = true;
    
    [MenuItem("Tools/Warrior/Generate Animator Controller")]
    public static void ShowWindow()
    {
        GetWindow<WarriorAnimatorGenerator>("Warrior Animator Generator");
    }
    
    [MenuItem("Tools/Warrior/Quick Generate (Auto)")]
    public static void QuickGenerate()
    {
        var generator = CreateInstance<WarriorAnimatorGenerator>();
        generator.GenerateAll();
        DestroyImmediate(generator);
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Warrior Animator Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "This tool will generate:\n" +
            "• Animation clips from sprite sheets\n" +
            "• Animator Controller with states\n" +
            "• All transitions and parameters",
            MessageType.Info);
        
        GUILayout.Space(10);
        
        animationFrameRate = EditorGUILayout.FloatField("Animation Frame Rate", animationFrameRate);
        createAnimationClips = EditorGUILayout.Toggle("Create Animation Clips", createAnimationClips);
        
        GUILayout.Space(10);
        
        // Show existing animation clips if found
        GUILayout.Label("Animation Clips (auto-detected or assign manually):", EditorStyles.boldLabel);
        idleClip = (AnimationClip)EditorGUILayout.ObjectField("Idle", idleClip, typeof(AnimationClip), false);
        runClip = (AnimationClip)EditorGUILayout.ObjectField("Run", runClip, typeof(AnimationClip), false);
        attack1Clip = (AnimationClip)EditorGUILayout.ObjectField("Attack1", attack1Clip, typeof(AnimationClip), false);
        attack2Clip = (AnimationClip)EditorGUILayout.ObjectField("Attack2", attack2Clip, typeof(AnimationClip), false);
        guardClip = (AnimationClip)EditorGUILayout.ObjectField("Guard", guardClip, typeof(AnimationClip), false);
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("Find Existing Animation Clips", GUILayout.Height(30)))
        {
            FindExistingClips();
        }
        
        GUILayout.Space(10);
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Generate Animator Controller", GUILayout.Height(40)))
        {
            GenerateAll();
        }
        GUI.backgroundColor = Color.white;
    }
    
    private void FindExistingClips()
    {
        // Search for existing animation clips
        string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { ANIMATIONS_FOLDER });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            
            if (clip == null) continue;
            
            string clipName = clip.name.ToLower();
            
            if (clipName.Contains("idle") && idleClip == null)
                idleClip = clip;
            else if (clipName.Contains("run") && runClip == null)
                runClip = clip;
            else if ((clipName.Contains("attack1") || clipName.Contains("attack_1")) && attack1Clip == null)
                attack1Clip = clip;
            else if ((clipName.Contains("attack2") || clipName.Contains("attack_2")) && attack2Clip == null)
                attack2Clip = clip;
            else if (clipName.Contains("guard") && guardClip == null)
                guardClip = clip;
        }
        
        Debug.Log("[WarriorAnimatorGenerator] Found existing clips - check the window for results.");
    }
    
    private void GenerateAll()
    {
        // Ensure animations folder exists
        if (!AssetDatabase.IsValidFolder(ANIMATIONS_FOLDER))
        {
            AssetDatabase.CreateFolder("Assets", "Animations");
        }
        
        // Create animation clips if needed
        if (createAnimationClips)
        {
            CreateAnimationClips();
        }
        
        // Generate the animator controller
        GenerateAnimatorController();
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"[WarriorAnimatorGenerator] Successfully generated Animator Controller at: {ANIMATOR_OUTPUT_PATH}");
        
        // Select the created asset
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ANIMATOR_OUTPUT_PATH);
        if (controller != null)
        {
            Selection.activeObject = controller;
            EditorGUIUtility.PingObject(controller);
        }
    }
    
    private void CreateAnimationClips()
    {
        // Create clips from sprite sheets
        if (idleClip == null)
            idleClip = CreateAnimationClipFromSprites("Warrior_Idle", "Warrior_Idle", true);
        
        if (runClip == null)
            runClip = CreateAnimationClipFromSprites("Warrior_Run", "Warrior_Run", true);
        
        if (attack1Clip == null)
            attack1Clip = CreateAnimationClipFromSprites("Warrior_Attack1", "Warrior_Attack1", false);
        
        if (attack2Clip == null)
            attack2Clip = CreateAnimationClipFromSprites("Warrior_Attack2", "Warrior_Attack2", false);
        
        if (guardClip == null)
            guardClip = CreateAnimationClipFromSprites("Warrior_Guard", "Warrior_Guard", true);
    }
    
    private AnimationClip CreateAnimationClipFromSprites(string spriteName, string clipName, bool loop)
    {
        // Find the sprite sheet
        string spritePath = $"{WARRIOR_SPRITES_PATH}/{spriteName}.png";
        
        // Load all sprites from the sprite sheet
        Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(spritePath);
        
        if (sprites == null || sprites.Length <= 1)
        {
            Debug.LogWarning($"[WarriorAnimatorGenerator] Could not find sliced sprites at: {spritePath}. " +
                           "Make sure the sprite is imported as Multiple and sliced.");
            return null;
        }
        
        // Filter to only get Sprite objects (not the Texture2D)
        var spriteList = new System.Collections.Generic.List<Sprite>();
        foreach (var obj in sprites)
        {
            if (obj is Sprite sprite)
            {
                spriteList.Add(sprite);
            }
        }
        
        if (spriteList.Count == 0)
        {
            Debug.LogWarning($"[WarriorAnimatorGenerator] No sprites found in: {spritePath}");
            return null;
        }
        
        // Sort sprites by name (they should be named with numbers)
        spriteList.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
        
        // Create the animation clip
        AnimationClip clip = new AnimationClip();
        clip.name = clipName;
        clip.frameRate = animationFrameRate;
        
        // Create the sprite keyframes
        EditorCurveBinding spriteBinding = new EditorCurveBinding();
        spriteBinding.type = typeof(SpriteRenderer);
        spriteBinding.path = "";
        spriteBinding.propertyName = "m_Sprite";
        
        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[spriteList.Count];
        
        for (int i = 0; i < spriteList.Count; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe();
            keyframes[i].time = i / animationFrameRate;
            keyframes[i].value = spriteList[i];
        }
        
        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);
        
        // Set loop settings
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        
        // Save the clip
        string clipPath = $"{ANIMATIONS_FOLDER}/{clipName}.anim";
        AssetDatabase.CreateAsset(clip, clipPath);
        
        Debug.Log($"[WarriorAnimatorGenerator] Created animation clip: {clipPath} ({spriteList.Count} frames)");
        
        return clip;
    }
    
    private void GenerateAnimatorController()
    {
        // Create or overwrite the animator controller
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ANIMATOR_OUTPUT_PATH);
        
        // Get the root state machine
        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;
        
        // ========== ADD PARAMETERS ==========
        controller.AddParameter("IsRunning", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsGuarding", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Attack1", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Attack2", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Guard", AnimatorControllerParameterType.Trigger);
        
        // ========== CREATE STATES ==========
        
        // Idle State (Default)
        AnimatorState idleState = rootStateMachine.AddState("Idle", new Vector3(0, 0, 0));
        if (idleClip != null) idleState.motion = idleClip;
        rootStateMachine.defaultState = idleState;
        
        // Run State
        AnimatorState runState = rootStateMachine.AddState("Run", new Vector3(300, 0, 0));
        if (runClip != null) runState.motion = runClip;
        
        // Attack1 State
        AnimatorState attack1State = rootStateMachine.AddState("Attack1", new Vector3(150, 150, 0));
        if (attack1Clip != null) attack1State.motion = attack1Clip;
        
        // Attack2 State
        AnimatorState attack2State = rootStateMachine.AddState("Attack2", new Vector3(150, 250, 0));
        if (attack2Clip != null) attack2State.motion = attack2Clip;
        
        // Guard State
        AnimatorState guardState = rootStateMachine.AddState("Guard", new Vector3(300, 150, 0));
        if (guardClip != null) guardState.motion = guardClip;
        
        // ========== CREATE TRANSITIONS ==========
        
        // --- Idle <-> Run ---
        // Idle -> Run (when IsRunning = true)
        AnimatorStateTransition idleToRun = idleState.AddTransition(runState);
        idleToRun.AddCondition(AnimatorConditionMode.If, 0, "IsRunning");
        idleToRun.hasExitTime = false;
        idleToRun.duration = 0f;
        
        // Run -> Idle (when IsRunning = false)
        AnimatorStateTransition runToIdle = runState.AddTransition(idleState);
        runToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsRunning");
        runToIdle.hasExitTime = false;
        runToIdle.duration = 0f;
        
        // --- Any State -> Attack1 ---
        AnimatorStateTransition anyToAttack1 = rootStateMachine.AddAnyStateTransition(attack1State);
        anyToAttack1.AddCondition(AnimatorConditionMode.If, 0, "Attack1");
        anyToAttack1.hasExitTime = false;
        anyToAttack1.duration = 0f;
        anyToAttack1.canTransitionToSelf = false;
        
        // Attack1 -> Idle (after animation completes)
        AnimatorStateTransition attack1ToIdle = attack1State.AddTransition(idleState);
        attack1ToIdle.hasExitTime = true;
        attack1ToIdle.exitTime = 1f;
        attack1ToIdle.duration = 0f;
        
        // --- Any State -> Attack2 ---
        AnimatorStateTransition anyToAttack2 = rootStateMachine.AddAnyStateTransition(attack2State);
        anyToAttack2.AddCondition(AnimatorConditionMode.If, 0, "Attack2");
        anyToAttack2.hasExitTime = false;
        anyToAttack2.duration = 0f;
        anyToAttack2.canTransitionToSelf = false;
        
        // Attack2 -> Idle (after animation completes)
        AnimatorStateTransition attack2ToIdle = attack2State.AddTransition(idleState);
        attack2ToIdle.hasExitTime = true;
        attack2ToIdle.exitTime = 1f;
        attack2ToIdle.duration = 0f;
        
        // --- Any State -> Guard (when IsGuarding = true) ---
        AnimatorStateTransition anyToGuard = rootStateMachine.AddAnyStateTransition(guardState);
        anyToGuard.AddCondition(AnimatorConditionMode.If, 0, "IsGuarding");
        anyToGuard.hasExitTime = false;
        anyToGuard.duration = 0f;
        anyToGuard.canTransitionToSelf = false;
        
        // Guard -> Idle (when IsGuarding = false)
        AnimatorStateTransition guardToIdle = guardState.AddTransition(idleState);
        guardToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsGuarding");
        guardToIdle.hasExitTime = false;
        guardToIdle.duration = 0f;
        
        // Save the controller
        EditorUtility.SetDirty(controller);
    }
}
