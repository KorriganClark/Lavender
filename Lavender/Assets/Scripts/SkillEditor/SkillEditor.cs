using Lavender;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Sirenix.Utilities;
using Lavender.Cardinal;
public class SkillEditor : OdinMenuEditorWindow
{

    /// <summary>
    /// 摄像机
    /// </summary>
    private static GameObject CameraObj;

    [MenuItem("LavenderTools/技能编辑器")]

    /// <summary>
    /// 窗口初始化
    /// </summary>
    private static void OpenWindow()
    {
        var window = GetWindow<SkillEditor>();
        //Debug.Log(window.position);
        //window.position = new Rect(100, 100, 500, 500);
        window.Show();
        window.Init();
    }
    private OdinMenuTree tempTree;
    private bool isEditingConfig;
    public static SkillEditorConfig editorConfig;
    public void Init()
    {
        monsterControl = ScriptableObject.CreateInstance<MonsterControl>();
        monsterControl.skillEditor = this;
    }
    /// <summary>
    /// 构建菜单
    /// </summary>
    /// <returns></returns>
    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree(false);
        tempTree = tree;
        var customMenuStyle = new OdinMenuStyle
        {
            BorderPadding = 0f,
            AlignTriangleLeft = true,
            TriangleSize = 16f,
            TrianglePadding = 0f,
            Offset = 20f,
            Height = 23,
            IconPadding = 0f,
            BorderAlpha = 0.323f
        };

