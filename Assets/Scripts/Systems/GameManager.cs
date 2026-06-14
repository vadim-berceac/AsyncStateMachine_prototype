using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager
{
    private bool _isInitialized;

    private GameManager()
    {
        if (_isInitialized)
        {
            return;
        }
        
        _isInitialized = true;
        _ = InitializeGameAsync();
    }
    
    private async UniTask InitializeGameAsync()
    {
        var loadLevelTask = LoadLevelAsync();
        var updateUITask = UpdateUIAsync();
        var saveGameTask = SaveGameAsync();

        await UniTask.WhenAll(loadLevelTask, updateUITask, saveGameTask);
        Debug.Log("Все задачи завершены!");
    }

    private async UniTask LoadLevelAsync()
    {
        await SceneManager.LoadSceneAsync(LevelsEnum.Level1.ToString());
        Debug.Log("Уровень загружен");
    }

    private async UniTask UpdateUIAsync()
    {
        await UniTask.Delay(500);
        Debug.Log("UI обновлен");
    }

    private async UniTask SaveGameAsync()
    {
        await UniTask.Delay(1000);
        Debug.Log("Игра сохранена");
    }
}