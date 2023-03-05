using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ColorPickerWindow : EditorWindow
{
    [MenuItem("Tools/ColorPicker")]
    public static void ShowWindow()
    {
        EditorWindow.CreateWindow<ColorPickerWindow>("ColorPicker");
    }

    private string _hexColor = "FFFFFFFF";
    private string _normalColor = "1f, 1f, 1f, 1f";
    private string _color32 = "255, 255, 255, 255";
    private Color _color = new Color(1, 1, 1, 1);

    void OnGUI()
    {
        string tempHexColor = EditorGUILayout.TextField("HexColor:", _hexColor);
        string tempNormalColor = EditorGUILayout.TextField("NormalColor:", _normalColor);
        string tempColor32 = EditorGUILayout.TextField("Color32:", _color32);
        Color tempColorValue = EditorGUILayout.ColorField("Color:",_color);

        if(tempHexColor != _hexColor)
        {
            _hexColor = tempHexColor;
            _color = HexToColor(_hexColor);
            UpdateColor();

            this.Repaint();
        }
        else if (tempNormalColor != _normalColor)
        {
            _normalColor = tempNormalColor;
            _color = NormalToColor(_normalColor);
            UpdateColor();

            this.Repaint();
        }
        else if (tempColor32 != _color32)
        {
            _color32 = tempColor32;
            _color = Color32ToColor(_color32);
            UpdateColor();

            this.Repaint();
        }
        else if(tempColorValue != _color)
        {
            _color = tempColorValue;
            UpdateColor();

            this.Repaint();
        }      
    }

    private Color HexToColor(string value)
    {
        Color color;
        value = value.Replace("0x", "");
        value = value.Replace("0X", "");
        if(value.IndexOf("#") != 0)
            value = "#" + value;
        ColorUtility.TryParseHtmlString(value, out color);
        return color;
    }

    private Color NormalToColor(string value)
    {
        Color color = new Color();
        value = value.Replace(" ","");
        value = value.Replace("f", "");
        string[] values = value.Split(',');
        float[] numbers = new float[4];
        for(int i = 0; i < 4; i++)
        {
            if(i < values.Length)
            {
                float.TryParse(values[i], out numbers[i]);

                numbers[i] = Mathf.Clamp(numbers[i], 0.0f, 1.0f);
            }
            else
            {
                numbers[i] = 1.0f;
            }
        }

        color.r = numbers[0];
        color.g = numbers[1];
        color.b = numbers[2];
        color.a = numbers[3];

        return color;
    }

    private Color Color32ToColor(string value)
    {
        Color32 color = new Color32();
        value = value.Replace(" ", "");
        string[] values = value.Split(',');
        byte[] numbers = new byte[4];
        for (int i = 0; i < 4; i++)
        {
            if (i < values.Length)
            {
                byte.TryParse(values[i], out numbers[i]);
            }
            else
            {
                numbers[i] = 255;
            }
        }
        color.r = numbers[0];
        color.g = numbers[1];
        color.b = numbers[2];
        color.a = numbers[3];

        return color;
    }

    private void UpdateColor()
    {
        _hexColor = ColorUtility.ToHtmlStringRGBA(_color);
        _normalColor = string.Format("{0}f, {1}f, {2}f, {3}f", _color.r, _color.g, _color.b, _color.a);
        Color32 color32 = _color;
        _color32 = string.Format("{0}, {1}, {2}, {3}", color32.r, color32.g, color32.b, color32.a);
    }
}
