using PSOBBCharacterDataDecoderWeb.Service.Interfaces;
using PSOBBCharactorGetter;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.IO;
using PSOBBCharacterDataDecoderWeb.Model;

namespace PSOBBCharacterDataDecoderWeb.Service.Implements
{
    /// <summary>
    /// PSOBBCharacterSearchService by PSOBB Binary File.
    /// </summary>
    public class PSOBBCharacterSearchFileService : IPSOBBCharacterSearchService
    {
        private IEnumerable<CharacterModel> SearchingModels { get; set; }

        /// <summary>
        /// Initialize Service
        /// </summary>
        /// <param name="fileInfo">FileInfos</param>
        public void Initialize(IEnumerable<CharacterModel> models)
        {
            SearchingModels = models;
        }


        public Task<IEnumerable<SearchResultModel>> SearchItem(string item)
        {
            var resultList = new List<SearchResultModel>();

            Func<ItemModel, bool> SearchPredicate = (i) => i.Item.Contains(item, StringComparison.OrdinalIgnoreCase);

            SearchingModels.ToList().ForEach(character =>
            {
                character.Items?.Where(SearchPredicate).ToList().ForEach(i =>
                {
                    resultList.Add(new SearchResultModel()
                    {
                        Character = character,
                        Item = i,
                        WhereIsIt = "Inventory",
                    });
                });

                character.Banks?.Where(SearchPredicate).ToList().ForEach(i =>
                {
                    resultList.Add(new SearchResultModel()
                    {
                        Character = character,
                        Item = i,
                        WhereIsIt = "Bank",
                    });
                });
            });

            IEnumerable<SearchResultModel> result = resultList;

            return Task.FromResult(result);
        }

    }
}