using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace UniverseEngine.Editor
{
	public class ShaderModule : UniverseEditorModule<ShaderVariantCollectorSetting>
	{
		enum ESteps
		{
			None,
			Prepare,
			CollectAllMaterial,
			CollectVariants,
			CollectSleeping,
			WaitingDone,
		}

		const float WAIT_MILLISECONDS = 1000f;
		const float SLEEP_MILLISECONDS = 100f;
		static string s_SavePath;
		static string s_PackageName;
		static int s_ProcessMaxNum;
		static Action s_CompletedCallback;

		static Scene s_TempScene;
		static ESteps s_Steps = ESteps.None;
		static ValueStopwatch s_ElapsedTime;
		static List<string> s_AllMaterials;
		static readonly List<GameObject> s_AllSpheres = new(1000);

		/// <summary>
		/// 开始收集
		/// </summary>
		public static void StartCollect(string savePath, string packageName, int processMaxNum, Action completedCallback)
		{
			if (s_Steps != ESteps.None)
				return;

			if (!Path.HasExtension(savePath))
			{
				savePath = FileSystem.AppendExtension(savePath, "shadervariants");
			}

			if (Path.GetExtension(savePath) != ".shadervariants")
			{
				throw new("Shader variant file extension is invalid.");
			}
			if (string.IsNullOrEmpty(packageName))
			{
				throw new("Package name is null or empty !");
			}

			// 注意：先删除再保存，否则ShaderVariantCollection内容将无法及时刷新
			AssetDatabase.DeleteAsset(savePath);
			FileSystem.CreateFileDirectory(savePath);
			s_SavePath = savePath;
			s_PackageName = packageName;
			s_ProcessMaxNum = processMaxNum;
			s_CompletedCallback = completedCallback;

			// 聚焦到游戏窗口
			WindowModule.FocusUnityGameWindow();

			// 创建临时测试场景
			s_TempScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);

			s_Steps = ESteps.Prepare;
		}

		protected override void OnUpdate()
		{
			if (s_Steps == ESteps.None)
				return;

			if (s_Steps == ESteps.Prepare)
			{
				ClearCurrentShaderVariantCollection();
				s_Steps = ESteps.CollectAllMaterial;
				return;
			}

			if (s_Steps == ESteps.CollectAllMaterial)
			{
				s_AllMaterials = GetAllMaterials();
				s_Steps = ESteps.CollectVariants;
				return;
			}

			if (s_Steps == ESteps.CollectVariants)
			{
				int count = Mathf.Min(s_ProcessMaxNum, s_AllMaterials.Count);
				List<string> range = s_AllMaterials.GetRange(0, count);
				s_AllMaterials.RemoveRange(0, count);
				CollectVariants(range);

				if (s_AllMaterials.Count > 0)
				{
					s_ElapsedTime = ValueStopwatch.StartNew();
					s_Steps = ESteps.CollectSleeping;
				}
				else
				{
					s_ElapsedTime = ValueStopwatch.StartNew();
					s_Steps = ESteps.WaitingDone;
				}
			}

			if (s_Steps == ESteps.CollectSleeping)
			{
				if (s_ElapsedTime.TotalMilliSeconds > SLEEP_MILLISECONDS)
				{
					DestroyAllSpheres();
					s_ElapsedTime = ValueStopwatch.StartNew();
					s_Steps = ESteps.CollectVariants;
				}
			}

			if (s_Steps == ESteps.WaitingDone)
			{
				// 注意：一定要延迟保存才会起效
				if (s_ElapsedTime.TotalMilliSeconds > WAIT_MILLISECONDS)
				{
					s_ElapsedTime = ValueStopwatch.StartNew();
					s_Steps = ESteps.None;

					// 保存结果并创建清单
					SaveCurrentShaderVariantCollection(s_SavePath);
					CreateManifest();

					EditorLog.Info("搜集SVC完毕！");
					s_Steps = ESteps.None;

					SceneManager.UnloadSceneAsync(s_TempScene);

					s_CompletedCallback?.Invoke();
				}
			}
		}

		static List<string> GetAllMaterials()
		{
			List<string> allAssets = new(1000);

			// 获取所有打包的资源
			CollectResult collectResult = AssetCollectModule.Setting.GetPackageAssets(EBuildMode.DryRunBuild, s_PackageName);
			foreach (CollectAssetInfo assetInfo in collectResult.CollectAssets)
			{
				string[] depends = AssetDatabase.GetDependencies(assetInfo.AssetPath, true);
				foreach (string dependAsset in depends)
				{
					if (!allAssets.Contains(dependAsset))
					{
						allAssets.Add(dependAsset);
					}
				}
			}

			// 搜集所有材质球
			List<string> allMaterial = new(1000);
			foreach (string assetPath in allAssets)
			{
				Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
				if (assetType == typeof(Material))
				{
					allMaterial.Add(assetPath);
				}
			}

			// 返回结果
			return allMaterial;
		}

		static void CollectVariants(List<string> materials)
		{
			Camera camera = Camera.main;
			if (camera == null)
			{
				throw new("Not found main camera.");
			}

			// 设置主相机
			float aspect = camera.aspect;
			int totalMaterials = materials.Count;
			float height = Mathf.Sqrt(totalMaterials / aspect) + 1;
			float width = Mathf.Sqrt(totalMaterials / aspect) * aspect + 1;
			float halfHeight = Mathf.CeilToInt(height / 2f);
			float halfWidth = Mathf.CeilToInt(width / 2f);
			camera.orthographic = true;
			camera.orthographicSize = halfHeight;
			camera.transform.position = new(0f, 0f, -10f);

			// 创建测试球体
			int xMax = (int)(width - 1);
			int x = 0, y = 0;
			for (int i = 0; i < materials.Count; i++)
			{
				string material = materials[i];
				Vector3 position = new(x - halfWidth + 1f, y - halfHeight + 1f, 0f);
				GameObject go = CreateSphere(material, position, i);
				if (go != null)
				{
					s_AllSpheres.Add(go);
				}

				if (x == xMax)
				{
					x = 0;
					y++;
				}
				else
				{
					x++;
				}
			}
		}

		static GameObject CreateSphere(string assetPath, Vector3 position, int index)
		{
			Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
			Shader shader = material.shader;
			if (shader == null)
				return null;

			GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			go.GetComponent<Renderer>().sharedMaterial = material;
			go.transform.position = position;
			go.name = $"Sphere_{index} | {material.name}";
			return go;
		}

		static void DestroyAllSpheres()
		{
			foreach (GameObject go in s_AllSpheres)
			{
				Object.DestroyImmediate(go);
			}
			s_AllSpheres.Clear();

			// 尝试释放编辑器加载的资源
			EditorUtility.UnloadUnusedAssetsImmediate(true);
		}

		static void CreateManifest()
		{
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			ShaderVariantCollection svc = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(s_SavePath);
			if (svc != null)
			{
				ShaderVariantCollectionManifest wrapper = ShaderVariantCollectionManifest.Extract(svc);
				string jsonData = JsonUtility.ToJson(wrapper, true);
				string savePath = s_SavePath.Replace(".shadervariants", ".json");
				File.WriteAllText(savePath, jsonData);
			}

			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
		}

		public static void ClearCurrentShaderVariantCollection()
		{
			ReflectionUtilities.NonPublicStatic<ShaderUtil>("ClearCurrentShaderVariantCollection");
		}

		public static void SaveCurrentShaderVariantCollection(string savePath)
		{
			ReflectionUtilities.NonPublicStatic<ShaderUtil>("SaveCurrentShaderVariantCollection", savePath);
		}

		public static int GetCurrentShaderVariantCollectionShaderCount()
		{
			return ReflectionUtilities.NonPublicStatic<ShaderUtil, int>("GetCurrentShaderVariantCollectionShaderCount");
		}

		public static int GetCurrentShaderVariantCollectionVariantCount()
		{
			return ReflectionUtilities.NonPublicStatic<ShaderUtil, int>("GetCurrentShaderVariantCollectionVariantCount");
		}

		/// <summary>
		/// 获取着色器的变种总数量
		/// </summary>
		public static string GetShaderVariantCount(string assetPath)
		{
			Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(assetPath);
			string variantCount = ReflectionUtilities.NonPublicStatic<ShaderUtil, string>("GetVariantCount", shader, true);
			return variantCount;
		}
	}
}
