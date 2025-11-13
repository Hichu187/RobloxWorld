using Cysharp.Threading.Tasks;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game
{
    public class CharacterRenderer : MonoBehaviour
    {
        private Character _character;
        [SerializeField] Transform _meshParent;
        private void Awake()
        {
            _character = GetComponent<Character>();
        }

        public async void LoadSkin(int skinIndex)
        {
            if (_meshParent == null) return;

            var handle = FactorySkin.skin[skinIndex].InstantiateAsync(_meshParent);

            GameObject obj = await handle.Task;

            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;

            _character.cRagdoll = obj.GetComponent<CharacterRagdoll>();
            _character.itemManager = obj.GetComponentInChildren<CharacterItemManager>();
            _character.SetItemManager();
            _character.cAnim._animator = obj.GetComponent<Animator>();


        }
    }
}
