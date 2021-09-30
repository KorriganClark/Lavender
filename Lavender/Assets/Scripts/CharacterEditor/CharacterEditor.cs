using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;

namespace Lavender
{


    public class CharacterEditor : OdinMenuEditorWindow
    {
        [MenuItem("LavenderTools/角色编辑器")]
        private static void OpenWindow()
        {
            var window = GetWindow<CharacterEditor>();
            window.Show();
        }
        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(false);
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
            tree.AddAllAssetsAtPath("角色", "Editor/CharacterEditor/Characters", typeof(LavenderCharacter), true);
            var buttonItem = new ButtonItem(tree);
            tree.AddMenuItemAtPath("角色", buttonItem);
            //tree.Selection.SupportsMultiSelect = false;
            tree.Selection.SelectionChanged += (x) =>
            {
                if (x == SelectionChangedType.ItemAdded)
                {
                    characterConfig = (LavenderCharacter)tree.Selection.SelectedValue;
                    LoadCharacter();
                }
            };
            return tree;
            throw new System.NotImplementedException();
        }
        private class ButtonItem : OdinMenuItem
        {
            //private readonly SomeCustomClass instance;

            public ButtonItem(OdinMenuTree tree) : base(tree, "新建角色", null)
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
        public static void AddCharacter()
        {
            EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, "t:Prefab ", 999);
        }
        public void TryCreateCharacter()
        {
            Event e = Event.current;
            Object newModel;
            if (e.type == EventType.ExecuteCommand && Event.current.commandName == "ObjectSelectorClosed")
            {
                if ((newModel = EditorGUIUtility.GetObjectPickerObject()) != null && UnityEditor.PrefabUtility.IsPartOfModelPrefab(newModel) && newModel is GameObject)
                {
                    LavenderCharacter newConfig = ScriptableObject.CreateInstance<LavenderCharacter>();
                    newConfig.model = (GameObject)newModel;
                    newConfig.characterName = newModel.name;
                    AssetDatabase.CreateAsset(newConfig, "Assets/Editor/CharacterEditor/Characters/" + newConfig.characterName + ".asset");
                }
            }
        }
        #region 编辑器相关变量
        public static GameObject CharacterEditorObj;
        RenderTexture characterView;
        private static GameObject CameraObj;
        public AudioSource audioSource;
        public AudioListener audioListener;
        //[OnValueChanged("UpdateData")]
        public LavenderCharacter characterConfig;
        public GameObject showModel;
        public CharacterEditorControl editorControl;
        public int characterState;
        #endregion
        #region 编辑器初始化与关闭处理
        protected override void OnEnable()
        {

            var window = GetWindow<CharacterEditor>();
            CharacterEditorObj = new GameObject("characterObj");
            CameraObj = new GameObject("myCamera");
            CameraObj.transform.SetParent(CharacterEditorObj.transform);
            Camera cam = CharacterEditor.CameraObj.AddComponent<Camera>();
            cam.depth = -1;
            characterView = new RenderTexture(500, 500, 32);
            cam.targetTexture = characterView;
            CameraObj.transform.position = new Vector3(-1000, 0, 2);
            CameraObj.transform.rotation = new Quaternion(0, 1, 0, 0);
            audioSource = CameraObj.AddComponent<AudioSource>();
            audioListener = CameraObj.AddComponent<AudioListener>();
            window.OnClose += () => shoudClose = false; ;
            editorControl = ScriptableObject.CreateInstance<CharacterEditorControl>();
        }
        public bool shoudClose = true;
        public static void OnViewEnd()
        {
            Object.DestroyImmediate(CharacterEditorObj);
            AssetDatabase.SaveAssets();
        }
        private void OnDisable()
        {
            OnViewEnd();
            if (shoudClose)
            {
                //加上这个if,就不会出现两个窗口了……
            }
        }
        public void LoadCharacter()
        {
            if (characterConfig == null)
            {
                return;
            }
            ChangeModel();
            UpdateData();

        }
        public void UpdateData()
        {
            if (characterConfig.moveAsset)
            {
                characterConfig.moveAnim = characterConfig.moveAsset.moveClips;
            }
            characterConfig.control = editorControl;
            if (characterConfig.skills != null)
            {
                editorControl.ImportSkill(characterConfig.skills);
            }
        }
        public void ChangeModel()
        {
            if (showModel != null)
            {
                DestroyImmediate(showModel);
                showModel = null;
            }
            if (characterConfig.model != null)
            {
                showModel = Instantiate(characterConfig.model);
                if (showModel != null)
                {
                    showModel.transform.SetParent(CharacterEditorObj.transform);
                    showModel.transform.position = new Vector3(-1000, -1, -2);
                    var animator = showModel.GetComponent<Animator>();
                    if (animator)
                    {
                        animator.applyRootMotion = false;
                    }
                    var characterController = showModel.GetComponent<LavenderCharacterControl>();
                    if (showModel.GetComponent<LavenderUnitMoveControl>() == null)
                    {
                        showModel.AddComponent<LavenderUnitMoveControl>();
                    }
                    if (characterController == null)
                    {
                        characterController = showModel.AddComponent<LavenderCharacterControl>();
                    }
                    characterController.editorControl = editorControl;
                    editorControl.Init(showModel, characterConfig);
                }
            }
        }
        #endregion
        #region UI元素
        public string[] charaterStateToolBar = { "静止", "行走", "跑步", "跳跃" };
        #endregion
        protected override void OnBeginDrawEditors()
        {
            base.OnBeginDrawEditors();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            var rect = EditorGUILayout.GetControlRect(GUILayout.Width(500), GUILayout.Height(500f));
            GUI.DrawTexture(rect, characterView);
            editorControl.Draw();
            EditorGUILayout.EndVertical();
        }
        protected override void OnEndDrawEditors()
        {
            EditorGUILayout.EndHorizontal();
            base.OnEndDrawEditors();
            TryCreateCharacter();
        }
        void Update()
        {
            this.Repaint();
        }
    }
}