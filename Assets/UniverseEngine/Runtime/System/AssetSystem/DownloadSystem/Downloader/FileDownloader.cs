using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace UniverseEngine
{
	internal sealed class FileDownloader : DownloaderBase
	{
		readonly bool m_BreakResume;
		readonly string m_TempFilePath;
		UnityWebRequest m_WebRequest = null;
		DownloadHandlerFileRange m_DownloadHandle = null;
		// VerifyTempFileOperation _checkFileOp = null;
		// VerifyTempFileOperation _verifyFileOp = null;

		// 重置变量
		bool m_IsAbort = false;
		ulong m_FileOriginLength;
		ulong m_LatestDownloadBytes;
		float m_LatestDownloadRealtime;
		float m_TryAgainTimer;
		EAssetFileCacheResult m_CheckResult;
		EAssetFileCacheResult m_VerifyResult;

		public FileDownloader(BundleInfo bundleInfo, bool breakResume) : base(bundleInfo)
		{
			m_BreakResume = breakResume;
			m_TempFilePath = bundleInfo.Bundle.TempDataFilePath;
		}

		public override void Update()
		{
			if (_steps == ESteps.None)
				return;
			if (IsDone())
				return;

			// 检测临时文件
			if (_steps == ESteps.CheckTempFile)
			{
				m_CheckResult = AssetCacheSystem.Verify(_bundleInfo.Bundle.TempDataFilePath, _bundleInfo.Bundle.FileSize, _bundleInfo.Bundle.FileCRC, EVerifyLevel.High);
				// VerifyTempElement element = new(_bundleInfo.Bundle.TempDataFilePath, _bundleInfo.Bundle.FileCRC, _bundleInfo.Bundle.FileSize);
				// _checkFileOp = VerifyTempFileOperation.CreateOperation(element);
				// OperationSystem.StartOperation(_checkFileOp);
				_steps = ESteps.WaitingCheckTempFile;
			}

			// 等待检测结果
			if (_steps == ESteps.WaitingCheckTempFile)
			{
				if (m_CheckResult == EAssetFileCacheResult.Succeed)
				{
					_steps = ESteps.CachingFile;
				}
				else
				{
					if (m_CheckResult == EAssetFileCacheResult.FileOverflow)
					{
						if (File.Exists(m_TempFilePath))
							File.Delete(m_TempFilePath);
					}
					_steps = ESteps.PrepareDownload;
				}
			}

			// 创建下载器
			if (_steps == ESteps.PrepareDownload)
			{
				// 重置变量
				_downloadProgress = 0f;
				_downloadedBytes = 0;
				m_IsAbort = false;
				m_FileOriginLength = 0;
				m_LatestDownloadBytes = 0;
				m_LatestDownloadRealtime = Time.realtimeSinceStartup;
				m_TryAgainTimer = 0f;

				// 获取请求地址
				_requestURL = GetRequestURL();

				if (m_BreakResume)
					_steps = ESteps.CreateResumeDownloader;
				else
					_steps = ESteps.CreateGeneralDownloader;
			}

			// 创建普通的下载器
			if (_steps == ESteps.CreateGeneralDownloader)
			{
				if (File.Exists(m_TempFilePath))
					File.Delete(m_TempFilePath);

				m_WebRequest = DownloadSystem.NewRequest(_requestURL);
				DownloadHandlerFile handler = new(m_TempFilePath);
				handler.removeFileOnAbort = true;
				m_WebRequest.downloadHandler = handler;
				m_WebRequest.disposeDownloadHandlerOnDispose = true;

				if (DownloadSystem.CertificateHandlerInstance != null)
				{
					m_WebRequest.certificateHandler = DownloadSystem.CertificateHandlerInstance;
					m_WebRequest.disposeCertificateHandlerOnDispose = false;
				}

				m_WebRequest.SendWebRequest();
				_steps = ESteps.CheckDownload;
			}

			// 创建断点续传下载器
			if (_steps == ESteps.CreateResumeDownloader)
			{
				long fileLength = -1;
				if (File.Exists(m_TempFilePath))
				{
					FileInfo fileInfo = new(m_TempFilePath);
					fileLength = fileInfo.Length;
					m_FileOriginLength = (ulong)fileLength;
					_downloadedBytes = m_FileOriginLength;
				}

			#if UNITY_2019_4_OR_NEWER
				m_WebRequest = DownloadSystem.NewRequest(_requestURL);
				DownloadHandlerFile handler = new DownloadHandlerFile(m_TempFilePath, true);
				handler.removeFileOnAbort = false;
			#else
				_webRequest = DownloadSystem.NewRequest(_requestURL);
				var handler = new DownloadHandlerFileRange(_tempFilePath, _bundleInfo.Bundle.FileSize, _webRequest);
				_downloadHandle = handler;
			#endif
				m_WebRequest.downloadHandler = handler;
				m_WebRequest.disposeDownloadHandlerOnDispose = true;
				if (fileLength > 0)
					m_WebRequest.SetRequestHeader("Range", $"bytes={fileLength}-");

				if (DownloadSystem.CertificateHandlerInstance != null)
				{
					m_WebRequest.certificateHandler = DownloadSystem.CertificateHandlerInstance;
					m_WebRequest.disposeCertificateHandlerOnDispose = false;
				}

				m_WebRequest.SendWebRequest();
				_steps = ESteps.CheckDownload;
			}

			// 检测下载结果
			if (_steps == ESteps.CheckDownload)
			{
				_downloadProgress = m_WebRequest.downloadProgress;
				_downloadedBytes = m_FileOriginLength + m_WebRequest.downloadedBytes;
				if (m_WebRequest.isDone == false)
				{
					CheckTimeout();
					return;
				}

				bool hasError = false;

				// 检查网络错误
				if (m_WebRequest.result != UnityWebRequest.Result.Success)
				{
					hasError = true;
					_lastError = m_WebRequest.error;
					_lastCode = m_WebRequest.responseCode;
				}

				// 如果网络异常
				if (hasError)
				{
					if (m_BreakResume)
					{
						// 注意：下载断点续传文件发生特殊错误码之后删除文件
						if (DownloadSystem.ClearFileResponseCodes != null)
						{
							if (DownloadSystem.ClearFileResponseCodes.Contains(m_WebRequest.responseCode))
							{
								if (File.Exists(m_TempFilePath))
									File.Delete(m_TempFilePath);
							}
						}
					}
					else
					{
						// 注意：非断点续传下载失败之后删除文件
						if (File.Exists(m_TempFilePath))
							File.Delete(m_TempFilePath);
					}

					_steps = ESteps.TryAgain;
				}
				else
				{
					_steps = ESteps.VerifyTempFile;
				}

				// 释放下载器
				DisposeWebRequest();
			}

			// 验证下载文件
			if (_steps == ESteps.VerifyTempFile)
			{
				m_VerifyResult = AssetCacheSystem.Verify(_bundleInfo.Bundle.TempDataFilePath, _bundleInfo.Bundle.FileSize, _bundleInfo.Bundle.FileCRC, EVerifyLevel.High);
				_steps = ESteps.WaitingVerifyTempFile;
			}

			// 等待验证完成
			if (_steps == ESteps.WaitingVerifyTempFile)
			{
				if (m_VerifyResult == EAssetFileCacheResult.Succeed)
				{
					_steps = ESteps.CachingFile;
				}
				else
				{
					if (File.Exists(m_TempFilePath))
						File.Delete(m_TempFilePath);

					_steps = ESteps.TryAgain;
				}
			}

			// 缓存下载文件
			if (_steps == ESteps.CachingFile)
			{
				try
				{
					string infoFilePath = _bundleInfo.Bundle.CachedInfoFilePath;
					string dataFilePath = _bundleInfo.Bundle.CachedDataFilePath;
					string dataFileCRC = _bundleInfo.Bundle.FileCRC;
					long dataFileSize = _bundleInfo.Bundle.FileSize;

					if (File.Exists(infoFilePath))
						File.Delete(infoFilePath);
					if (File.Exists(dataFilePath))
						File.Delete(dataFilePath);

					FileInfo fileInfo = new(m_TempFilePath);
					fileInfo.MoveTo(dataFilePath);

					FileCacheMeta fileMeta = new(infoFilePath, dataFilePath, dataFileCRC, dataFileSize);

					// 写入信息文件记录验证数据
					fileMeta.WriteInfoToFile(infoFilePath, dataFileCRC, dataFileSize);

					// 记录缓存文件
					AssetCacheSystem.CacheFile(_bundleInfo.Bundle.PackageName, _bundleInfo.Bundle.CacheGuid, fileMeta);

					_lastError = string.Empty;
					_lastCode = 0;
					_steps = ESteps.Succeed;
				}
				catch (Exception e)
				{
					_lastError = e.Message;
					_steps = ESteps.TryAgain;
				}
			}

			// 重新尝试下载
			if (_steps == ESteps.TryAgain)
			{
				if (_failedTryAgain <= 0)
				{
					ReportError();
					_steps = ESteps.Failed;
					return;
				}

				m_TryAgainTimer += Time.unscaledDeltaTime;
				if (m_TryAgainTimer > 1f)
				{
					_failedTryAgain--;
					_steps = ESteps.PrepareDownload;
					ReportWarning();
					Log<AssetSystem>.Warning($"Try again download : {_requestURL}");
				}
			}
		}
		public override void Abort()
		{
			if (IsDone() == false)
			{
				_steps = ESteps.Failed;
				_lastError = "user abort";
				_lastCode = 0;
				DisposeWebRequest();
			}
		}

		void CheckTimeout()
		{
			// 注意：在连续时间段内无新增下载数据及判定为超时
			if (m_IsAbort == false)
			{
				if (m_LatestDownloadBytes != DownloadedBytes)
				{
					m_LatestDownloadBytes = DownloadedBytes;
					m_LatestDownloadRealtime = Time.realtimeSinceStartup;
				}

				float offset = Time.realtimeSinceStartup - m_LatestDownloadRealtime;
				if (offset > _timeout)
				{
					Log<AssetSystem>.Warning($"Web file request timeout : {_requestURL}");
					m_WebRequest.Abort();
					m_IsAbort = true;
				}
			}
		}
		void DisposeWebRequest()
		{
			if (m_DownloadHandle != null)
			{
				m_DownloadHandle.Cleanup();
				m_DownloadHandle = null;
			}

			if (m_WebRequest != null)
			{
				m_WebRequest.Dispose();
				m_WebRequest = null;
			}
		}
	}
}
