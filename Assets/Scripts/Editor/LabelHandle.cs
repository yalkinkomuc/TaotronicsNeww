using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DoorTriggerInteraction))] // Özel Düzenleyici (DoorTriggerInteraction türünde)
// Unity Script | 0 referans // Unity Betiği | 0 referans
class LabelHandle : Editor // Sınıf LabelHandle : Düzenleyici
{
    private static GUIStyle labelStyle; // özel statik GUIStili labelStyle;

    // Unity Mesajı | 0 referans // Unity Mesajı | 0 referans
    private void OnEnable() // özel geçersiz OnEnable()
    {
        labelStyle = new GUIStyle(); // labelStyle = yeni GUIStyle();
        labelStyle.normal.textColor = Color.white; // labelStyle.normal.textColor = Renk.beyaz;
        labelStyle.alignment = TextAnchor.MiddleCenter; // labelStyle.alignment = TextAnchor.MiddleCenter;
    }

    // Unity Mesajı | 0 referans // Unity Mesajı | 0 referans
    private void OnSceneGUI() // özel geçersiz OnSceneGUI()
    {
        DoorTriggerInteraction door = (DoorTriggerInteraction)target; // DoorTriggerInteraction kapı = (DoorTriggerInteraction)hedef;

        Handles.BeginGUI(); // Handles.BeginGUI();
        Handles.Label(door.transform.position + new Vector3(0f, 1f, 0f), door.CurrentDoorPosition.ToString(), labelStyle); // Handles.Label(kapı.dönüşüm.konum + yeni Vektör3(0f, 4f, 0f), kapı.currentDoorPosition.ToString(), labelStyle);
        Handles.EndGUI(); // Handles.EndGUI();
    }
}
