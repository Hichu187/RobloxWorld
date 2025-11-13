using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game
{
    public class BrainrotEvoAI : MonoBehaviour
    {
        [SerializeField] CharacterCombat characterCombat;
        [SerializeField] BrainrotEvoConfig _currentConfig;
        [SerializeField] Transform _meshTransform;

        private AI _ai;

        private void Start()
        {
            _ai = GetComponent<AI>();

            InitData();
        }

        public async void InitData()
        {
            int random = Random.Range(0, FactoryBrainrotEvo.brainrotConfigs.Count - 2);

            _currentConfig = FactoryBrainrotEvo.brainrotConfigs[random];

            characterCombat._maxHealth = (int)_currentConfig.health;
            characterCombat._currentHealth = characterCombat._maxHealth;
            characterCombat._damage = (int)_currentConfig.damage;

            await UniTask.WaitForSeconds(0.35f);

            for (int i = _meshTransform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(_meshTransform.GetChild(i).gameObject);
            }

            var model = Instantiate(_currentConfig.model, _meshTransform);

            await UniTask.WaitUntil(() => model != null);

            _ai.character.cAnim.InitAnimator();
        }
    }
}
