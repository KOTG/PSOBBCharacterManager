using PSOBBCharactorGetter;

namespace PSOBBCharacterDataDecoderWeb.Model
{
    /// <summary>
    /// Psobb CharacterModel (charactor wrapper)
    /// </summary>
    public class CharacterModel
    {
        public Charactor charactor { get; set; }

        public IEnumerable<ItemModel> Items { get; set; }

        public IEnumerable<ItemModel> Banks { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hexCharactor"></param>
        /// <param name="slotNumber"></param>
        public CharacterModel(Charactor charactor)
        {
            this.charactor = charactor;

            Items = charactor.Inventory.Select(x => new ItemModel()
            {
                ItemCode = x[0],
                Item = x[1],
                SlotNumber = x[2]
            });

            Banks = charactor.Bank.Select(x => new ItemModel()
            {
                ItemCode = x[0],
                Item = x[1],
                SlotNumber = x[2]
            });
        }

        public string Name
        {
            get => charactor.Name;
        }

        public string Level
        {
            get => charactor.Lavel;
        }

        public string Experience
        {
            get => charactor.Experience;
        }

        public string Race
        {
            get => charactor.Race;
        }
    }
}
