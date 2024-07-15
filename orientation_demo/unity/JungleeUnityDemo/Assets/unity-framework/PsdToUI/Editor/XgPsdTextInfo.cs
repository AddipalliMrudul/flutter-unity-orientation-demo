using System.Collections;
using XcelerateGames.PsdImporter.PsdParser;
using XcelerateGames.PsdImporter.PsdParser.Readers.LayerResources;
using XcelerateGames.PsdImporter.PsdParser.Structures;

namespace XcelerateGames.EditorTools.PsdToUI.Text
{
    class XgPsdTextInfo
    {
        public string text;
        public float[] color;
        public int fontSize;
        public string fontName;
        public bool boldEnabled;
        public bool italicsEnabled;

        public XgPsdTextInfo(PsdLayer layer)
        {
            Reader_TySh reader = layer.Resources["TySh"] as Reader_TySh;
            DescriptorStructure text = null;
            if (reader.TryGetValue(ref text, "Text"))
                this.text = text["Txt"].ToString();
            var engineData = text["EngineData"] as StructureEngineData;
            var engineDict = engineData["EngineDict"] as Properties;

            var stylerun = engineDict["StyleRun"] as Properties;
            var runarray = stylerun["RunArray"] as ArrayList;
            var styleSheet = (runarray[0] as Properties)["StyleSheet"] as Properties;
            var styleSheetsData = (styleSheet as Properties)["StyleSheetData"] as Properties;
            if (styleSheetsData.Contains("FontSize"))
            {
                fontSize = (int)(System.Single)styleSheetsData["FontSize"];
            }
            if (styleSheetsData.Contains("Font"))
            {
                var docResources = engineData["DocumentResources"] as Properties;
                var fontSet = docResources["FontSet"] as ArrayList;
                int fontIndex = (int)styleSheetsData["Font"];
                var fontData = fontSet[fontIndex] as Properties;
                fontName = fontData["Name"] as string;
            }

            if(styleSheetsData.Contains("FauxBold"))
            {
                var val = styleSheetsData["FauxBold"];
                boldEnabled = (bool)val;
            }

            if (styleSheetsData.Contains("FillColor"))
            {
                var strokeColorProp = styleSheetsData["FillColor"] as Properties;
                var strokeColor = strokeColorProp["Values"] as ArrayList;
                if (strokeColor != null && strokeColor.Count >= 4)
                {
                    color = new float[] {
                        float.Parse(strokeColor[1].ToString()),
                        float.Parse(strokeColor[2].ToString()),
                        float.Parse(strokeColor[3].ToString()),
                        float.Parse(strokeColor[0].ToString())};
                }
                else
                {
                    color = new float[4] { 0, 0, 0, 1 };
                }
            }
            else
            {
                color = new float[4] { 0, 0, 0, 1 };
            }
        }
    }
}
