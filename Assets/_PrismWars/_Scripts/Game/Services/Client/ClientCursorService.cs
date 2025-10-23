using UnityEngine;

namespace _PrismWars._Scripts.Systems {
    public class ClientCursorService : MonoBehaviour, IClientService, IInitializable {
        const string _cursorPath = "Cursors/target";
        public void Initialize() {
            // Cursor.SetCursor((Texture2D)Resources.Load(_cursorPath), Vector2.zero, CursorMode.Auto);
        }
    }
}