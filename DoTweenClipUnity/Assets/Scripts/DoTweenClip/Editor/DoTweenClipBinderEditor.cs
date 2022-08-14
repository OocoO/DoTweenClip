using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.DOTweenEditor;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Carotaa.Code.Editor
{
    [CustomEditor(typeof(DoTweenClipBinder))]
    public class DoTweenClipBinderEditor : UnityEditor.Editor
    {
        private DoTweenClipBinder Binder => target as DoTweenClipBinder;

        private Transform _root;
        private DoTweenClip _clip;
        private DoTweenModuleAnimationClip.PropertyName[] _names;
        private string[] _contents;
        private int _otherIndex;
        
        private void OnEnable()
        {
            _root = Binder.transform;
            _names = DoTweenModuleAnimationClip.PropertyName.GetAll();
            _contents = _names.Select(x => x.ToString()).ToArray();
            _otherIndex = Array.FindIndex(_names, x
                => Equals(x, DoTweenModuleAnimationClip.PropertyName.Other));
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var title = _clip ? "Bind Reference" : "Choice a clip first";
            EditorGUILayout.LabelField(title);
            EditorGUI.BeginChangeCheck();
            var clip = (DoTweenClip) EditorGUILayout.ObjectField("Clip", _clip, typeof(DoTweenClip), false);
            if (EditorGUI.EndChangeCheck())
            {
                _clip = clip;
            }

            if (!_clip) return;
            
            if (GUILayout.Button("Play"))
            {
                var tweener = Binder.transform.DoAnimationClipAbsolute(_clip);
                tweener.SetLink(Binder.gameObject);
                tweener.OnComplete(() =>
                {
                    DOTweenEditorPreview.Stop();
                });
                DOTweenEditorPreview.PrepareTweenForPreview(tweener, false);
                DOTweenEditorPreview.Start();
            }

            var curves = _clip.Curves;
            var changed = false;
            EditorGUI.indentLevel++;
            foreach (var curve in curves)
            {
                DrawCurveInspector(curve, ref changed);
            }
            EditorGUI.indentLevel--;

            if (changed)
            {
                EditorUtility.SetDirty(_clip);
                AssetDatabase.SaveAssets();
            }
        }

        private void DrawCurveInspector(DoTweenClipCurve curve, ref bool changed)
        {
            var style = new GUIStyle(GUI.skin.textField);
            var labelColor = EditorStyles.label.normal.textColor;
            var isCurveError =
                !DoTweenModuleAnimationClip.PropertyBridge.TryGetPropertyBridge(Binder.transform, curve, out var setter);

            if (isCurveError)
            {
                style.normal.textColor = Color.yellow;
                EditorStyles.label.normal.textColor = Color.yellow;
            }
            
            EditorGUI.BeginChangeCheck();
            var curveName = EditorGUILayout.TextField("Name", curve.Name);

            if (EditorGUI.EndChangeCheck())
            {
                curve.Name = curveName;
                changed = true;
            }
            EditorGUI.indentLevel++;
            
            EditorGUI.BeginChangeCheck();
            var refObject = curve.FindRefTarget(_root);
            var refTarget = EditorGUILayout.ObjectField("Ref Target:", refObject, typeof(Object), true);
            if (EditorGUI.EndChangeCheck())
            {
                var trans = GetTargetTrans(refTarget);
                if (trans)
                {
                    var success = TryGetHierarchyPath(_root, trans, out var path);
                    if (success)
                    {
                        curve.Path = path;
                        curve.TargetType = refTarget.GetType();

                        changed = true;
                    }
                }
            }
            
            EditorGUI.BeginChangeCheck();
            var pathInput = EditorGUILayout.TextField("Path:", curve.Path, style);
            if (EditorGUI.EndChangeCheck())
            {
                curve.Path = pathInput;
                changed = true;
            }
            
            EditorGUI.BeginChangeCheck();
            var index = Array.FindIndex(_names, 0,x => x.Name == curve.PropertyName);
            if (index < 0)
            {
                index = _otherIndex;
            }
            var select = EditorGUILayout.Popup("Build In Property", index, _contents);
            var selectPn = _names[select];
            
            if (EditorGUI.EndChangeCheck())
            {
                curve.PropertyName = selectPn.Name;
                
                changed = true;
            }


            EditorGUI.BeginChangeCheck();
            var text = EditorGUILayout.TextField("Property Name: ", curve.PropertyName, style);
            
            if (EditorGUI.EndChangeCheck())
            {
                curve.PropertyName = text;
                    
                changed = true;
            }
            
            EditorStyles.label.normal.textColor = labelColor;
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }


        private static Transform GetTargetTrans(Object target)
        {
            Transform trans = null;
            if (target)
            {
                if (target is Component cp)
                {
                    trans = cp.transform;
                }
                else if (target is GameObject go)
                {
                    trans = go.transform;
                }
            }

            return trans;
        }

        private static readonly StringBuilder Builder = new StringBuilder();
        private static readonly Stack<string> Stack = new Stack<string>();

        public static bool TryGetHierarchyPath(Transform root, Transform child, out string path)
        {
            Builder.Clear();
            Stack.Clear();
            
            var target = child;
            while (target != null)
            {
                if (target == root)
                {
                    // success
                    while (Stack.Count > 1)
                    {
                        Builder.Append(Stack.Pop());
                        Builder.Append("/");
                    }

                    if (Stack.Count > 0)
                    {
                        Builder.Append(Stack.Pop());
                        path = Builder.ToString();
                    }
                    else
                    {
                        path = string.Empty;
                    }
                    return true;

                }
                else
                {
                    Stack.Push(target.gameObject.name);
                    target = target.parent;
                }
            }

            path = null;
            return false;
        }
    }
}