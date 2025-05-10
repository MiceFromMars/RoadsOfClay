using System.Threading;
using Cysharp.Threading.Tasks;

namespace ROC.Data.SaveLoad
{
    public interface ISaveLoadService
    {
        UniTask<PlayerProgressData> LoadProgress(CancellationToken cancellationToken);
        UniTask SaveProgress(PlayerProgressData progressData, CancellationToken cancellationToken);
    }
} 