        tree.DefaultMenuStyle = customMenuStyle;
        tree.Config.DrawSearchToolbar = true;
        LoadAsset();
        tree.EnumerateTree()
            .AddThumbnailIcons()
            .SortMenuItemsByName();
        var buttonItem = new ButtonItem(tree);
        tree.AddMenuItemAtPath("技能", buttonItem);
        //SkillEditorConfig config = AssetDatabase.LoadAssetAtPath<SkillEditorConfig>("Assets/Editor/SkillEditor/config.Asset");
        tree.AddAssetAtPath("配置", "Assets/Editor/SkillEditor/config.Asset");
        tree.Selection.SelectionChanged += (x) =>
        {
            if (x == SelectionChangedType.ItemAdded && tree.Selection.Count == 1)
            {
                if (skillConfig != null)
                {
                    DataSetDirty();
                }
                if (tree.Selection.SelectedValue is SkillConfig)
                {
                    skillConfig = (SkillConfig)tree.Selection.SelectedValue;
                    skillConfig.ownerProcessor = this;
                    LoadSkillConfig();
                    isEditingConfig = false;
                }
                else if (tree.Selection.SelectedValue is SkillEditorConfig)
                {
                    isEditingConfig = true;
                }
            }
        };
        return tree;
    }
    /// <summary>
    /// 新建角色使用
    /// </summary>
    public static GameObject newModel;
    /// <summary>
    /// 添加角色用的按钮
    /// </summary>
    private class ButtonItem : OdinMenuItem
    {
        //private readonly SomeCustomClass instance;

        public ButtonItem(OdinMenuTree tree) : base(tree, "Add Character", null)
        {
            //this.instance = instance;
        }

        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
        {

        }

        public override void Select(bool addToSelection = false)
        {
            AddCharacter();
        }
    }

    public static GameObject editorObj;
    /// <summary>
    /// 创建摄像机与rendertexture，将其投影到窗口上
    /// 添加窗口关闭事件
    /// </summary>
    protected override void OnEnable()
    {
        var window = GetWindow<SkillEditor>();
        editorObj = new GameObject("SkillEditor");
        CameraObj = new GameObject("myCamera");
        CameraObj.transform.SetParent(editorObj.transform);
        Camera cam = SkillEditor.CameraObj.AddComponent<Camera>();
        cam.depth = -1;
        aView = new RenderTexture(500, 500, 32);
        cam.targetTexture = aView;
        CameraObj.transform.position = new Vector3(1000, 0, 2);
        CameraObj.transform.rotation = new Quaternion(0, 1, 0, 0);
        audioSource = CameraObj.AddComponent<AudioSource>();
        audioListener = CameraObj.AddComponent<AudioListener>();
        timeLine = new LavenderTimeLine();
        timeLine.owner = this;
        cameraControl = new EditorCameraControl();
        cameraControl.TargetCamera = CameraObj;
        LoadEditorConfig();

    }
    private void LoadEditorConfig()
    {
        SkillEditorConfig config = AssetDatabase.LoadAssetAtPath<SkillEditorConfig>("Assets/Editor/SkillEditor/config.Asset");
        if (config == null)
        {
            config = new SkillEditorConfig();
            AssetDatabase.CreateAsset(config, "Assets/Editor/SkillEditor/config.Asset");
        }
        editorConfig = config;
        var window = GetWindow<SkillEditor>();
        window.position = editorConfig.position;
    }
    private void LoadAsset()
    {
        tempTree.AddAllAssetsAtPath("技能", "Editor/SkillEditor/Character", typeof(SkillConfig), true);
    }
    private void OnDisable()
    {
        OnViewEnd();
        if (true)
        {
            //加上这个if,就不会出现两个窗口了……
        }
    }
    #region 动画组件、变量
    private static Animator showAnimator;
    public static GameObject showModel;
    private static AnimViewDirector director;
    public static bool isPlaying;
    public static float scaleTotalTime;
    [OnValueChanged("DataSetDirty", includeChildren: true)]
    public SkillConfig skillConfig;
    public static RenderTexture aView;
    public float timeLineValue;
    public LavenderTimeLine timeLine;
    public AudioSource audioSource;
    public AudioListener audioListener;
    public EditorCameraControl cameraControl;
    public class MonsterControl : OdinEditorWindow
    {

        //if (GUILayout.Button("添加测试怪物", GUILayout.Width(100f), GUILayout.Height(20f)))
        [Button("添加测试怪物")]
        public void AddMonster()
        {
            var monster = new TestMonster();
            monster.monster = SkillEditor.editorConfig.Monster;
            monster.ChangeMonster();
            monster.owner = this;
            monsters.Add(monster);
            monsterTransforms.Add(monster.showMonster.transform.name, monster.showMonster.transform);
            monsterNames = monsterTransforms.Keys.ToImmutableList();
            skillEditor.LoadSkillConfig();
        }
        [HideInInspector]
        public SkillEditor skillEditor;
        [HideInInspector]
        public IEnumerable<String> monsterNames;
        [HideInInspector]
        public Dictionary<string, Transform> monsterTransforms = new Dictionary<string, Transform>();
        [TableList]
        public List<TestMonster> monsters = new List<TestMonster>();
        public void Draw()
        {
            OnGUI();
        }
    }
    public MonsterControl monsterControl;// = ScriptableObject.CreateInstance<MonsterControl>();// <MonsterControl>();
    public class TestMonster
    {
        [OnValueChanged("ChangeMonster")]
        [PreviewField(Alignment = ObjectFieldAlignment.Center)]
        public LavenderCharacter monster;
        [HideInInspector]
        public GameObject showMonster;
        //[InlineEditor(InlineEditorModes.GUIAndHeader, InlineEditorObjectFieldModes.Hidden)]
        [OnValueChanged("RefreshPos")]
        [VerticalGroup("Transform"), LabelWidth(60), LabelText("Position")]
        [TableColumnWidth(300, resizable: false)]
        public Vector3 monsterPosition = new Vector3(1000, 0, 0);
        [OnValueChanged("RefreshPos")]
        [VerticalGroup("Transform"), LabelWidth(60)]
        [TableColumnWidth(300, resizable: false)]
        public Quaternion rotation = new Quaternion();
        [HideInInspector]
        public MonsterControl owner;

        public void ChangeMonster()
        {
            if (showMonster)
            {
                GameObject.DestroyImmediate(showMonster);
            }
            showMonster = monster.CreateCharacterInWorld(new Vector3(1000, 0, 0), false);
            showMonster.transform.SetParent(SkillEditor.editorObj.transform);
            //monsterTransform.SetParent(CameraObj.transform);
        }
        public void RefreshPos()
        {
            //if (monsterPosition && rotation)
            {
                showMonster.transform.SetPositionAndRotation(monsterPosition, rotation);
            }
        }

        [Button("删除")]
        public void Delete()
        {
            owner.monsters.Remove(this);
            DestroyImmediate(showMonster);
        }
    }

    #endregion

    public AnimViewDirector Director
    {
        get
        {
            return director;
        }
    }

    #region 系统初始化
    public void LoadSkillConfig()
    {
        ChangeModel();
        if (showModel.GetComponent<CardinalSystem>() == null)
        {
            showModel.AddComponent<CardinalSystem>();
        }
        skillConfig.ImportAnimations();
        skillConfig.AudioEntrepot.Clear();
        skillConfig.LoadJoints(showModel);
        skillConfig.Detail = null;
        skillConfig.InitCurve();
        InitPlayGraph();
        InitTimeLine();
        SetScaleTotalTime(director.totalTime);
    }
    /// <summary>
    /// 初始化时间轴轨道
    /// </summary>
    public void InitTimeLine()
    {
        if (!skillConfig.CheckValid())
        {
            Debug.LogError("Config不完整或有误，请重新生成。");
            return;
        }
        timeLine.Init();
        timeLine.tracks[0].lavenderClips = skillConfig.LavenderClips[0];//动画主轨道
        for (int i = 1; i < skillConfig.LavenderClips.Count; i++)
        {
            timeLine.tracks[i].lavenderClips = skillConfig.LavenderClips[i];
        }

    }
    /// <summary>
    /// 初始化播放器
    /// </summary>
    public void InitPlayGraph()
    {
        if (director == null)
        {
            director = AnimViewDirector.Create(showModel, skillConfig);
        }
        else
        {
            director.Init(showModel, skillConfig);
        }
        timeLineValue = 0;
        isPlaying = false;
        director.Stop();
        director.currentTime = timeLineValue;
        scaleTotalTime = Math.Max(director.totalTime, scaleTotalTime);
    }
    #endregion

    public static void AddCharacter()
    {
        EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, "t:Prefab ", 99999);
    }
    public static void ResetModelPos()
    {
        if (showModel != null)
        {
            showModel.transform.SetPositionAndRotation(new Vector3(1000, -1, -2), new Quaternion());
        }
    }
    public void Play()
    {
        if (timeLineValue - scaleTotalTime >= -0.1)
        {
            director.currentTime = 0;
        }
        isPlaying = true;
        director.Play();
        EditorUtility.SetDirty(skillConfig.Model);
    }
    public void Stop()
    {
        isPlaying = false;
        director.Stop();
    }

    /// <summary>
    /// 刷新时处理事件
    /// </summary>
    void Update()
    {
        //ForceUpdate();
        this.Repaint();
        if (director != null)
        {
            isPlaying = director.isPlaying;
            if (isPlaying)
            {

                if (director.currentTime > director.totalTime - 1e-4)
                {
                    director.currentTime = director.totalTime;
                    director.Stop();
                }
                timeLineValue = (float)director.currentTime;
                timeLine.currentTime = timeLineValue;
            }
            else
            {
                if (timeLineValue != timeLine.currentTime)
                {
                    timeLineValue = timeLine.currentTime;
                    director.currentTime = timeLineValue;
                    director.Evaluate();
                }

            }
        }
        /*
                if (animConfig && animConfig.AudioEntrepot.Count == 0)
                {
                    foreach (var clip in animConfig.AudioList)
                    {
                        animConfig.AudioEntrepot.Add(clip);
                    }
                }*/
    }

    public void SetScaleTotalTime(float totalTime)
    {
        scaleTotalTime = totalTime;
    }
    public float GetAnimTotalTime()
    {
        if (director != null)
        {
            return director.totalTime;
        }
        return 0;
    }
    /// <summary>
    /// 选择的模型改变时，切换新的模型
    /// </summary>
    private void ChangeModel()
    {
        //InspectObject(Models);
        timeLineValue = 0;
        isPlaying = false;

        if (showModel != null)
        {
            DestroyImmediate(showModel);
            showModel = null;
        }
        if (skillConfig.Model != null)
        {
            //string prefabPath = animConfig.Model.name;
            showModel = Instantiate(skillConfig.Model);
            //showModel = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
            if (showModel == null)
            {
                return;
            }
            var animator = showModel.GetComponent<Animator>();
            if (animator != null)
            {
                animator.applyRootMotion = false;
            }
            showModel.transform.position = new Vector3(1000, -1, -2);
            cameraControl.TargetCharacter = showModel;
            if (showModel.GetComponent<LavenderUnitMoveControl>() == null)
            {
                showModel.AddComponent<LavenderUnitMoveControl>();
            }
            //showAnimator = showModel.GetComponent<Animator>();
        }
        else
        {
            director.Destory();
        }
    }

    protected override void OnGUI()
    {
        base.OnGUI();

    }

    UnityEngine.Vector2 configScrollPosition;
    public int byFrame = 1;
    private string[] byFrameString = { "秒", "帧(30)" };
    protected override void OnBeginDrawEditors()
    {
        if (isEditingConfig)
        {
            return;
        }
        EditorGUILayout.BeginHorizontal(GUILayout.Height(500f));
        EditorGUILayout.BeginVertical();

        var rect = EditorGUILayout.GetControlRect(GUILayout.Width(500), GUILayout.Height(500f));
        GUI.DrawTexture(rect, aView);
        cameraControl.UpdateCamera(rect);
        rect = EditorGUILayout.GetControlRect(GUILayout.Width(500), GUILayout.Height(20f));
        if (byFrame == 0)
        {
            GUI.Label(rect, new GUIContent(Convert.ToString(timeLineValue) + "s"));
        }
        else
        {
            GUI.Label(rect, new GUIContent(Convert.ToString((int)(timeLineValue * 30)) + "f"));
        }
        rect.x = rect.x + 250f;
        rect.width = 100f;
        cameraControl.Forcus = GUI.Toggle(rect, cameraControl.Forcus, "锁定角色");
        rect.x = rect.x + 100f;
        rect.width = 150f;
        byFrame = GUI.Toolbar(rect, byFrame, byFrameString);
        rect = EditorGUILayout.GetControlRect(GUILayout.Width(500), GUILayout.Height(20f));
        if (GUI.Button(rect, isPlaying ? "暂停" : "播放"))
        {
            if (isPlaying)
            {
                Stop();
            }
            else
            {
                Play();
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        configScrollPosition = EditorGUILayout.BeginScrollView(
        configScrollPosition, GUILayout.Width(position.width - MenuWidth - 550), GUILayout.Height(340));
        //EditorGUILayout.BeginScrollView();
        //EditorGUILayout.BeginVertical();

    }
    UnityEngine.Vector2 monsterScrollPosition;
    protected override void OnEndDrawEditors()
    {
        if (isEditingConfig)
        {
            return;
        }
        EditorGUILayout.EndScrollView();
        monsterScrollPosition = EditorGUILayout.BeginScrollView(
        monsterScrollPosition, GUILayout.Width(position.width - MenuWidth - 550), GUILayout.Height(150));
        if (monsterControl == null)
        {
            Init();
        }
        monsterControl.Draw();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        //timeLine.currentTime = timeLineValue;
        EditorGUILayout.BeginVertical();
        timeLine.totalTime = scaleTotalTime;
        timeLine.DrawTimeLine(position.width - MenuWidth - 25);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("添加音频轨道", GUILayout.Width(100f), GUILayout.Height(20f)))
        {
            skillConfig.LavenderClips.Add(new List<LavenderClip>());
            skillConfig.trackTypes.Add(TrackType.Audio);
            InitTimeLine();
            DataSetDirty();
        }
        if (GUILayout.Button("添加特效轨道", GUILayout.Width(100f), GUILayout.Height(20f)))
        {
            skillConfig.LavenderClips.Add(new List<LavenderClip>());
            skillConfig.trackTypes.Add(TrackType.Particle);
            InitTimeLine();
            DataSetDirty();
        }
        if (GUILayout.Button("添加打击范围轨道", GUILayout.Width(100f), GUILayout.Height(20f)))
        {
            skillConfig.LavenderClips.Add(new List<LavenderClip>());
            skillConfig.trackTypes.Add(TrackType.Attack);
            InitTimeLine();
            DataSetDirty();
        }
        if (GUILayout.Button("添加位移轨道", GUILayout.Width(100f), GUILayout.Height(20f)))
        {
            skillConfig.LavenderClips.Add(new List<LavenderClip>());
            skillConfig.trackTypes.Add(TrackType.Move);
            InitTimeLine();
            DataSetDirty();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        Event e = Event.current;
        if (e.type == EventType.ExecuteCommand && Event.current.commandName == "ObjectSelectorClosed")
        {
            if (newModel = (GameObject)EditorGUIUtility.GetObjectPickerObject())
            {
                SkillConfig newConfig = ScriptableObject.CreateInstance<SkillConfig>();
                newConfig.Model = newModel;
                CreateSkill(newConfig);
                AssetDatabase.CreateAsset(newConfig, "Assets/Editor/SkillEditor/Character/" + newConfig.SkillName + ".asset");

            }
        }
    }
    public LavenderClip chosedClip;
    public void SetChosedClip(LavenderClip clip)
    {
        skillConfig.Detail = clip;
        skillConfig.Detail.jointsNames = skillConfig.joints.Keys.ToImmutableList();
        chosedClip = clip;
        /*
        foreach (var monster in monsterControl.monsters)
        {
            skillConfig.Detail.baseNames.AppendWith(monster.showMonster.transform.name);
        }
        skillConfig.Detail.joints = skillConfig.joints;
        foreach (var monster in monsterControl.monsters)
        {
            skillConfig.Detail.joints.AppendWith(new KeyValuePair<string, Transform>(monster.showMonster.transform.name, monster.showMonster.transform));
        }*/
    }
    public void SetChosedVal(LavenderClip clip)
    {
        if (clip == chosedClip)
        {
            clip = skillConfig.Detail;
        }
    }
    public Action SaveChosedVal;
    /// <summary>
    /// 对Config信息进行完善
    /// </summary>
    /// <param name="config"></param>
    public void CreateSkill(SkillConfig config)
    {
        config.SkillName = config.Model.name + "Skill";
    }
    /// <summary>
    /// 删除轨道
    /// </summary>
    /// <param name="index"></param>
    public void RemoveTrack(int index)
    {
        skillConfig.LavenderClips.RemoveAt(index);
        skillConfig.trackTypes.RemoveAt(index);
        DataSetDirty();
    }
    /// <summary>
    /// 置脏数据
    /// </summary>
    public void DataSetDirty()
    {
        EditorUtility.SetDirty(skillConfig);
        EditorUtility.SetDirty(skillConfig.Model);
    }
    public static GameObject updateObj;
    public void ForceUpdate()
    {
        if (updateObj == null)
        {
            updateObj = new GameObject("ForceUpdate");
        }
        if (updateObj.transform.position.x == -1)
        {
            updateObj.transform.position = new Vector3(1, 0, 0);
        }
        else
        {
            updateObj.transform.position = new Vector3(-1, 0, 0);
        }
    }
    public void SetConfigData()
    {
        for (int i = 1; i < skillConfig.LavenderClips.Count; i++)
        {
            skillConfig.LavenderClips[i]=timeLine.tracks[i].lavenderClips;
        }
    }
    /// <summary>
    /// 窗口关闭事件处理
    /// 销毁摄像机、模型
    /// </summary>
    private static void OnViewEnd()
    {
        DestroyImmediate(CameraObj);
        DestroyImmediate(aView);
        DestroyImmediate(updateObj);
        if (showModel != null)
        {
            DestroyImmediate(showModel);
            showModel = null;
        }
        DestroyImmediate(editorObj);
        director.Destory();
        var window = GetWindow<SkillEditor>();
        window.SetConfigData();
        //EditorUtility.SetDirty(animConfig);
        AssetDatabase.SaveAssets();
        editorConfig.position = window.position;
        
        //SaveGraph();
    }


    /// <summary>
    /// Adds newly (if not already in the list) found assets.
    /// Returns how many found (not how many added)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="assetsFound">Adds to this list if it is not already there</param>
    /// <returns></returns>
    public static int TryGetUnityObjectsOfTypeFromPath<T>(string path, List<T> assetsFound) where T : UnityEngine.Object
    {
        string[] filePaths = System.IO.Directory.GetFiles(path);

        int countFound = 0;

        if (filePaths != null && filePaths.Length > 0)
        {
            for (int i = 0; i < filePaths.Length; i++)
            {
                UnityEngine.Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath(filePaths[i], typeof(T));
                if (obj is T asset)
                {
                    countFound++;
                    if (!assetsFound.Contains(asset))
                    {
                        assetsFound.Add(asset);
                    }
                }
            }
        }

        return countFound;
    }
}