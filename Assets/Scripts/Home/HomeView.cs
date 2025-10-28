using Hichu;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class HomeView : MonoBehaviour
    {
        [SerializeField] private GameOption gameOptionPrefab;
        [SerializeField] private Transform content;
        [SerializeField] private List<GameOption> options;
        private void Start()
        {
            foreach(var op in FactoryMinigame.minigames)
            {
                var option = gameOptionPrefab.Create(content);
                option.InitData(op);

                option.GetComponent<Button>().onClick.AddListener(() => option.SelectMinigame());

                options.Add(option);
            }
        }
    }
}
