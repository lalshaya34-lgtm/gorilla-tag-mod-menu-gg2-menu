
using UnityEngine;

namespace GorillaTagMod.Menu
{
    public sealed class WristMenu : MonoBehaviour
    {
        private bool _isOpen;
        private int _category;
        private int _page;

        private Rect _window = new Rect(20, 20, 360, 400);

        private readonly string[] _categories =
        {
            "Movement",
            "Platforms",
            "Visuals",
            "Utility",
            "Settings"
        };

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Insert))
                _isOpen = !_isOpen;

            _category = Mathf.Clamp(
                _category, 0, _categories.Length - 1);

            _page = Mathf.Max(0, _page);
        }

        private void OnGUI()
        {
            if (!_isOpen)
                return;

            _window = GUI.Window(
                GetInstanceID(),
                _window,
                DrawWindow,
                "Gorilla Tag Mod Menu");
        }

        private void DrawWindow(int id)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("<"))
            {
                _category =
                    (_category - 1 + _categories.Length)
                    % _categories.Length;
                _page = 0;
            }

            GUILayout.Label(_categories[_category]);

            if (GUILayout.Button(">"))
            {
                _category =
                    (_category + 1) % _categories.Length;
                _page = 0;
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            GUILayout.Label("Category: " + _categories[_category]);
            GUILayout.Label("Page: " + (_page + 1));

            if (GUILayout.Button("Previous Page"))
                _page = Mathf.Max(0, _page - 1);

            if (GUILayout.Button("Next Page"))
                _page++;

            if (GUILayout.Button("Close Menu"))
                _isOpen = false;

            GUI.DragWindow(
                new Rect(0, 0, 10000, 25));
        }
    }
}
