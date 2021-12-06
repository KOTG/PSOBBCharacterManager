using PSOBBCharacterDataDecoderWeb.Service.Interfaces;
using PSOBBCharactorGetter;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using PSOBBCharacterDataDecoderWeb.Model;


namespace PSOBBCharacterDataDecoderWeb.Service.Implements
{
    /// <summary>
    /// PSOBBCharacterDataService by PSOBB Binary File.
    /// </summary>
    public class PSOBBCharacterDataFileService : IPSOBBCharacterDataService
    {
        IWebHostEnvironment Environment;

        private ILogger<PSOBBCharacterDataFileService> logger;

        private IEnumerable<IBrowserFile> FileInfos;

        private long maxFileSize = 1024 * 15;

        public PSOBBCharacterDataFileService(ILogger<PSOBBCharacterDataFileService> logger, IWebHostEnvironment env)
        {
            this.logger = logger;
            this.Environment = env;
            FileInfos = new List<IBrowserFile>();
        }

        /// <summary>
        /// Get PSOBBCharacter's Data using PSOBB Binary 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IEnumerable<CharacterModel>> GetCharactors()
        {
            var characters = new List<CharacterModel>();
            foreach (var binaryFile in FileInfos)
            {
                logger.LogInformation("target charactor binary file: " + binaryFile.Name);

                Charactor character;

                var trustedFileNameForFileStorage = Path.GetRandomFileName();
                var parentPath = Path.Combine(Environment.ContentRootPath,
                        Environment.EnvironmentName, "unsafe_uploads");

                // delete existing folder (recursive)
                Directory.Delete(parentPath, true);

                // create uploading folder
                Directory.CreateDirectory(parentPath);

                var path = Path.Combine(parentPath,
                        trustedFileNameForFileStorage);

                // copy to server
                await using FileStream fileStream = File.Create(path);
                await binaryFile.OpenReadStream(maxFileSize).CopyToAsync(fileStream);
                fileStream.Flush();
                fileStream.Close();

                // read bytes
                byte[] bytes = File.ReadAllBytes(path);

                string slotNumber = GetSlotNumber(binaryFile.Name);

                // read character info
                character = new Charactor(BitConverter.ToString(bytes).Replace("-", string.Empty), slotNumber);

                characters.Add(new CharacterModel(character));
            }

            return characters;
        }

        /// <summary>
        /// Get Slot Number from fileName
        /// </summary>
        /// <param name="fileName">fileName</param>
        /// <returns>SlotNumber</returns>
        private string GetSlotNumber(string fileName)
        {
            Match match = Regex.Match(fileName, @"(?<= ).+(?=\.)");
            if (match.Success)
            {
                return match.Value;
            }

            return "unknown";
        }

        /// <summary>
        /// Initialize Service
        /// </summary>
        /// <param name="fileInfo">FileInfos</param>
        public void Initialize(IEnumerable<IBrowserFile> fileInfo)
        {
            FileInfos = fileInfo;
        }
    }
}