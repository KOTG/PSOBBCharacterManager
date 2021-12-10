using PSOBBCharactorGetter;

namespace PSOBBCharacterDataDecoderWeb.Model
{
    /// <summary>
    /// Psobb CharacterModel
    /// </summary>
    public class CharacterModel
    {
        public IEnumerable<ItemModel> Items { get; set; }

        public IEnumerable<ItemModel> Banks { get; set; }

        public string Name
        {
            get; set;
        }

        public string Level
        {
            get; set;
        }

        public string Experience
        {
            get; set;
        }

        public string Race
        {
            get; set;
        }
    }
}
