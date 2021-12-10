using PSOBBCharactorGetter;

namespace PSOBBCharacterDataDecoderWeb.Model
{
    /// <summary>
    /// Psobb SearchResultModel
    /// </summary>
    public class SearchResultModel
    {
        public CharacterModel Character { get; set; }
        public ItemModel Item { get; set; }

        public string WhereIsIt { get; set; }
    }
}
