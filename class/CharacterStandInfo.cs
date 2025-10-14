using System.Collections.Generic;

namespace TRPGLogArrangeTool.resource
{
    public class CharacterStandInfo
    {
        /// <summary>
        /// キャラクター名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Stand画像情報一覧
        /// Stand名,画像名
        /// </summary>
        public Dictionary<string, string> StandDictionary { get; set; } = new Dictionary<string, string>();

    }
}
