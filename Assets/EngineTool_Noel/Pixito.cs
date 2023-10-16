using UnityEditor;
using UnityEngine;
using System.IO;

public class Pixito : EditorWindow
{
    private Texture2D canvas;
    private Color32[] pixels;
    private Color32 selectedColor = Color.black;

    private bool isDrawing = false;
    private bool isErasing = false;
    private bool isEyedropperActive = false;
    private bool isManualDisplayed = false;

    private int canvasWidth = 32;
    private int canvasHeight = 32;
    private int dimension = 16;

    #region manual
    private string manualText = "\nThank you for downloading Pixito - The Pixel Art Drawing Tool inside Unity. \n" +
        "\nBrush            -> Draw pixels in the selected color on the canvas. " +
        "\nEraser           -> Erase pixels on the canvas. " +
        "\nEyedropper       -> Pick a color by selecting a pixel on the canvas. " +
        "\nClear            -> Clear the canvas." +
        "\nColor Picker     -> Pick a brush color." +
        "\nSave             -> Save your pixel art image with the optimal settings for pixel art";
    #endregion

    [MenuItem("Tools/Pixito")]
    public static void OpenPixito()
    {
        Pixito window = GetWindow<Pixito>();
        window.titleContent = new GUIContent("Pixito");
        window.Show();
    }
    private void OnEnable()
    {
        canvas = new Texture2D(canvasWidth, canvasHeight, TextureFormat.ARGB32, false);
        canvas.filterMode = FilterMode.Point;
        pixels = canvas.GetPixels32();

        ClearCanvas();
    }
    private void OnGUI()
    {
        Event e = Event.current;

        minSize = new Vector2(640, 640);
        maxSize = new Vector2(640, 640);
        Rect canvasRect = CalculateCanvasRect();
        Rect toolRect = CalculateToolRect();
        float mouseY = InvertYAxisMouse(canvasRect, e.mousePosition.y);

        DrawBackgrounds(canvasRect, toolRect);

        if (!isManualDisplayed)
        {
            SwitchMouseEvents(e, canvasRect, mouseY);
            DrawGrid(canvasRect);
        }
        else
            GUI.Label(new Rect(canvasRect.x, canvasRect.y, canvasRect.width, canvasRect.height), manualText, GUI.skin.textArea);

        ShowUI(canvasRect);
    }
    private Rect CalculateCanvasRect()
    {
        return new Rect((position.width - dimension * canvasWidth) / 2 + 37, (position.height - dimension * canvasHeight) / 2, dimension * canvasWidth, dimension * canvasHeight);
    }
    private Rect CalculateToolRect()
    {
        return new Rect(0, 0, 75, position.height);
    }
    private float InvertYAxisMouse(Rect canvasRect, float mouseY)
    {
        return canvasRect.height - (mouseY - canvasRect.y);
    }
    private void DrawBackgrounds(Rect canvasRect, Rect toolRect)
    {
        EditorGUI.DrawRect(canvasRect, Color.grey);
        EditorGUI.DrawRect(toolRect, Color.grey);
    }
    private void SwitchMouseEvents(Event e, Rect canvasRect, float mouseY)
    {
        switch (e.type)
        {
            case EventType.MouseDrag:
            case EventType.MouseDown:
                if (!isEyedropperActive)
                    PixelDrawing(e, canvasRect);
                else if (isEyedropperActive)
                    Eyedrop(e, canvasRect);
                break;
        }
    }
    private void PixelDrawing(Event e, Rect canvasRect)
    {
        int x = (int)((e.mousePosition.x - canvasRect.x) / dimension);
        int y = (int)(InvertYAxisMouse(canvasRect, e.mousePosition.y) / dimension);
        int index = x + (y * canvasWidth);

        if (canvasRect.Contains(e.mousePosition))
        {
            Color32[] previousPixels = (Color32[])pixels.Clone();

            if (isErasing)
                pixels[index] = Color.clear;
            else if (isDrawing)
                pixels[index] = selectedColor;

            canvas.SetPixels32(pixels);
            canvas.Apply();
            GUI.changed = true;
        }
    }
    private void Eyedrop(Event e, Rect canvasRect)
    {
        int x = (int)((e.mousePosition.x - canvasRect.x) / dimension);
        int y = (int)(InvertYAxisMouse(canvasRect, e.mousePosition.y) / dimension);
        int index = x + (y * canvasWidth);

        if (canvasRect.Contains(e.mousePosition))
        {
            if (pixels[index] == Color.clear || pixels[index].a < 0.05)
                return;
            else
                selectedColor = pixels[index];
            GUI.changed = true;
        }
    }
    private void DrawGrid(Rect canvasRect)
    {
        GUI.DrawTexture(canvasRect, canvas);
        for (int x = 0; x <= canvasWidth; x++)
        {
            float xPos = canvasRect.x + x * dimension;
            Handles.DrawLine(new Vector2(xPos, canvasRect.y), new Vector2(xPos, canvasRect.y + canvasRect.height));
        }

        for (int y = 0; y <= canvasHeight; y++)
        {
            float yPos = canvasRect.y + y * dimension;
            Handles.DrawLine(new Vector2(canvasRect.x, yPos), new Vector2(canvasRect.x + canvasRect.width, yPos));
        }
    }
    private void ShowUI(Rect canvasRect)
    {
        selectedColor = EditorGUI.ColorField(new Rect(10, (position.height / 2) - 240, 55, 20), selectedColor);

        #region brush

        GUI.backgroundColor = Color.grey;

        if (isDrawing == true && isErasing == false && isEyedropperActive == false)
            GUI.backgroundColor = Color.blue;
        else
            GUI.backgroundColor = Color.grey;

        if (GUI.Button(new Rect(10, (position.height / 2) - 200, 55, 55), "Brush"))
        {
            isDrawing = true;
            isErasing = false;
            isEyedropperActive = false;
        }

        #endregion

        #region eraser

        if (isDrawing == false && isErasing == true && isEyedropperActive == false)
            GUI.backgroundColor = Color.blue;
        else
            GUI.backgroundColor = Color.grey;

        if (GUI.Button(new Rect(10, (position.height / 2) - 135, 55, 55), "Eraser"))
        {
            isDrawing = false;
            isErasing = true;
            isEyedropperActive = false;
        }

        #endregion

        #region eyedropper

        if (isDrawing == false && isErasing == false && isEyedropperActive == true)
            GUI.backgroundColor = Color.blue;
        else
            GUI.backgroundColor = Color.grey;

        if (GUI.Button(new Rect(10, (position.height / 2) - 70, 55, 55), "Eye- \ndropper"))
        {
            isDrawing = false;
            isErasing = false;
            isEyedropperActive = true;
        }

        #endregion

        GUI.backgroundColor = Color.grey;

        if (GUI.Button(new Rect(10, (position.height / 2) - 5, 55, 55), "Clear"))
        {
            ClearCanvas();
        }

        if (GUI.Button(new Rect(10, (position.height / 2) + 60, 55, 55), "Manual"))
        {
            isManualDisplayed = true;
            isDrawing = false;
        }

        if (GUI.Button(new Rect(10, (position.height / 2) + 125, 55, 55), "Save"))
        {
            SavePixelArt();
        }

        if (isManualDisplayed == true)
        {
            if (GUI.Button(new Rect((position.width / 2), (position.height / 2) + 270, 80, 20), "Close"))
            {
                isManualDisplayed = false;
            }
        }
    }
    private void ClearCanvas()
    {
        pixels = new Color32[canvasWidth * canvasHeight];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        canvas.SetPixels32(pixels);
        canvas.Apply();
        GUI.changed = true;
    }
    private void SavePixelArt()
    {
        string folderNamePath = "Assets/Pixito";

        if (!AssetDatabase.IsValidFolder(folderNamePath))
            AssetDatabase.CreateFolder("Assets", "Pixito");

        string baseFileName = "Pixito";
        string fileName = SetSavedFileName(folderNamePath, baseFileName, "png");
        string filePath = Path.Combine(folderNamePath, fileName);

        Texture2D savedPixelArt = new Texture2D(canvasWidth, canvasHeight, TextureFormat.RGBA32, false);
        savedPixelArt.SetPixels32(pixels);
        savedPixelArt.Apply();

        byte[] pngBytes = savedPixelArt.EncodeToPNG();

        File.WriteAllBytes(filePath, pngBytes);
        AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);

        TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(filePath);
        textureImporter.textureType = TextureImporterType.Sprite;
        textureImporter.filterMode = FilterMode.Point;
        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;

        AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();
    }
    private string SetSavedFileName(string folderPath, string baseFileName, string export)
    {
        int index = 0;
        string fileName = baseFileName + "." + export;

        while (File.Exists(Path.Combine(folderPath, fileName)))
        {
            index++;
            fileName = baseFileName + "_" + index + "." + export;
        }
        return fileName;
    }
}