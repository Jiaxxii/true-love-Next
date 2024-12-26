namespace Xiyu.DataStructure
{
    public readonly struct LoaderSelectShowCharacterInfo
    {
        public LoaderSelectShowCharacterInfo(bool use, string characterName, int difficultyLevel, string content, LightweightRectTransform transform)
        {
            CharacterName = characterName;
            DifficultyLevel = difficultyLevel;
            Content = content;
            Transform = transform;
            Use = use;
        }

        public bool Use { get; }
        public string CharacterName { get; }
        public int DifficultyLevel { get; }
        public string Content { get; }
        public LightweightRectTransform Transform { get; }
    }
}