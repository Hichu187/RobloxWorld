using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class StealBrainrot_Index : MonoBehaviour
    {
        [SerializeField] private List<ButtonRank> btn_ranks;

        private void Start()
        {
            foreach (var btn in btn_ranks)
            {
                btn.UnPicked();
            }

            for (int i = 0; i < btn_ranks.Count; i++)
            {
                int index = i;
                btn_ranks[index].GetComponent<Button>().onClick.AddListener(() => PickRank(index));
            }
        }

        public void PickRank(int index)
        {
            foreach (var btn in btn_ranks)
            {
                btn.UnPicked();
            }

            btn_ranks[index].Picked();
        }
    }
}
