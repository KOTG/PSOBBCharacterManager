using PSOBBCharactorGetter;
using PSOBBCharacterDataDecoderWeb.Model;

namespace PSOBBCharacterDataDecoderWeb.Service.Interfaces
{
    /// <summary>
    /// Interface of Decoding PSOBBCharacter Service
    /// </summary>
    public interface IPSOBBCharacterDataService
    {
        /// <summary>
        /// Get PSOBBCharacter's Data
        /// </summary>
        /// <returns>IEnumerable of PSOBBCharactorGetter.Character</returns>
        public Task<IEnumerable<CharacterModel>> GetCharactors();
    }
}
