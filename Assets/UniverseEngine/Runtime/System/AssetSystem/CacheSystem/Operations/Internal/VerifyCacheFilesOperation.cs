﻿// using System;
// using System.Collections.Generic;
// using System.Threading;
//
// namespace UniverseEngine
// {
// 	internal abstract class VerifyCacheFilesOperation : AsyncOperationBase
// 	{
// 		public static VerifyCacheFilesOperation CreateOperation(List<VerifyCacheElement> elements)
// 		{
// #if UNITY_WEBGL
// 			var operation = new VerifyCacheFilesWithoutThreadOperation(elements);
// #else
// 			VerifyCacheFilesWithThreadOperation operation = new VerifyCacheFilesWithThreadOperation(elements);
// #endif
// 			return operation;
// 		}
// 	}
//
// 	/// <summary>
// 	/// 本地缓存文件验证（线程版）
// 	/// </summary>
// 	internal class VerifyCacheFilesWithThreadOperation : VerifyCacheFilesOperation
// 	{
// 		private enum ESteps
// 		{
// 			None,
// 			InitVerify,
// 			UpdateVerify,
// 			Done,
// 		}
//
// 		private readonly ThreadSyncContext _syncContext = new();
// 		private List<VerifyCacheElement> _waitingList;
// 		private List<VerifyCacheElement> _verifyingList;
// 		private int _verifyMaxNum;
// 		private int _verifyTotalCount;
// 		private float _verifyStartTime;
// 		private int _succeedCount;
// 		private int _failedCount;
// 		private ESteps _steps = ESteps.None;
//
// 		public VerifyCacheFilesWithThreadOperation(List<VerifyCacheElement> elements)
// 		{
// 			_waitingList = elements;
// 		}
// 		internal override void Start()
// 		{
// 			_steps = ESteps.InitVerify;
// 			_verifyStartTime = UnityEngine.Time.realtimeSinceStartup;
// 		}
// 		internal override void Update()
// 		{
// 			if (_steps == ESteps.None || _steps == ESteps.Done)
// 				return;
//
// 			if (_steps == ESteps.InitVerify)
// 			{
// 				int fileCount = _waitingList.Count;
//
// 				// 设置同时验证的最大数
// 				ThreadPool.GetMaxThreads(out int workerThreads, out int ioThreads);
// 				Log<AssetSystem>.Info($"Work threads : {workerThreads}, IO threads : {ioThreads}");
// 				_verifyMaxNum = Math.Min(workerThreads, ioThreads);
// 				_verifyTotalCount = fileCount;
// 				if (_verifyMaxNum < 1)
// 					_verifyMaxNum = 1;
//
// 				_verifyingList = new(_verifyMaxNum);
// 				_steps = ESteps.UpdateVerify;
// 			}
//
// 			if (_steps == ESteps.UpdateVerify)
// 			{
// 				_syncContext.Update();
//
// 				Progress = GetProgress();
// 				if (_waitingList.Count == 0 && _verifyingList.Count == 0)
// 				{
// 					_steps = ESteps.Done;
// 					Status = EOperationStatus.Succeed;
// 					float costTime = UnityEngine.Time.realtimeSinceStartup - _verifyStartTime;
// 					Log<AssetSystem>.Info($"Verify cache files elapsed time {costTime:f1} seconds");
// 				}
//
// 				for (int i = _waitingList.Count - 1; i >= 0; i--)
// 				{
// 					if (OperationSystem.IsBusy)
// 						break;
//
// 					if (_verifyingList.Count >= _verifyMaxNum)
// 						break;
//
// 					VerifyCacheElement element = _waitingList[i];
// 					if (BeginVerifyFileWithThread(element))
// 					{
// 						_waitingList.RemoveAt(i);
// 						_verifyingList.Add(element);
// 					}
// 					else
// 					{
// 						Log<AssetSystem>.Warning("The thread pool is failed queued.");
// 						break;
// 					}
// 				}
// 			}
// 		}
//
// 		private float GetProgress()
// 		{
// 			if (_verifyTotalCount == 0)
// 				return 1f;
// 			return (float)(_succeedCount + _failedCount) / _verifyTotalCount;
// 		}
// 		private bool BeginVerifyFileWithThread(VerifyCacheElement element)
// 		{
// 			return ThreadPool.QueueUserWorkItem(new(VerifyInThread), element);
// 		}
// 		private void VerifyInThread(object obj)
// 		{
// 			VerifyCacheElement element = (VerifyCacheElement)obj;
// 			element.Result = CacheSystem.VerifyingCacheFile(element);
// 			_syncContext.Post(VerifyCallback, element);
// 		}
// 		private void VerifyCallback(object obj)
// 		{
// 			VerifyCacheElement element = (VerifyCacheElement)obj;
// 			_verifyingList.Remove(element);
//
// 			if (element.Result == EVerifyResult.Succeed)
// 			{
// 				_succeedCount++;
// 				PackageCache.RecordWrapper wrapper = new PackageCache.RecordWrapper(element.InfoFilePath, element.DataFilePath, element.DataFileCRC, element.DataFileSize);
// 				CacheSystem.RecordFile(element.PackageName, element.CacheGUID, wrapper);
// 			}
// 			else
// 			{
// 				_failedCount++;
//
// 				Log<AssetSystem>.Warning($"Failed verify file and delete files : {element.FileRootPath}");
// 				element.DeleteFiles();
// 			}
// 		}
// 	}
//
// 	/// <summary>
// 	/// 本地缓存文件验证（非线程版）
// 	/// </summary>
// 	internal class VerifyCacheFilesWithoutThreadOperation : VerifyCacheFilesOperation
// 	{
// 		private enum ESteps
// 		{
// 			None,
// 			InitVerify,
// 			UpdateVerify,
// 			Done,
// 		}
//
// 		private List<VerifyCacheElement> _waitingList;
// 		private List<VerifyCacheElement> _verifyingList;
// 		private int _verifyMaxNum;
// 		private int _verifyTotalCount;
// 		private float _verifyStartTime;
// 		private int _succeedCount;
// 		private int _failedCount;
// 		private ESteps _steps = ESteps.None;
// 		
// 		public VerifyCacheFilesWithoutThreadOperation(List<VerifyCacheElement> elements)
// 		{
// 			_waitingList = elements;
// 		}
// 		internal override void Start()
// 		{
// 			_steps = ESteps.InitVerify;
// 			_verifyStartTime = UnityEngine.Time.realtimeSinceStartup;
// 		}
// 		internal override void Update()
// 		{
// 			if (_steps == ESteps.None || _steps == ESteps.Done)
// 				return;
//
// 			if (_steps == ESteps.InitVerify)
// 			{
// 				int fileCount = _waitingList.Count;
//
// 				// 设置同时验证的最大数
// 				_verifyMaxNum = fileCount;
// 				_verifyTotalCount = fileCount;
//
// 				_verifyingList = new(_verifyMaxNum);
// 				_steps = ESteps.UpdateVerify;
// 			}
//
// 			if (_steps == ESteps.UpdateVerify)
// 			{
// 				Progress = GetProgress();
// 				if (_waitingList.Count == 0 && _verifyingList.Count == 0)
// 				{
// 					_steps = ESteps.Done;
// 					Status = EOperationStatus.Succeed;
// 					float costTime = UnityEngine.Time.realtimeSinceStartup - _verifyStartTime;
// 					Log<AssetSystem>.Info($"Package verify elapsed time {costTime:f1} seconds");
// 				}
//
// 				for (int i = _waitingList.Count - 1; i >= 0; i--)
// 				{
// 					if (OperationSystem.IsBusy)
// 						break;
//
// 					if (_verifyingList.Count >= _verifyMaxNum)
// 						break;
//
// 					VerifyCacheElement element = _waitingList[i];
// 					BeginVerifyFileWithoutThread(element);
// 					_waitingList.RemoveAt(i);
// 					_verifyingList.Add(element);
// 				}
//
// 				// 主线程内验证，可以清空列表
// 				_verifyingList.Clear();
// 			}
// 		}
//
// 		private float GetProgress()
// 		{
// 			if (_verifyTotalCount == 0)
// 				return 1f;
// 			return (float)(_succeedCount + _failedCount) / _verifyTotalCount;
// 		}
// 		private void BeginVerifyFileWithoutThread(VerifyCacheElement element)
// 		{
// 			element.Result = CacheSystem.VerifyingCacheFile(element);
// 			if (element.Result == EVerifyResult.Succeed)
// 			{
// 				_succeedCount++;
// 				PackageCache.RecordWrapper wrapper = new PackageCache.RecordWrapper(element.InfoFilePath, element.DataFilePath, element.DataFileCRC, element.DataFileSize);
// 				CacheSystem.RecordFile(element.PackageName, element.CacheGUID, wrapper);
// 			}
// 			else
// 			{
// 				_failedCount++;
//
// 				Log<AssetSystem>.Warning($"Failed verify file and delete files : {element.FileRootPath}");
// 				element.DeleteFiles();
// 			}
// 		}
// 	}
// }