using System;
using UnityEngine;
using UnityEngine.UI;

namespace _PrismWars._Scripts.UI.CharacterSelection.View {
    [Serializable]
    public class CharacterButton {
        public Button button;
        public Image characterImage;
        public Image backgroundImage;
        public GameObject selectionFrame;
        public Image selectedImageFrame;
    }
}