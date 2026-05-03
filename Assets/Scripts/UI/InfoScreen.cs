using AdapticaDebugStuff;
using UnityEngine;
using UnityEngine.UIElements;

public class InfoScreen : MonoBehaviour
{
    public UIDocument uiDocument;
    public string Name;
    private Label infoLabel;

    void Start()
    {
        var root = uiDocument.rootVisualElement;

        infoLabel = root.Q<Label>(Name);

        string version = Application.version;
        string companyName = Application.companyName;
        string productName = Application.productName;
        if (MetaUtils.IsSnapshot(version))
            infoLabel.text = $"{productName} Snapshot {MetaUtils.GetSnapshotPart(version)} hecho por {companyName}";
		
		else if (MetaUtils.GetPhase(version) != AdapticaDevPhases.Realese)
            infoLabel.text = $"{productName} Versión {version} hecho por {companyName}";
        else 
            infoLabel.text = $"{productName} Versión {MetaUtils.ExtraerParteNumerica(version)} hecho por {companyName}";
    }
}
