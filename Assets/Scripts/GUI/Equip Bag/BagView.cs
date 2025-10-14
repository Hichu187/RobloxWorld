using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game
{
    public class BagView : MonoBehaviour
    {
        public List<SlapBagOption> option;

        private void Start()
        {
            for (int i = 0; i < option.Count; i++)
            {
                int index = i;

                option[index].GetComponent<Button>().onClick.AddListener(() => { SelectOption(index); });
            }

            switch (SceneManager.GetActiveScene().name)
            {
                case "Game Steal Brainrot":
                    SelectOption(DataStealBrainrot.curSlap);
                    break;
            }
        }

        public void SelectOption(int id)
        {
            foreach( var op in option)
            {
                op.status.SetActive(false);
            }

            option[id].status.SetActive(true);


            //Logic equipment
            switch (SceneManager.GetActiveScene().name)
            {
                case "Game Steal Brainrot":
                    DataStealBrainrot.SetSlapIndex(id);
                    break;
            }
        }
    }
}
