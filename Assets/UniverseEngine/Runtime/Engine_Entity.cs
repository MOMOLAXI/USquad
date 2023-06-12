using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniverseEngine
{
    public static partial class UniverseEngine
    {
        /// <summary>
        /// 当前场景对象
        /// </summary>
        public static EntityID CurrentScene => SceneSystem.CurrentScene;

        /// <summary>
        /// 设置场景资源包名称，在加载之前调用，否则场景系统无法找到场景资源
        /// </summary>
        /// <param name="scenePackageName"></param>
        public static void InitializeSceneSystem(string scenePackageName)
        {
            if (string.IsNullOrEmpty(scenePackageName))
            {
                Log.Error("Scene Package name is not valid (null or empty)");
                return;
            }

            SceneSystem.ScenePackageName = scenePackageName;
        }

        /// <summary>
        /// 创建场景
        /// </summary>
        /// <param name="sceneName"></param>
        public static async UniTask<bool> CreateScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Log.Error("Scene name is not valid (null or empty) , can not create");
                return false;
            }

            return await SceneSystem.CreateScene(sceneName);
        }

        /// <summary>
        /// 创建Entity
        /// </summary>
        /// <param name="className"></param>
        /// <param name="parentID"></param>
        /// <param name="identity"></param>
        /// <param name="isStatic"></param>
        /// <returns></returns>
        public static EntityID CreateEntity(string className, EntityID parentID, int identity = 0, bool isStatic = false)
        {
            if (parentID == EntityID.Invalid)
            {
                Log.Error("Entity id is not valid");
                return EntityID.Invalid;
            }

            Entity parent = EngineSystem<EntitySystem>.System.Find(parentID);
            Entity entity = EngineSystem<EntitySystem>.System.CreateEntity(className, identity, parent, isStatic);
            return entity.ID;
        }

        /// <summary>
        /// 创建Entity
        /// </summary>
        /// <param name="className"></param>
        /// <param name="identity"></param>
        /// <param name="isStatic"></param>
        /// <returns></returns>
        public static EntityID CreateEntity(string className, int identity = 0, bool isStatic = false)
        {
            Entity entity = EngineSystem<EntitySystem>.System.CreateEntity(className, identity, null, isStatic);
            return entity.ID;
        }

        /// <summary>
        /// 删除Entity
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="immediate"></param>
        public static void Destroy(this EntityID entityID, bool immediate = false)
        {
            if (entityID == EntityID.Invalid)
            {
                Log.Error("Entity id is not valid");
                return;
            }

            EngineSystem<EntitySystem>.System.DestroyEntity(entityID, immediate);
        }

        /// <summary>
        /// 获取游戏对象
        /// </summary>
        /// <param name="entityID"></param>
        /// <returns></returns>
        public static GameObject GetEntityGameObject(this EntityID entityID)
        {
            if (entityID == EntityID.Invalid)
            {
                Log.Error("Entity id is not valid");
                return null;
            }

            return entityID.GetEntity()
                           .gameObject;
        }

        /// <summary>
        /// Entity添加Mono组件
        /// </summary>
        /// <param name="entityID"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T AddComponent<T>(this EntityID entityID) where T : Component
        {
            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.AddComponent<T>();
        }

        /// <summary>
        /// Entity添加逻辑组件
        /// </summary>
        /// <param name="entityID"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T AddEntityComponent<T>(this EntityID entityID) where T : EntityComponent, new()
        {
            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.AddEntityComponent<T>();
        }

        /// <summary>
        /// Entity添加逻辑组件
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static EntityComponent AddEntityComponent(this EntityID entityID, Type type)
        {
            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.AddEntityComponent(type);
        }

        /// <summary>
        /// 获取逻辑组件
        /// </summary>
        /// <param name="entityID"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetEntityComponent<T>(this EntityID entityID) where T : EntityComponent
        {
            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.GetEntityComponent<T>();
        }

        /// <summary>
        /// 获取Mono组件
        /// </summary>
        /// <param name="entityID"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetComponent<T>(this EntityID entityID) where T : Component
        {
            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.GetComponent<T>();
        }

        /// <summary>
        /// 移除Entity逻辑组件
        /// </summary>
        /// <param name="entityID"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool RemoveEntityComponent<T>(this EntityID entityID) where T : EntityComponent
        {
            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return false;
            }

            return entity.RemoveEntityComponent<T>();
        }

        /// <summary>
        /// 移除Entity逻辑组件
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool RemoveEntityComponent(this EntityID entityID, Type type)
        {
            if (type == null)
            {
                Log.Error("can not remove null type");
                return false;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return false;
            }

            return entity.RemoveEntityComponent(type);
        }

        /// <summary>
        /// 移除Entity逻辑组件
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="component"></param>
        /// <returns></returns>
        public static bool RemoveEntityComponent(this EntityID entityID, EntityComponent component)
        {
            if (component == null)
            {
                Log.Error("can not remove null type");
                return false;
            }

            Entity id = GetEntity(entityID);
            if (id == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return false;
            }

            return id.RemoveEntityComponent(component);
        }

        public static Vector3 GetPosition(this EntityID id)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.Position;
        }

        public static void SetPotision(this EntityID id, Vector3 position)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Position = position;
        }

        public static Vector3 GetLocalPosition(this EntityID id)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.LocalPosition;
        }

        public static void SetLocalPosition(this EntityID id, Vector3 position)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.LocalPosition = position;
        }

        public static Vector3 GetEulerAngles(this EntityID id)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.EulerAngles;
        }

        public static void SetEulerAngles(this EntityID id, Vector3 eulers)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.EulerAngles = eulers;
        }

        public static Vector3 GetLocalEulerAngles(this EntityID id)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.LocalEulerAngles;
        }

        public static void SetLocalEulerAngles(this EntityID id, Vector3 eulers)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.LocalEulerAngles = eulers;
        }

        public static Vector3 GetRight(this EntityID id)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.Right;
        }

        public static void SetRight(this EntityID id, Vector3 right)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Right = right;
        }

        public static Vector3 GetUp(this EntityID id)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.Up;
        }

        public static void SetUp(this EntityID id, Vector3 up)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Up = up;
        }

        public static Vector3 GetForward(this EntityID id)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.Forward;
        }

        public static void SetForward(this EntityID id, Vector3 forward)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Forward = forward;
        }

        public static Quaternion GetRotation(this EntityID id)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Quaternion.identity;
            }

            return entity.Rotation;
        }

        public static void SetRotation(this EntityID id, Quaternion rotation)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Rotation = rotation;
        }

        public static Quaternion GetLocalRotation(this EntityID id)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Quaternion.identity;
            }

            return entity.LocalRotation;
        }

        public static void SetLocalRotation(this EntityID id, Quaternion localRotation)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.LocalRotation = localRotation;
        }

        public static Vector3 GetLocalScale(this EntityID id)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.LocalScale;
        }

        public static void SetLocalScale(this EntityID id, Vector3 scale)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.LocalScale = scale;
        }

        public static Matrix4x4 GetWorldToLocalMatrix(this EntityID id)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Matrix4x4.zero;
            }

            return entity.WorldToLocalMatrix;
        }


        public static Matrix4x4 GetLocalToWorldMatrix(this EntityID id)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Matrix4x4.zero;
            }

            return entity.LocalToWorldMatrix;
        }

        public static void Translate(this EntityID id, Vector3 translation, Space relativeTo)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Translate(translation, relativeTo);
        }

        public static void Translate(this EntityID id, Vector3 translation)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Translate(translation);
        }

        public static void Translate(this EntityID id, float x, float y, float z, Space relativeTo)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Translate(x, y, z, relativeTo);
        }

        public static void Translate(this EntityID id, float x, float y, float z)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }


            entity.Translate(x, y, z);
        }

        public static void Translate(this EntityID id, Vector3 translation, Transform relativeTo)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }


            entity.Translate(translation, relativeTo);
        }

        public static void Translate(this EntityID id, float x, float y, float z, Transform relativeTo)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }


            entity.Translate(x, y, z, relativeTo);
        }

        public static void Rotate(this EntityID id, Vector3 eulers, Space relativeTo)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }


            entity.Rotate(eulers, relativeTo);
        }

        public static void Rotate(this EntityID id, Vector3 eulers)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Rotate(eulers);
        }

        public static void Rotate(this EntityID id, float xAngle, float yAngle, float zAngle, Space relativeTo)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Rotate(xAngle, yAngle, zAngle, relativeTo);
        }

        public static void Rotate(this EntityID id, float xAngle, float yAngle, float zAngle)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Rotate(xAngle, yAngle, zAngle);
        }

        public static void Rotate(this EntityID id, Vector3 axis, float angle, Space relativeTo)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Rotate(axis, angle, relativeTo);
        }

        public static void Rotate(this EntityID id, Vector3 axis, float angle)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.Rotate(axis, angle);
        }

        public static void RotateAround(this EntityID id, Vector3 point, Vector3 axis, float angle)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.RotateAround(point, axis, angle);
        }

        public static void LookAt(this EntityID id, Transform target, Vector3 worldUp)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.LookAt(target, worldUp);
        }

        public static void LookAt(this EntityID id, Transform target)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.LookAt(target);
        }

        public static void LookAt(this EntityID id, Vector3 worldPosition, Vector3 worldUp)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.LookAt(worldPosition, worldUp);
        }

        public static void LookAt(this EntityID id, Vector3 worldPosition)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return;
            }

            entity.LookAt(worldPosition);
        }

        public static Vector3 TransformDirection(this EntityID id, Vector3 direction)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.TransformDirection(direction);
        }

        public static Vector3 TransformDirection(this EntityID id, float x, float y, float z)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }


            return entity.TransformDirection(x, y, z);
        }

        public static Vector3 InverseTransformDirection(this EntityID id, Vector3 direction)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.InverseTransformDirection(direction);
        }

        public static Vector3 InverseTransformDirection(this EntityID id, float x, float y, float z)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.InverseTransformDirection(x, y, z);
        }

        public static Vector3 TransformVector(this EntityID id, Vector3 vector)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.TransformVector(vector);
        }

        public static Vector3 TransformyVector(this EntityID id, float x, float y, float z)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.TransformVector(x, y, z);
        }

        public static Vector3 InverseTransformVector(this EntityID id, Vector3 vector)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.InverseTransformVector(vector);
        }

        public static Vector3 InverseTransformVector(this EntityID id, float x, float y, float z)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.InverseTransformVector(x, y, z);
        }

        public static Vector3 TransformPoint(this EntityID id, Vector3 position)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.TransformPoint(position);
        }

        public static Vector3 TransformPoint(this EntityID id, float x, float y, float z)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.TransformPoint(x, y, z);
        }

        public static Vector3 InverseTransformPoint(this EntityID id, Vector3 position)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.InverseTransformPoint(position);
        }

        public static Vector3 InverseTransformPoint(this EntityID id, float x, float y, float z)
        {
            Entity entity = GetEntity(id);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {id.ToString()}");
                return Vector3.zero;
            }

            return entity.InverseTransformPoint(x, y, z);
        }

        /// <summary>
        /// 索引获取子Entity
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static EntityID GetChildAtIndex(this EntityID entityID, int index)
        {
            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                return default;
            }

            return entity.GetChildAtIndex(index);
        }

        /// <summary>
        /// 获取子对象
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="result"></param>
        public static void GetChildren(this EntityID entityID, List<EntityID> result)
        {
            if (result == null)
            {
                Log.Error("input list is null");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.GetChildren(result);
        }

        /// <summary>
        /// 注册类事件Hook
        /// </summary>
        /// <param name="className"></param>
        /// <param name="eventType"></param>
        /// <param name="callBack"></param>
        public static void RegisterClassEventHook(string className, EntityEvent eventType, EntityEventCallBack callBack)
        {
            if (string.IsNullOrEmpty(className))
            {
                Log.Error($"class name can not be null while {nameof(RegisterClassEventHook)}");
                return;
            }

            if (callBack == null)
            {
                Log.Error("callback is null");
                return;
            }


            EntityClassEventContext context = EngineSystem<EntitySystem>.System.EventLibrary.Get(className);
            context?.RegisterEvent(eventType, callBack);
        }

        /// <summary>
        /// 添加属性变化回调
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="callbackName"></param>
        /// <param name="function"></param>
        public static void AddPropertyHook(
            this EntityID entityID,
            string propName,
            string callbackName,
            PropChangeFunction function)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("[AddPropHook]property name must not be null or empty");
                return;
            }

            if (string.IsNullOrEmpty(callbackName))
            {
                Log.Error("[AddPropHook]callback name must not be null or empty");
                return;
            }

            if (function == null)
            {
                Log.Error("[AddPropHook] function is null");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.AddPropHook(propName, callbackName, function);
        }

        /// <summary>
        /// 移除属性变化回调
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="callbackName"></param>
        public static void RemovePropertyHook(this EntityID entityID, string propName, string callbackName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("[RemovePropHook]property name must not be null or empty");
                return;
            }

            if (string.IsNullOrEmpty(callbackName))
            {
                Log.Error("[RemovePropHook]callback name must not be null or empty");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.RemovePropHook(propName, callbackName);
        }

        /// <summary>
        /// 获取所有离散属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="variableList"></param>
        /// <returns></returns>
        public static bool GetAllProperties(this EntityID entityID, List<EntityProperty> variableList)
        {
            if (variableList == null)
            {
                return false;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return false;
            }

            return entity.GetPropList(variableList);
        }

        /// <summary>
        /// Entity是否有该属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static bool HasProp(this EntityID entityID, string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return false;
            }

            return entity.HasProp(propName);
        }

        /// <summary>
        /// 获取Entity属性的数据类型
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static VarType GetVariableType(this EntityID entityID, string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return VarType.None;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return VarType.None;
            }

            return entity.GetVariableType(propName);
        }

        /// <summary>
        /// 获取Entity的bool属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static bool GetBool(this EntityID entityID, string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.GetBool(propName);
        }

        public static string GetClassName(this EntityID entityID)
        {
            if (entityID == EntityID.Invalid)
            {
                return string.Empty;
            }

            Entity entity = EngineSystem<EntitySystem>.System.Find(entityID);
            if (entity == null)
            {
                Log.Error($"Can not find Entity with ID : {entityID.ToString()}");
                return string.Empty;
            }

            return entity.ClassName;
        }

        /// <summary>
        /// 获取Entity的Int属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static int GetInt(this EntityID entityID, string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.GetInt(propName);
        }

        /// <summary>
        /// 获取Entity的long属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static long GetInt64(this EntityID entityID, string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.GetInt64(propName);
        }

        /// <summary>
        /// 获取Entity的float属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static float GetFloat(this EntityID entityID, string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.GetFloat(propName);
        }

        /// <summary>
        /// 获取Entity的double属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static double GetDouble(this EntityID entityID, string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.GetDouble(propName);
        }

        /// <summary>
        /// 获取角色身上的字符串属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static string GetString(this EntityID entityID, string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.GetString(propName);
        }

        /// <summary>
        /// 获取Entity的EntityID属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static EntityID GetEntityID(this EntityID entityID, string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return EntityID.Invalid;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return EntityID.Invalid;
            }

            return entity.GetEntityID(propName);
        }

        /// <summary>
        /// 获取Entity的二进制属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static byte[] GetBinary(this EntityID entityID, string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return null;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return null;
            }

            return entity.GetBinary(propName);
        }

        /// <summary>
        /// 获取Entity的对象属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static object GetObject(this EntityID entityID, string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.GetObject(propName);
        }

        /// <summary>
        /// 获取Entity的对象属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static T GetClass<T>(this EntityID entityID, string propName) where T : class
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.GetClass<T>(propName);
        }

        /// <summary>
        /// 获取Entity的属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static Var GetProp(this EntityID entityID, string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return default;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity.GetProp(propName);
        }

        /// <summary>
        /// 设置Entity的属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetProp(this EntityID entityID, string propName, Var value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.SetProp(propName, value);
        }

        /// <summary>
        /// 设置Entity的bool属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetBool(this EntityID entityID, string propName, bool value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.SetBool(propName, value);
        }

        /// <summary>
        /// 设置Entity的int属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetInt(this EntityID entityID, string propName, int value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.SetInt(propName, value);
        }

        /// <summary>
        /// 设置Entity的long属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetInt64(this EntityID entityID, string propName, long value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.SetInt64(propName, value);
        }

        /// <summary>
        /// 设置Entity的float属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetFloat(this EntityID entityID, string propName, float value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.SetFloat(propName, value);
        }

        /// <summary>
        /// 设置Entity的double属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetDouble(this EntityID entityID, string propName, double value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.SetDouble(propName, value);
        }

        /// <summary>
        /// 设置Entity的string属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetString(this EntityID entityID, string propName, string value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.SetString(propName, value);
        }

        /// <summary>
        /// 设置Entity的EntityID属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetEntityID(this EntityID entityID, string propName, EntityID value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.SetEntityID(propName, value);
        }

        /// <summary>
        /// 设置Entity的二进制属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetBinary(this EntityID entityID, string propName, byte[] value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.SetBinary(propName, value);
        }

        /// <summary>
        /// 设置Entity的对象属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetObject(this EntityID entityID, string propName, object value)
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.SetObject(propName, value);
        }

        /// <summary>
        /// 设置Entity的对象属性
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="propName"></param>
        /// <param name="value"></param>
        public static void SetClass<T>(this EntityID entityID, string propName, T value) where T : class
        {
            if (string.IsNullOrEmpty(propName))
            {
                Log.Error("property name must not be null or empty");
                return;
            }

            Entity entity = GetEntity(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return;
            }

            entity.SetClass(propName, value);
        }

        /// <summary>
        /// 获取Entity
        /// </summary>
        /// <param name="entityID"></param>
        /// <returns></returns>
        static Entity GetEntity(this EntityID entityID)
        {
            if (entityID == EntityID.Invalid)
            {
                Log.Error("Entity id is not valid");
                return default;
            }

            Entity entity = EngineSystem<EntitySystem>.System.Find(entityID);
            if (entity == null)
            {
                Log.Error($"Entity not exist with ID {entityID.ToString()}");
                return default;
            }

            return entity;
        }
    }
}