using PSOBBCharactorGetter;
using PSOBBCharacterDataDecoderWeb.Model;

namespace PSOBBCharacterDataDecoderWeb.Service.Interfaces
{
    /// <summary>
    /// Interface of Decoding PSOBBCharacter Search Service
    /// </summary>
    public interface IPSOBBCharacterSearchService
    {
        /// <summary>
        /// Search PSOBBCharacter's item
        /// </summary>
        /// <returns>IEnumerable of PSOBBCharactorGetter.Character having specify item</returns>
        public Task<IEnumerable<SearchResultModel>> SearchItem(string item);
    }
}
