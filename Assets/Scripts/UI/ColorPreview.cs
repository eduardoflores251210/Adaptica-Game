using UnityEngine;
using UnityEngine.UIElements;

public class ColorPreview : MonoBehaviour
{
    public UIDocument uiDocument;
    public string ElementName, RSlider, GSlider, BSlider;
    private void Update()
    {
        var root = uiDocument.rootVisualElement;
        if (root != null )
        {
            var rr  = root.Q<SliderInt>(RSlider);
            var gg  = root.Q<SliderInt>(GSlider);
            var bb  = root.Q<SliderInt>(BSlider);
            byte Rval = (byte)rr.value;
            byte Gval = (byte)gg.value;
            byte Bval = (byte)bb.value;
            ChangeBackgroundColor(ElementName, Rval, Gval, Bval);
        }
    }
    public void ChangeBackgroundColor(string elementName, byte r, byte g, byte b)
    {
        if (uiDocument == null)
        {
            Debug.LogError("No hay UIDocument asignado.");
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;

        VisualElement targetElement = root.Q<VisualElement>(elementName);

        if (targetElement == null)
        {
            Debug.LogWarning($"Elemento con nombre '{elementName}' no encontrado en el UI Document.");
            return;
        }

        Color newColor = new Color(r / 255f, g / 255f, b / 255f);

        targetElement.style.backgroundColor = new StyleColor(newColor);

        //Debug.Log($"Color de fondo del elemento '{elementName}' cambiado a RGB({r}, {g}, {b}).");
    }
    public Color GetColor()
    {
        var root = uiDocument.rootVisualElement;
        if (root != null)
        {
            var rr = root.Q<SliderInt>(RSlider);
            var gg = root.Q<SliderInt>(GSlider);
            var bb = root.Q<SliderInt>(BSlider);
            byte Rval = (byte)rr.value;
            byte Gval = (byte)gg.value;
            byte Bval = (byte)bb.value;
            return InternalGetColor(Rval, Gval, Bval);
        }
        else return Color.black;
    }
    public Color GetColor(float Alpha)
    {
        var BaseColor = GetColor();
        BaseColor.a = Alpha;
        return BaseColor;
    }
    public void SetColor(Color color)
    {
        var root = uiDocument.rootVisualElement;
        float rf, gf, bf;
        rf = color.r; gf = color.g;
        bf = color.b;
        byte R,G, B;
        R = (byte)(rf * 255);
        G = (byte)(gf * 255);
        B = (byte)(bf * 255);
        var rr = root.Q<SliderInt>(RSlider);
        var gg = root.Q<SliderInt>(GSlider);
        var bb = root.Q<SliderInt>(BSlider);
        rr.value = R;
        gg.value = G;
        bb.value = B;
    }
    private Color InternalGetColor(byte r, byte g, byte b)
    {
        return  new Color(r / 255f, g / 255f, b / 255f);
    }
}
