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

        private static Dictionary<string, string> itemCodes = ItemConfig.getItemCodes();

        /// <summary>
        /// Initialize Service
        /// </summary>
        /// <param name="fileInfo">FileInfos</param>
        public void Initialize(IEnumerable<IBrowserFile> fileInfo)
        {
            FileInfos = fileInfo;
        }


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

            // decode character
            foreach (var binaryFile in FileInfos.Where(f => f.Name.Contains("Slot")))
            {
                logger.LogInformation("target charactor binary file: " + binaryFile.Name);

                string hexCharacter = await GetHexChracter(binaryFile);
                string slotNumber = GetSlotNumber(binaryFile.Name);
                // read character info
                var characterModel = new CharacterModel()
                {
                    Items = DecodeInventory(hexCharacter, slotNumber),
                    Banks = DecodeCharacterBank(hexCharacter, slotNumber),
                };
                characters.Add(characterModel);
            }
            // decode sharebank
            var shareBankFile = FileInfos.FirstOrDefault(f => !f.Name.Contains("Slot"));
            if (shareBankFile is null)
            {
                return characters;
            }

            // decode sharebank
            characters.Add(new CharacterModel()
            {
                Name = "SHARE BANK",
                Banks = DecodeShareBank(await GetHexChracter(shareBankFile), "SHARE"),
            });

            return characters;
        }

        private async Task<string> GetHexChracter(IBrowserFile file)
        {
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
            await file.OpenReadStream(maxFileSize).CopyToAsync(fileStream);
            fileStream.Flush();
            fileStream.Close();

            // read bytes
            byte[] bytes = File.ReadAllBytes(path);

            return BitConverter.ToString(bytes).Replace("-", string.Empty);
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

        private string DecodeName(string hexCharactor)
        {
            // TODO: implements
            return null;
        }

        private string DecodeRace(string hexCharactor)
        {
            // TODO: implements
            return null;
        }
        private string DecodeLevel(string hexCharactor)
        {
            // TODO: implements
            return null;
        }

        private string DecodeExperience(string hexCharactor)
        {
            // TODO: implements
            return null;
        }

        private IEnumerable<ItemModel> DecodeInventory(string hexCharactor, string slotNumber)
        {
            return DecodeItemList(hexCharactor.Substring(40, 1680), 30, 56, slotNumber);
        }

        private IEnumerable<ItemModel> DecodeCharacterBank(string hexCharactor, string slotNumber)
        {
            return DecodeItemList(hexCharactor.Substring(3600, 13200), 200, 48, slotNumber);
        }

        private IEnumerable<ItemModel> DecodeShareBank(string hexCharactor, string slotNumber)
        {
            return DecodeItemList(hexCharactor.Substring(16, 9600), 200, 48, slotNumber);
        }

        private IEnumerable<ItemModel> DecodeItemList(string inventoryRange, int max, int length, string slotNumber)
        {
            int index = 0;
            var list = new List<ItemModel>();

            // 全アイテムエリアをアイテム単位でループする。
            for (int i = 0; i < max; i++)
            {
                // アイテムを取得
                string itemRange = inventoryRange.Substring(index, length);
                // アイテムコード取得
                string itemCode = itemRange.Substring(0, 6);

                // アイテムの種類を取得（武器、鎧、テクニックなど）
                int itemType = GetItemType(itemCode);

                // 空欄チェック
                if (IsBlank(itemRange)) break;

                // アイテム情報を取得
                string item = GetItem(itemType, itemCode, itemRange);

                // 所持品のリストにアイテム情報を追加
                list.Add(new ItemModel()
                {
                    ItemCode = itemCode,
                    Item = item,
                    SlotNumber = slotNumber
                }
                );

                // アイテム情報の開始位置を次のアイテムに更新
                index += length;
            }
            return list;
        }

        private string GetItem(int itemType, string itemCode, string itemRange)
        {
            switch (itemType)
            {
                case ((int)ItemConfig.ItemType.WEAPON):
                    return Weapon(itemCode, itemRange);
                case ((int)ItemConfig.ItemType.FRAME):
                    return Armor(itemCode, itemRange);
                case ((int)ItemConfig.ItemType.BARRIER):
                    return Barrier(itemCode, itemRange);
                case ((int)ItemConfig.ItemType.UNIT):
                    return Unit(itemCode, itemRange);
                case ((int)ItemConfig.ItemType.MAG):
                    return Mag(itemCode, itemRange);
                case ((int)ItemConfig.ItemType.DISK):
                    return Disk(itemCode, itemRange);
                case ((int)ItemConfig.ItemType.SRANK):
                    return SRankWeapon(itemCode, itemRange);
                case ((int)ItemConfig.ItemType.OTHER):
                    return Other(itemCode, itemRange);
                default:
                    return $"unknown. ({itemCode}). There's a possibility that New Ephinea Item";
            }
        }

        private int GetItemType(string itemRangeCode)
        {
            int itemCode = Convert.ToInt32(itemRangeCode, 16);

            // Sランク武器は4byteまでを参照する
            if (IsSLank(Convert.ToInt32(itemRangeCode.Substring(0, 4), 16)))
            {
                return ((int)ItemConfig.ItemType.SRANK);
            }

            if (IsWeapon(itemCode))
            {
                return ((int)ItemConfig.ItemType.WEAPON);
            }

            if (IsArmor(itemCode))
            {
                return ((int)ItemConfig.ItemType.FRAME);
            }

            if (IsBarrier(itemCode))
            {
                return ((int)ItemConfig.ItemType.BARRIER);
            }

            if (IsUnit(itemCode))
            {
                return ((int)ItemConfig.ItemType.UNIT);
            }

            if (IsMag(itemCode))
            {
                return ((int)ItemConfig.ItemType.MAG);
            }

            if (IsDisk(Convert.ToInt32(itemRangeCode.Substring(0, 4), 16)))
            {
                return ((int)ItemConfig.ItemType.DISK);
            }

            return ((int)ItemConfig.ItemType.OTHER);
        }

        private bool IsBlank(string itemRange)
        {
            // アイテム情報がすべて0だった場合は空欄。倉庫の空欄は途中FFFFFFFFが入る
            return (Regex.IsMatch(itemRange.Substring(0, 39), @"^[0]+$") || Regex.IsMatch(itemRange.Substring(0, 39), @"^[0]+[F]+[0]+$"));
        }

        private bool IsWeapon(int itemCode)
        {
            return (ItemConfig.WeaponRange[0] <= itemCode && itemCode <= ItemConfig.WeaponRange[1]);
        }
        private bool IsCommonWeapon(string itemCode)
        {
            // コモン武器の最大アイテムコード以前であり、CommonWeaponsMaxCode
            return (Convert.ToInt32(itemCode, 16) <= ItemConfig.CommonWeaponContainsCode && Convert.ToInt32(itemCode.Substring(4, 2), 16) <= ItemConfig.CommonWeaponsMaxCode);

        }
        private bool IsArmor(int itemCode)
        {
            return (ItemConfig.FrameRange[0] <= itemCode && itemCode <= ItemConfig.FrameRange[1]);
        }

        private bool IsBarrier(int itemCode)
        {
            return (ItemConfig.BarrierRange[0] <= itemCode && itemCode <= ItemConfig.BarrierRange[1]);
        }
        private bool IsUnit(int itemCode)
        {
            return (ItemConfig.UnitRange[0] <= itemCode && itemCode <= ItemConfig.UnitRange[1]);
        }

        private bool IsMag(int itemCode)
        {
            return (ItemConfig.MagRange[0] <= itemCode && itemCode <= ItemConfig.MagRange[1]);
        }
        private bool IsTool(int itemCode)
        {
            return (ItemConfig.ToolRange[0] <= itemCode && itemCode <= ItemConfig.ToolRange[1]);
        }
        private bool IsDisk(int itemCode)
        {
            return (itemCode == ItemConfig.DiskCode);
        }
        private bool IsSLank(int itemCode)
        {
            return (ItemConfig.SLankWeaponRange[0] <= itemCode && itemCode <= ItemConfig.SLankWeaponRange[1]);
        }

        private string Weapon(string itemCode, string itemRange)
        {
            string name = GetName(itemCode);
            int grinder = Convert.ToInt32(itemRange.Substring(6, 2), 16);
            int native = GetNative(itemRange);
            int aBeast = GetABeast(itemRange);
            int machine = GetMachine(itemRange);
            int dark = GetDark(itemRange);
            int hit = GetHit(itemRange);

            // グラインダーが1以上の場合
            string grinderLabel = null;
            if (grinder > 0)
            {
                grinderLabel = $" +{grinder}";
            }

            // コモン武器の場合はエレメントの設定をする。
            string element = null;
            if (IsCommonWeapon(itemCode))
            {
                element = GetElement(itemRange);
                if (element != null)
                {
                    return $"{name}{grinderLabel} [{element}] [{native}/{aBeast}/{machine}/{dark}|{hit}]";
                }
            }

            // レア武器の場合はエレメント非表示
            return $"{name}{grinderLabel} [{native}/{aBeast}/{machine}/{dark}|{hit}]";
        }

        private string Armor(string itemCode, string itemRange)
        {
            string name = GetName(itemCode);
            int slot = Convert.ToInt32(itemRange.Substring(10, 2), 16);
            int def = Convert.ToInt32(itemRange.Substring(12, 2), 16);
            string defMaxAddition = GetAddition(name, ItemConfig.FramesAdditions, ((int)ItemConfig.AdditionType.DEF));
            int avoid = Convert.ToInt32(itemRange.Substring(16, 2), 16);
            string avoidMaxAddition = GetAddition(name, ItemConfig.FramesAdditions, ((int)ItemConfig.AdditionType.AVOID));

            return $"{name} [{def}/{defMaxAddition} | {avoid}/{avoidMaxAddition}] [{slot}S]";
        }
        private string Barrier(string itemCode, string itemRange)
        {
            string name = GetName(itemCode);
            int def = Convert.ToInt32(itemRange.Substring(12, 2), 16);
            string defMaxAddition = GetAddition(name, ItemConfig.ShieldAdditions, ((int)ItemConfig.AdditionType.DEF));
            int avoid = Convert.ToInt32(itemRange.Substring(16, 2), 16);
            string avoidMaxAddition = GetAddition(name, ItemConfig.ShieldAdditions, ((int)ItemConfig.AdditionType.AVOID));

            return $"{name} [{def}/{defMaxAddition} | {avoid}/{avoidMaxAddition}]";
        }
        private string Mag(string itemCode, string itemRange)
        {
            // まぐのアイテムコードは3バイト目に00を追加
            string name = GetName(itemCode.Substring(0, 4) + "00");
            int level = Convert.ToInt32(itemRange.Substring(4, 2), 16);
            int sync = Convert.ToInt32(itemRange.Substring(32, 2), 16);
            int iq = Convert.ToInt32(itemRange.Substring(34, 2), 16);
            string collor = ItemConfig.MagCollorCodes[itemRange.Substring(38, 2)];
            double def = Convert.ToInt32(itemRange.Substring(10, 2) + itemRange.Substring(8, 2), 16) / 100;
            double pow = Convert.ToInt32(itemRange.Substring(14, 2) + itemRange.Substring(12, 2), 16) / 100;
            double dex = Convert.ToInt32(itemRange.Substring(18, 2) + itemRange.Substring(16, 2), 16) / 100;
            double mind = Convert.ToInt32(itemRange.Substring(22, 2) + itemRange.Substring(20, 2), 16) / 100;

            string[] pbs = GetPbs(itemRange.Substring(6, 2) + itemRange.Substring(36, 2));

            return $"{name} LV{level} [{collor}] [{def}/{pow}/{dex}/{mind}] [{pbs[2]}|{pbs[0]}|{pbs[1]}]";
        }

        private string Unit(string itemCode, string itemRange)
        {
            string name = GetName(itemCode);
            return $"{name}";
        }
        private string Disk(string itemCode, string itemRange)
        {
            string name = ItemConfig.DiskNameCodes[itemRange.Substring(8, 2)];
            int level = Convert.ToInt32(itemRange.Substring(4, 2), 16);
            return $"{name} LV{level} disk";
        }
        private string SRankWeapon(string itemCode, string itemRange)
        {
            // S武器のアイテムコードは行頭～4桁に00を追加
            string name = GetName(itemCode.Substring(0, 4) + "00");
            int grinder = Convert.ToInt32(itemRange.Substring(6, 2), 16);
            string element = GetElement(itemRange);

            // グラインダーが1以上の場合は表示する
            string grinderLabel = null;
            if (grinder > 0)
            {
                grinderLabel = $" +{grinder}";
            }

            return $"S-RANK {name}{grinderLabel} [{element}]";
        }
        private string Other(string itemCode, string itemRange)
        {
            string name = GetName(itemCode);
            int number = Convert.ToInt32(itemRange.Substring(10, 2), 16);

            string numberLabel = null;
            if (number > 0)
            {
                numberLabel = $" x{number}";
            }

            return $"{name}{numberLabel}";
        }

        private int GetNative(string itemRange)
        {
            return GetAttribute(ItemConfig.AttributeType["native"], itemRange);
        }
        private int GetABeast(string itemRange)
        {
            return GetAttribute(ItemConfig.AttributeType["aBeast"], itemRange);
        }
        private int GetMachine(string itemRange)
        {
            return GetAttribute(ItemConfig.AttributeType["machine"], itemRange);
        }
        private int GetDark(string itemRange)
        {
            return GetAttribute(ItemConfig.AttributeType["dark"], itemRange);
        }
        private int GetHit(string itemRange)
        {
            return GetAttribute(ItemConfig.AttributeType["hit"], itemRange);
        }
        private int GetAttribute(string attributeType, string itemRange)
        {
            string[] attributes =
            {
                // ひとつめの属性値
                itemRange.Substring(12,4),
                // ふたつめの属性値
                itemRange.Substring(16,4),
                // みっつめの属性値
                itemRange.Substring(20,4),
            };

            foreach (string attribute in attributes)
            {
                if (attribute.Substring(0, 2) == attributeType)
                {
                    return SByte.Parse(attribute.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                }
            }
            return 0;
        }
        private string GetName(string itemCode)
        {
            if (itemCodes.ContainsKey(itemCode))
            {
                return itemCodes[itemCode];
            }
            return $"undefined. ({itemCode})";
        }

        private string GetElement(string itemRange)
        {
            string elementCode = itemRange.Substring(8, 2);
            if (ItemConfig.ElementCodes.ContainsKey(elementCode))
            {
                return ItemConfig.ElementCodes[elementCode];
            }

            return "undefined";
        }

        private string GetAddition(string name, Dictionary<string, int[]> additions, int type)
        {
            if (additions.ContainsKey(name))
            {
                return additions[name][type].ToString();
            }
            return "undefined";
        }
        private string[] GetPbs(string pbsCode)
        {
            if (ItemConfig.PBs.ContainsKey(pbsCode))
            {
                return ItemConfig.PBs[pbsCode];
            }

            return new string[] { "undefined", "undefined", "undefined" };
        }

    }
}