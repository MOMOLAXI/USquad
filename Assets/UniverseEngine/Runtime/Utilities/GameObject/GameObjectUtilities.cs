using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UniverseEngine
{
    public static class GameObjectUtilities
    {
        static readonly List<Transform> s_TempTransforms = new();
        static readonly List<GraphicRaycaster> s_TempRaycasters = new();

        public static T Find<T>(this Transform trans, string name = "") where T : class
        {
            if (string.IsNullOrEmpty(name))
            {
                return trans.GetComponent<T>();
            }

            Transform node = trans.Find(name);
            if (node == null)
            {
                return default;
            }

            return node.GetComponent<T>();
        }

        public static T Find<T>(this GameObject go, string name = "") where T : class
        {
            if (string.IsNullOrEmpty(name))
            {
                return go.GetComponent<T>();
            }

            Transform node = go.transform.Find(name);
            if (node == null)
            {
                return default;
            }

            return node.GetComponent<T>();
        }

        public static bool BelongsToLayerMask(int layer, int layerMask)
        {
            return (layerMask & 1 << layer) > 0;
        }

        public static void SetLayerRecursively(this Transform parent, int layer, int ignoreLayerMask = 0)
        {
            if (parent == null)
            {
                return;
            }

            s_TempTransforms.Clear();
            parent.GetComponentsInChildren(true, s_TempTransforms);
            foreach (Transform transform in s_TempTransforms)
            {
                if ((1 << transform.gameObject.layer & ignoreLayerMask) == 0)
                {
                    transform.gameObject.layer = layer;
                }
            }
        }

        public static void SetGraphicRaycasterState(this GameObject root, bool state)
        {
            if (root == null)
            {
                return;
            }

            s_TempRaycasters.Clear();
            root.GetComponentsInChildren(true, s_TempRaycasters);
            foreach (GraphicRaycaster raycaster in s_TempRaycasters)
            {
                raycaster.enabled = state;
            }
        }

        public static void SetGraphicRaycasterMask(this GameObject root, LayerMask mask)
        {
            if (root == null)
            {
                return;
            }

            s_TempRaycasters.Clear();
            root.GetComponentsInChildren(true, s_TempRaycasters);
            foreach (GraphicRaycaster raycaster in s_TempRaycasters)
            {
                raycaster.blockingMask = mask;
            }
        }

        public static void SetGraphicRaycasterBlockingObjects(this GameObject root, GraphicRaycaster.BlockingObjects blockingObjects)
        {
            if (root == null)
            {
                return;
            }

            s_TempRaycasters.Clear();
            root.GetComponentsInChildren(true, s_TempRaycasters);
            foreach (GraphicRaycaster raycaster in s_TempRaycasters)
            {
                raycaster.blockingObjects = blockingObjects;
            }
        }

        public static void StretchHorizontalAndVerticle(this RectTransform transform)
        {
            transform.anchorMax = Vector2.one;
            transform.anchorMin = Vector2.zero;
            transform.sizeDelta = Vector2.zero;
            transform.localScale = Vector3.one;
            transform.anchoredPosition = Vector2.zero;
            transform.anchoredPosition3D = Vector3.zero;
        }

        public static void StretchTop(this RectTransform transform)
        {
            transform.anchorMax = new(0, 1);
            transform.anchorMin = new(1, 1);
        }

        public static void StretchHorizontal(this RectTransform transform)
        {
            transform.anchorMax = new(1, 0.5f);
            transform.anchorMin = new(0, 0.5f);
        }

        public static void StretchBottom(this RectTransform transform)
        {
            transform.anchorMax = new(0, 0);
            transform.anchorMin = new(1, 0);
        }

        public static void StretchRight(this RectTransform transform)
        {
            transform.anchorMax = new(1, 0);
            transform.anchorMin = new(1, 1);
        }

        public static void StretchVerticle(this RectTransform transform)
        {
            transform.anchorMax = new(0.5f, 1);
            transform.anchorMin = new(0.5f, 0);
        }

        public static void StretchLeft(this RectTransform transform)
        {
            transform.anchorMax = new(0, 1);
            transform.anchorMin = new(0, 0);
        }

        public static void AnchorLeftTop(this RectTransform transform)
        {
            transform.anchorMax = new(0, 1);
            transform.anchorMin = new(0, 1);
        }

        public static void AnchorTop(this RectTransform transform)
        {
            transform.anchorMax = new(0.5f, 1);
            transform.anchorMin = new(0.5f, 1);
        }

        public static void AnchorRightTop(this RectTransform transform)
        {
            transform.anchorMax = new(1, 1);
            transform.anchorMin = new(1, 1);
        }

        public static void AnchorLeft(this RectTransform transform)
        {
            transform.anchorMax = new(0, 0.5f);
            transform.anchorMin = new(0, 0.5f);
        }

        public static void AnchorCenter(this RectTransform transform)
        {
            transform.anchorMax = new(0.5f, 0.5f);
            transform.anchorMin = new(0.5f, 0.5f);
        }

        public static void AnchorRight(this RectTransform transform)
        {
            transform.anchorMax = new(1, 0.5f);
            transform.anchorMin = new(1, 0.5f);
        }

        public static void AnchorLeftBottom(this RectTransform transform)
        {
            transform.anchorMax = new(0, 0);
            transform.anchorMin = new(0, 0);
        }

        public static void AnchorBottom(this RectTransform transform)
        {
            transform.anchorMax = new(0.5f, 0);
            transform.anchorMin = new(0.5f, 0);
        }

        public static void AnchorRightBottom(this RectTransform transform)
        {
            transform.anchorMax = new(1, 0);
            transform.anchorMin = new(1, 0);
        }

        /// <summary>
        /// 在根节点下点显示cell
        /// </summary>
        public static void RefreshCell(this Transform cellParentRoot, Transform cellTemplate, int dataCount, Func<Transform, int, bool> onCellUpdate)
        {
            if (cellTemplate == null || cellParentRoot == null)
            {
                return;
            }

            cellTemplate.gameObject.SetActive(false);
            for (int i = 0; i < dataCount; i++)
            {
                Transform cell = null;
                cell = i >= cellParentRoot.childCount
                           ? Object.Instantiate(cellTemplate, cellParentRoot, false)
                           : cellParentRoot.GetChild(i);
                cell.gameObject.SetActive(true);
                if (onCellUpdate != null)
                {
                    //增加一个返回值，如果更新时出现错误，就直接隐藏掉
                    bool ret = onCellUpdate.Invoke(cell, i);
                    if (!ret)
                    {
                        cell.gameObject.SetActive(false);
                    }
                }
            }

            // 隐藏未使用
            for (int i = dataCount; i < cellParentRoot.childCount; i++)
            {
                cellParentRoot.GetChild(i).gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 在根节点下点显示cell
        /// </summary>
        public static void RefreshCell(this Transform cellParentRoot, Transform cellTemplate, int dataCount, Action<Transform, int> onCellUpdate)
        {
            if (cellTemplate == null || cellParentRoot == null)
            {
                return;
            }

            cellTemplate.gameObject.SetActive(false);

            for (int i = 0; i < dataCount; i++)
            {
                Transform cell = null;
                cell = i >= cellParentRoot.childCount
                           ? Object.Instantiate(cellTemplate, cellParentRoot, false)
                           : cellParentRoot.GetChild(i);
                cell.gameObject.SetActive(true);
                onCellUpdate?.Invoke(cell, i);
            }

            // 隐藏未使用
            for (int i = dataCount; i < cellParentRoot.childCount; i++)
            {
                cellParentRoot.GetChild(i).gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 在节点点显示cell
        /// </summary>
        public static void RefreshCell<T>(this Transform cellParentRoot, Transform cellTemplate, List<T> dataList, Action<Transform, int, List<T>> onCellUpdate)
        {
            if (cellTemplate == null || cellParentRoot == null)
            {
                return;
            }

            cellTemplate.gameObject.SetActive(false);
            int dataCount = dataList?.Count ?? 0;

            for (int i = 0; i < dataCount; i++)
            {
                Transform cell = null;
                cell = i >= cellParentRoot.childCount
                           ? Object.Instantiate(cellTemplate, cellParentRoot, false)
                           : cellParentRoot.GetChild(i);
                cell.gameObject.SetActive(true);
                onCellUpdate?.Invoke(cell, i, dataList);
            }

            // 隐藏未使用
            for (int i = dataCount; i < cellParentRoot.childCount; i++)
            {
                cellParentRoot.GetChild(i).gameObject.SetActive(false);
            }
        }

        public static TComponent GetOrAddComponent<TComponent>(this GameObject gameObject, bool includeChildren = false) where TComponent : Component
        {
            TComponent component = null;
            if (includeChildren)
            {
                component = gameObject.GetComponentInChildren<TComponent>();
                if (component == null)
                {
                    component = gameObject.AddComponent<TComponent>();
                }

                return component;
            }

            if (gameObject.TryGetComponent(out component))
            {
                return component;
            }

            component = gameObject.AddComponent<TComponent>();
            return component;
        }



        public static TValue GetOrRegisterValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, bool addIfNull = false)
            where TKey : Component
            where TValue : Component
        {
            if (key == null)
            {
                return null;
            }

            bool found = dictionary.TryGetValue(key, out TValue value);
            if (found)
            {
                return value;
            }

            value = addIfNull ? key.gameObject.GetOrAddComponent<TValue>() : key.GetComponent<TValue>();
            if (value != null)
            {
                dictionary.Add(key, value);
            }

            return value;
        }

        /// <summary>
        /// Gets a "target" component within a particular branch (inside the hierarchy). The branch is defined by the "branch root object", which is also defined by the chosen 
        /// "branch root component". The returned component must come from a child of the "branch root object".
        /// </summary>
        /// <param name="callerComponent"></param>
        /// <param name="includeInactive">Include inactive objects?</param>
        /// <typeparam name="T1">Branch root component type.</typeparam>
        /// <typeparam name="T2">Target component type.</typeparam>
        /// <returns>The target component.</returns>
        public static T2 GetComponentInBranch<T1, T2>(this Component callerComponent, bool includeInactive = true) where T1 : Component
                                                                                                                   where T2 : Component
        {
            T1[] rootComponents = callerComponent.transform.root.GetComponentsInChildren<T1>(includeInactive);

            if (rootComponents.Length == 0)
            {
                Debug.LogWarning($"Root component: No objects found with {typeof(T1).Name} component");
                return null;
            }

            for (int i = 0; i < rootComponents.Length; i++)
            {
                T1 rootComponent = rootComponents[i];

                // Is the caller a child of this root?
                if (!callerComponent.transform.IsChildOf(rootComponent.transform) && !rootComponent.transform.IsChildOf(callerComponent.transform))
                    continue;

                T2 targetComponent = rootComponent.GetComponentInChildren<T2>(includeInactive);

                if (targetComponent == null)
                    continue;

                return targetComponent;
            }

            return null;
        }

        /// <summary>
        /// Gets a "target" component within a particular branch (inside the hierarchy). The branch is defined by the "branch root object", which is also defined by the chosen 
        /// "branch root component". The returned component must come from a child of the "branch root object".
        /// </summary>
        /// <param name="callerComponent"></param>
        /// <param name="includeInactive">Include inactive objects?</param>
        /// <typeparam name="T1">Target component type.</typeparam>	
        /// <returns>The target component.</returns>
        public static T1 GetComponentInBranch<T1>(this Component callerComponent, bool includeInactive = true) where T1 : Component
        {
            return callerComponent.GetComponentInBranch<T1, T1>(includeInactive);
        }
    }
}
