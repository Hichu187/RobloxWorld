using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace Hichu
{
    public enum LoadingMode
    {
        Real = 0,
        FakeThenReal = 1
    }

    public class SceneLoader : MonoSingleton<SceneLoader>
    {
        private static readonly float s_loadSceneProgressMax = 0.7f;

        [Title("UI")]
        [SerializeField] private Slider _progressSlider;

        [Title("Event")]
        [SerializeField] private UnityEvent _onFadeInStart;
        [SerializeField] private UnityEvent _onFadeInEnd;
        [SerializeField] private UnityEvent _onFadeOutStart;
        [SerializeField] private UnityEvent _onFadeOutEnd;
        [SerializeField] private UnityEvent<float> _onLoadProgress;

        [Title("Config")]
        [SerializeField] private float _fadeInDuration = 0.4f;
        [SerializeField] private float _fadeOutDuration = 0.4f;
        [SerializeField] private float _loadMinDuration = 1f;

        [SerializeField, MinMaxSlider(0f, 5f, true)]
        private Vector2 _fakeLoadDurationRange = new Vector2(1f, 2.5f);

        [SerializeField] private LoadingMode _loadingMode = LoadingMode.FakeThenReal;

        private bool _isTransiting = false;

        public float fadeInDuration => _fadeInDuration;
        public float fadeOutDuration => _fadeOutDuration;

        private void ReportProgress(float p)
        {
            p = Mathf.Clamp01(p);
            _onLoadProgress?.Invoke(p);
            if (_progressSlider != null)
                _progressSlider.value = p;
        }

        private async UniTaskVoid LoadAsync(AsyncOperation asyncOperation)
        {
            GameObjectCached.SetActive(true);
            _isTransiting = true;

            ReportProgress(0f);
            _onFadeInStart?.Invoke();
            await UniTask.WaitForSeconds(_fadeInDuration, true);
            _onFadeInEnd?.Invoke();

            if (_loadingMode == LoadingMode.FakeThenReal)
            {
                float fakeDuration = Random.Range(_fakeLoadDurationRange.x, _fakeLoadDurationRange.y);
                float elapsed = 0f;
                const float fakeTarget = 0.9f;

                while (elapsed < fakeDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / fakeDuration);
                    float prog = Mathf.Lerp(0f, fakeTarget, t);
                    ReportProgress(prog);
                    await UniTask.Yield();
                }

                ReportProgress(fakeTarget);

                await WaitForSceneLoadedOrMinDuration(asyncOperation, true);
            }
            else
            {
                await WaitForSceneLoadedOrMinDuration(asyncOperation, false);
            }

            _onFadeOutStart?.Invoke();
            await UniTask.WaitForSeconds(_fadeOutDuration, true);
            _onFadeOutEnd?.Invoke();

            _isTransiting = false;
        }

        private async UniTask WaitForSceneLoadedOrMinDuration(AsyncOperation handle, bool preserveCurrentProgress)
        {
            var progress = Progress.CreateOnlyValueChanged<float>(x =>
            {
                float mapped = x * s_loadSceneProgressMax;

                if (preserveCurrentProgress && _progressSlider != null)
                {
                    float current = _progressSlider.value;
                    mapped = Mathf.Max(mapped, current);
                }

                ReportProgress(mapped);
            });

            float timeStartLoading = Time.unscaledTime;

            handle.allowSceneActivation = true;
            await handle.ToUniTask(progress);

            await Resources.UnloadUnusedAssets();

            float timeRemain = _loadMinDuration - (Time.unscaledTime - timeStartLoading);
            float time = 0f;

            while (time < timeRemain)
            {
                time += Time.unscaledDeltaTime;
                await UniTask.Yield();

                float lerpLocal = (timeRemain <= 0f) ? 1f : (time / timeRemain);
                float finalProgress = lerpLocal * (1f - s_loadSceneProgressMax) + s_loadSceneProgressMax;

                if (preserveCurrentProgress && _progressSlider != null)
                    finalProgress = Mathf.Max(finalProgress, _progressSlider.value);

                ReportProgress(finalProgress);
            }

            ReportProgress(1f);
        }

        #region Public

        public void Load(string sceneName)
        {
            if (_isTransiting)
            {
                LDebug.Log<SceneLoader>("A scene is transiting, can't execute load scene command!");
                return;
            }

            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            asyncOperation.allowSceneActivation = false;

            LoadAsync(asyncOperation).Forget();
        }

        public void Load(int sceneBuildIndex)
        {
            if (_isTransiting)
            {
                LDebug.Log<SceneLoader>("A scene is transiting, can't execute load scene command!");
                return;
            }

            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneBuildIndex);
            asyncOperation.allowSceneActivation = false;

            LoadAsync(asyncOperation).Forget();
        }

        #endregion
    }

    public static class SceneLoaderHelper
    {
        private static bool _isInitialized = false;

        private static void LazyInit()
        {
            if (_isInitialized)
                return;

            LFactory.sceneLoaderPrefab.Create();
            _isInitialized = true;
        }

        public static void Load(int sceneBuildIndex)
        {
            LazyInit();
            SceneLoader.Instance.Load(sceneBuildIndex);
        }

        public static void Load(string sceneName)
        {
            LazyInit();
            SceneLoader.Instance.Load(sceneName);
        }

        public static void Reload()
        {
            LazyInit();
            SceneLoader.Instance.Load(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
