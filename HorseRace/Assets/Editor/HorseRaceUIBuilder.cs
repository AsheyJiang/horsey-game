using HorseRace;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class HorseRaceUIBuilder
{
    private const string MenuPath = "HorseRace/UI/Create Editable UGUI";

    [MenuItem(MenuPath)]
    private static void CreateEditableUi()
    {
        GameUIRoot existingRoot = Object.FindObjectOfType<GameUIRoot>();
        if (existingRoot != null)
        {
            Selection.activeGameObject = existingRoot.gameObject;
            EditorGUIUtility.PingObject(existingRoot.gameObject);
            Debug.Log("Editable HorseRace UGUI already exists. Existing hierarchy was selected and left unchanged.");
            return;
        }

        HorseRaceGame game = Object.FindObjectOfType<HorseRaceGame>();
        GameObject host;
        if (game == null)
        {
            host = new GameObject("HorseRaceMVP");
            Undo.RegisterCreatedObjectUndo(host, "Create HorseRace UI Host");
            game = Undo.AddComponent<HorseRaceGame>(host);
        }
        else
        {
            host = game.gameObject;
        }

        Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        HorseRaceUIFactory.BuildResult build = HorseRaceUIFactory.Create(host.transform, font);
        Undo.RegisterCreatedObjectUndo(build.Root.gameObject, "Create Editable HorseRace UGUI");

        game.AssignUIView(build.RaceView);
        GameFlowController flow = host.GetComponent<GameFlowController>();
        if (flow == null)
        {
            flow = Undo.AddComponent<GameFlowController>(host);
        }
        flow.Configure(build.Root, game, shouldStartAtRace: false);

        MainMenuController mainMenu = build.MainMenuView.GetComponent<MainMenuController>();
        mainMenu.Configure(build.MainMenuView, game, flow);

        EditorUtility.SetDirty(game);
        EditorUtility.SetDirty(flow);
        EditorUtility.SetDirty(build.Root);
        EditorUtility.SetDirty(build.MainMenuView);
        EditorUtility.SetDirty(mainMenu);
        EditorUtility.SetDirty(build.RaceView);

        Scene activeScene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        Selection.activeGameObject = build.MainMenuView.gameObject;
        EditorGUIUtility.PingObject(build.MainMenuView.gameObject);
        Debug.Log("Editable HorseRace UGUI created. Adjust the hierarchy, then save the scene when ready.");
    }

    [MenuItem(MenuPath, true)]
    private static bool ValidateCreateEditableUi()
    {
        return !Application.isPlaying;
    }
}
