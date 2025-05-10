using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ROC.Data.SaveLoad
{
    public class SaveLoadService : ISaveLoadService, IDisposable
    {
        private const string ProgressFileName = "player_progress.json";
        private readonly string _progressFilePath;
        private readonly SemaphoreSlim _semaphore;
        
        public SaveLoadService()
        {
            _progressFilePath = Path.Combine(Application.persistentDataPath, ProgressFileName);
            _semaphore = new SemaphoreSlim(1, 1);
        }
        
        public async UniTask<PlayerProgressData> LoadProgress(CancellationToken cancellationToken)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);
                
                if (!File.Exists(_progressFilePath))
                    return CreateDefaultProgress();
                
                string json = await ReadFileAsync(_progressFilePath, cancellationToken);
                
                // Move JSON deserialization to a background thread
                return await UniTask.RunOnThreadPool(() => 
                {
                    try
                    {
                        return JsonUtility.FromJson<PlayerProgressData>(json);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to deserialize player progress: {ex.Message}");
                        return CreateDefaultProgress();
                    }
                }, cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading progress: {ex.Message}");
                return CreateDefaultProgress();
            }
            finally
            {
                _semaphore.Release();
            }
        }
        
        public async UniTask SaveProgress(PlayerProgressData progressData, CancellationToken cancellationToken)
        {
            if (progressData == null)
                throw new ArgumentNullException(nameof(progressData));
                
            try
            {
                await _semaphore.WaitAsync(cancellationToken);
                
                // Move JSON serialization to a background thread
                string json = await UniTask.RunOnThreadPool(() => 
                {
                    return JsonUtility.ToJson(progressData);
                }, cancellationToken: cancellationToken);
                
                await WriteFileAsync(_progressFilePath, json, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving progress: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }
        
        private async UniTask<string> ReadFileAsync(string path, CancellationToken cancellationToken)
        {
            try
            {
                return await File.ReadAllTextAsync(path, cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading file at {path}: {ex.Message}");
                throw;
            }
        }
        
        private async UniTask WriteFileAsync(string path, string content, CancellationToken cancellationToken)
        {
            try
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                    
                await File.WriteAllTextAsync(path, content, cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error writing file at {path}: {ex.Message}");
                throw;
            }
        }
        
        private PlayerProgressData CreateDefaultProgress()
        {
            PlayerProgressData defaultProgress = new PlayerProgressData
            {
                TotalScore = 0,
                MaxHeight = 0,
                MaxSpeed = 0,
                LevelProgress = new System.Collections.Generic.List<LevelProgressData>()
            };
            
            // Initialize with level 0 unlocked
            defaultProgress.LevelProgress.Add(new LevelProgressData
            {
                LevelIndex = 0,
                IsUnlocked = true,
                MaxScore = 0,
                MaxHeight = 0,
                MaxSpeed = 0
            });
            
            return defaultProgress;
        }
        
        public void Dispose()
        {
            _semaphore?.Dispose();
        }
    }
} 