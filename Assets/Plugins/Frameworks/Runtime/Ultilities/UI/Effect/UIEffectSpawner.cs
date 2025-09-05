using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;


namespace Hichu
{
    public static class UIEffectSpawner
    {
        public static void SpawnEffect(
            RectTransform spawnArea,
            RectTransform target,
            GameObject imagePrefab,
            int spawnCount = 10,
            float moveDuration = 1.5f,
            float scaleDuration = 0.5f,
            Vector2 randomOffset = default,
            float delayBetweenSpawns = 0.2f,
            Action onComplete = null
)
        {
            if (spawnArea == null || target == null || imagePrefab == null)
            {
                Debug.LogError("UIEffectSpawner: Missing required parameters!");
                return;
            }

            CoroutineRunner.Instance.StartCoroutine(SpawnEffectRoutine(
                spawnArea, target, imagePrefab, spawnCount, moveDuration,
                scaleDuration, randomOffset, delayBetweenSpawns, onComplete
            ));
        }

        private static IEnumerator SpawnEffectRoutine(
RectTransform spawnArea,
    RectTransform target,
    GameObject imagePrefab,
    int spawnCount,
    float moveDuration,
    float scaleDuration,
    Vector2 randomOffset,
    float delayBetweenSpawns,
    Action onComplete
)
        {
            if (spawnArea == null || target == null || imagePrefab == null)
            {
                Debug.LogWarning("SpawnEffectRoutine: One or more required references are null. Aborting.");
                yield break;
            }

            int completed = 0;

            for (int i = 0; i < spawnCount; i++)
            {
                if (spawnArea == null || target == null || imagePrefab == null)
                {
                    Debug.LogWarning("SpawnEffectRoutine aborted due to destroyed object.");
                    yield break;
                }

                SpawnImage(spawnArea, target, imagePrefab, moveDuration, scaleDuration, randomOffset, () =>
                {
                    completed++;
                    if (completed == spawnCount)
                    {
                        if (onComplete != null)
                        {
                            onComplete.Invoke();
                        }
                    }
                });

                yield return new WaitForSeconds(delayBetweenSpawns);
            }
        }

        private static void SpawnImage(
    Transform spawnArea,
    RectTransform target,
    GameObject imagePrefab,
    float moveDuration,
    float scaleDuration,
    Vector2 randomOffset,
    Action onDone
)
        {
            GameObject newImage = Object.Instantiate(imagePrefab, spawnArea);
            RectTransform imgRect = newImage.GetComponent<RectTransform>();

            Vector2 randomPos = new Vector2(
                UnityEngine.Random.Range(-randomOffset.x, randomOffset.x),
                UnityEngine.Random.Range(-randomOffset.y, randomOffset.y)
            );
            imgRect.anchoredPosition = randomPos;

            imgRect.SetParent(spawnArea.root);

            imgRect.localScale = Vector3.zero;
            imgRect.DOScale(Vector3.one, scaleDuration).SetEase(Ease.OutBack);

            Vector2 worldPos = target.position;
            imgRect.DOMove(worldPos, moveDuration, false)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    Object.Destroy(newImage);
                    onDone?.Invoke();
                });
        }
    }
}
