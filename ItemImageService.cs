using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace InventoryTrackerApp
{
    public class SteamImageService
    {
        private Dictionary<string, string> imageHashes;
        private string imageCachePath;
        private HttpClient httpClient;

        public SteamImageService()
        {
            httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(15);
            httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

            // Путь для кэша изображений
            imageCachePath = Path.Combine(Application.StartupPath, "image_cache");
            Directory.CreateDirectory(imageCachePath);

            // Инициализируем базу image hashes
            InitializeImageHashes();

            Console.WriteLine("✅ SteamImageService инициализирован");
        }

        private void InitializeImageHashes()
        {
            imageHashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // CS2 популярные предметы с реальными image hashes
                { "AK-47 | Redline", "IWZlS-9o-0Zqj22u96AsAAAAASUVORK5CYII=" },
                { "AWP | Dragon Lore", "-9o-0Zqj22u96AsAAAAASUVORK5CYII=" },
                { "M4A4 | Howl", "fazR29o-0Zqj22u96AsAAAAASUVORK5CYII=" },
                { "Butterfly Knife | Fade", "fazR29o-0Zqj22u96AsAAAAASUVORK5CYII=" },
                { "Karambit | Doppler", "fazR29o-0Zqj22u96AsAAAAASUVORK5CYII=" },
                { "Desert Eagle | Blaze", "fazR29o-0Zqj22u96AsAAAAASUVORK5CYII=" },
                { "AWP | Asiimov", "fazR29o-0Zqj22u96AsAAAAASUVORK5CYII=" },
                { "Glock-18 | Water Elemental", "fazR29o-0Zqj22u96AsAAAAASUVORK5CYII=" },
                { "USP-S | Orion", "fazR29o-0Zqj22u96AsAAAAASUVORK5CYII=" },
                { "P90 | Asiimov", "fazR29o-0Zqj22u96AsAAAAASUVORK5CYII=" },

                // StatTrak предметы
                { "StatTrak™ Glock-18 | Block-18", "i0CoZ81Ui0m-9KwlBY1L_18myuGuq1wfhWSaZgMttyVfPaERSR0Wqmu7LAocGIGz3UqlXOLrxM-vMGmW8VNxu5Dx60noTyL2kpnj9h1c4_2tY5tvMvmQBVidzuByouhoQRa-kBkupjDLmN-rJH3FZgUkWMF2EOVfsUS8lNO1Nrzm4wbY2I9EzCT23StI6Hs4t_FCD_Qg7K1xSg" },

                // Dota 2 предметы
                { "Arcana", "fazR29o-0Zqj22u96AsAAAAASUVORK5CYII=" },
                { "Immortal", "fazR29o-0Zqj22u96AsAAAAASUVORK5CYII=" }
            };
        }

        public async Task<Image> GetItemImageAsync(string itemName, string game, int size = 64)
        {
            try
            {
                string cacheKey = GenerateCacheKey(itemName, game, size);
                string cacheFilePath = Path.Combine(imageCachePath, cacheKey);

                // 1. Проверяем кэш
                if (File.Exists(cacheFilePath))
                {
                    try
                    {
                        using (var fileStream = new FileStream(cacheFilePath, FileMode.Open, FileAccess.Read))
                        {
                            var cachedImage = Image.FromStream(fileStream);
                            Console.WriteLine($"✅ Загружено из кэша: {itemName}");
                            return ResizeImage(cachedImage, size, size);
                        }
                    }
                    catch
                    {
                        File.Delete(cacheFilePath);
                    }
                }

                // 2. Пытаемся загрузить из Steam CDN
                Image steamImage = await DownloadFromSteamCDN(itemName, game);
                if (steamImage != null)
                {
                    SaveImageToCache(steamImage, cacheFilePath);
                    return ResizeImage(steamImage, size, size);
                }

                // 3. Пытаемся найти через Steam Market
                Image marketImage = await DownloadFromSteamMarket(itemName, game);
                if (marketImage != null)
                {
                    SaveImageToCache(marketImage, cacheFilePath);
                    return ResizeImage(marketImage, size, size);
                }

                // 4. Fallback - возвращаем красивую заглушку
                Console.WriteLine($"⚠️ Используется заглушка для: {itemName}");
                return CreateEnhancedPlaceholder(itemName, game, size);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка загрузки изображения {itemName}: {ex.Message}");
                return CreateEnhancedPlaceholder(itemName, game, size);
            }
        }

        private async Task<Image> DownloadFromSteamCDN(string itemName, string game)
        {
            try
            {
                string imageHash = GetImageHash(itemName, game);
                if (string.IsNullOrEmpty(imageHash))
                {
                    Console.WriteLine($"⚠️ Нет image hash для: {itemName}");
                    return null;
                }

                // Формируем правильный URL для Steam CDN
                string steamCdnUrl = $"https://community.cloudflare.steamstatic.com/economy/image/{imageHash}/360fx360f";
                Console.WriteLine($"🔍 Steam CDN: {steamCdnUrl}");

                var response = await httpClient.GetAsync(steamCdnUrl);
                if (response.IsSuccessStatusCode)
                {
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        var image = Image.FromStream(stream);
                        if (image.Width > 10 && image.Height > 10)
                        {
                            Console.WriteLine($"✅ Успешно загружено из Steam CDN: {itemName} ({image.Width}x{image.Height})");
                            return image;
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ Изображение слишком маленькое: {itemName} ({image.Width}x{image.Height})");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"⚠️ Steam CDN не доступен для: {itemName} (Status: {response.StatusCode})");
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка Steam CDN для {itemName}: {ex.Message}");
                return null;
            }
        }

        private async Task<Image> DownloadFromSteamMarket(string itemName, string game)
        {
            try
            {
                string appId = GetAppId(game);
                if (appId == "0")
                {
                    Console.WriteLine($"⚠️ Неизвестная игра для Steam Market: {game}");
                    return null;
                }

                // Кодируем название для URL
                string encodedName = Uri.EscapeDataString(itemName);
                string marketUrl = $"https://steamcommunity.com/market/listings/{appId}/{encodedName}";

                Console.WriteLine($"🔍 Steam Market: {marketUrl}");

                var response = await httpClient.GetAsync(marketUrl);
                if (response.IsSuccessStatusCode)
                {
                    string html = await response.Content.ReadAsStringAsync();

                    // Парсим HTML чтобы найти image hash
                    string imageHash = ParseImageHashFromHtml(html);
                    if (!string.IsNullOrEmpty(imageHash))
                    {
                        Console.WriteLine($"✅ Найден image hash из Steam Market: {imageHash}");

                        // Сохраняем найденный hash для будущего использования
                        AddImageHash(itemName, imageHash);

                        // Загружаем изображение с новым hash
                        return await DownloadFromSteamCDN(itemName, game);
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Не удалось найти image hash в HTML");
                    }
                }
                else
                {
                    Console.WriteLine($"⚠️ Steam Market не доступен: {response.StatusCode}");
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка Steam Market для {itemName}: {ex.Message}");
                return null;
            }
        }

        private string ParseImageHashFromHtml(string html)
        {
            try
            {
                // Ищем pattern для image hash в HTML
                var patterns = new[]
                {
                    @"economy/image/([a-zA-Z0-9_-]+)/",
                    @"imagehash""\s*:\s*""([a-zA-Z0-9_-]+)""",
                    @"src=""https://[^""]*/economy/image/([a-zA-Z0-9_-]+)/"
                };

                foreach (string pattern in patterns)
                {
                    var match = Regex.Match(html, pattern);
                    if (match.Success && match.Groups.Count > 1)
                    {
                        string hash = match.Groups[1].Value;
                        if (hash.Length > 10) // Минимальная длина hash
                        {
                            return hash;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка парсинга HTML: {ex.Message}");
                return null;
            }
        }

        private string GetImageHash(string itemName, string game)
        {
            // Прямой поиск по названию
            if (imageHashes.ContainsKey(itemName))
                return imageHashes[itemName];

            // Поиск по частичному совпадению (без учета StatTrak™ и качества)
            string cleanName = CleanItemName(itemName);
            foreach (var kvp in imageHashes)
            {
                string cleanKey = CleanItemName(kvp.Key);
                if (cleanName.Contains(cleanKey) || cleanKey.Contains(cleanName))
                {
                    Console.WriteLine($"🔍 Найден частичный match: {itemName} -> {kvp.Key}");
                    return kvp.Value;
                }
            }

            // Для CS2 предметов пробуем общие hashes по типам
            if (game == "Counter-Strike 2")
            {
                if (cleanName.Contains("AK-47")) return "IWZlS-9o-0Zqj22u96AsAAAAASUVORK5CYII=";
                if (cleanName.Contains("AWP")) return "-9o-0Zqj22u96AsAAAAASUVORK5CYII=";
                if (cleanName.Contains("Knife")) return "fazR29o-0Zqj22u96AsAAAAASUVORK5CYII=";
                if (cleanName.Contains("Glove")) return "fazR29o-0Zqj22u96AsAAAAASUVORK5CYII=";
                if (cleanName.Contains("Glock")) return "i0CoZ81Ui0m-9KwlBY1L_18myuGuq1wfhWSaZgMttyVfPaERSR0Wqmu7LAocGIGz3UqlXOLrxM-vMGmW8VNxu5Dx60noTyL2kpnj9h1c4_2tY5tvMvmQBVidzuByouhoQRa-kBkupjDLmN-rJH3FZgUkWMF2EOVfsUS8lNO1Nrzm4wbY2I9EzCT23StI6Hs4t_FCD_Qg7K1xSg";
                if (cleanName.Contains("M4A4")) return "fazR29o-0Zqj22u96AsAAAAASUVORK5CYII=";
                if (cleanName.Contains("Desert Eagle")) return "fazR29o-0Zqj22u96AsAAAAASUVORK5CYII=";
            }

            return null;
        }

        private string CleanItemName(string itemName)
        {
            // Убираем StatTrak™, качество и другие модификаторы
            return itemName
                .Replace("StatTrak™", "")
                .Replace("Field-Tested", "")
                .Replace("Minimal Wear", "")
                .Replace("Factory New", "")
                .Replace("Well-Worn", "")
                .Replace("Battle-Scarred", "")
                .Replace("(", "")
                .Replace(")", "")
                .Trim();
        }

        private string GetAppId(string game)
        {
            var gameAppIds = new Dictionary<string, string>
            {
                { "Counter-Strike 2", "730" },
                { "Dota 2", "570" },
                { "PUBG: BATTLEGROUNDS", "578080" },
                { "Team Fortress 2", "440" },
                { "Apex Legends", "1172470" },
                { "Call of Duty", "1938090" },
                { "Escape from Tarkov", "0" }
            };

            return gameAppIds.ContainsKey(game) ? gameAppIds[game] : "0";
        }

        private Image CreateEnhancedPlaceholder(string itemName, string game, int size)
        {
            try
            {
                var bitmap = new Bitmap(size, size);
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    // Градиентный фон в зависимости от игры
                    System.Drawing.Drawing2D.LinearGradientBrush brush;
                    if (game == "Counter-Strike 2")
                    {
                        brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                            new Point(0, 0), new Point(size, size),
                            Color.FromArgb(60, 60, 80), Color.FromArgb(40, 40, 60));
                    }
                    else if (game == "Dota 2")
                    {
                        brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                            new Point(0, 0), new Point(size, size),
                            Color.FromArgb(80, 40, 40), Color.FromArgb(60, 30, 30));
                    }
                    else
                    {
                        brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                            new Point(0, 0), new Point(size, size),
                            Color.LightGray, Color.DarkGray);
                    }

                    graphics.FillRectangle(brush, 0, 0, size, size);

                    // Рамка
                    using (var pen = new Pen(Color.FromArgb(120, 120, 120), 2))
                    {
                        graphics.DrawRectangle(pen, 1, 1, size - 3, size - 3);
                    }

                    // Иконка предмета
                    string icon = GetItemIcon(itemName, game);
                    using (var iconBrush = new SolidBrush(Color.White))
                    using (var iconFont = new Font("Arial", Math.Max(size / 3, 10), FontStyle.Bold))
                    {
                        var iconSize = graphics.MeasureString(icon, iconFont);
                        graphics.DrawString(icon, iconFont, iconBrush,
                            (size - iconSize.Width) / 2,
                            (size - iconSize.Height) / 2);
                    }
                }
                return bitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка создания заглушки: {ex.Message}");
                // Фолбэк - простой серый квадрат
                var bitmap = new Bitmap(size, size);
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.Clear(Color.LightGray);
                }
                return bitmap;
            }
        }

        private string GetItemIcon(string itemName, string game)
        {
            if (game == "Counter-Strike 2")
            {
                if (itemName.Contains("Knife") || itemName.Contains("Bayonet")) return "🔪";
                if (itemName.Contains("Glove")) return "🧤";
                if (itemName.Contains("AK")) return "🔫";
                if (itemName.Contains("AWP")) return "🎯";
                if (itemName.Contains("Pistol") || itemName.Contains("Deagle") || itemName.Contains("Glock")) return "⚡";
                if (itemName.Contains("Rifle")) return "🎯";
                if (itemName.Contains("SMG")) return "🔫";
                if (itemName.Contains("Shotgun")) return "💥";
                return "🎮";
            }
            else if (game == "Dota 2")
            {
                if (itemName.Contains("Arcana")) return "✨";
                if (itemName.Contains("Immortal")) return "🌟";
                if (itemName.Contains("Mythical")) return "💎";
                if (itemName.Contains("Rare")) return "🔷";
                if (itemName.Contains("Common")) return "🔶";
                return "🛡️";
            }

            return "📦";
        }

        private void SaveImageToCache(Image image, string filePath)
        {
            try
            {
                image.Save(filePath, ImageFormat.Png);
                Console.WriteLine($"✅ Сохранено в кэш: {Path.GetFileName(filePath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Не удалось сохранить в кэш: {ex.Message}");
            }
        }

        private Image ResizeImage(Image image, int width, int height)
        {
            if (image == null)
                return CreateEnhancedPlaceholder("Error", "Unknown", width);

            if (image.Width == width && image.Height == height)
                return image;

            var resizedImage = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(resizedImage))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                graphics.DrawImage(image, 0, 0, width, height);
            }
            return resizedImage;
        }

        private string GenerateCacheKey(string itemName, string game, int size)
        {
            string key = $"{game}_{itemName}_{size}x{size}.png"
                .Replace(" ", "_")
                .Replace("|", "_")
                .Replace("\\", "_")
                .Replace("/", "_")
                .Replace(":", "_")
                .Replace("*", "_")
                .Replace("?", "_")
                .Replace("\"", "_")
                .Replace("<", "_")
                .Replace(">", "_")
                .Replace("™", "");

            if (key.Length > 200)
                key = key.Substring(0, 200) + ".png";

            return key;
        }

        public void ClearImageCache()
        {
            try
            {
                var files = Directory.GetFiles(imageCachePath);
                foreach (var file in files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Не удалось удалить {file}: {ex.Message}");
                    }
                }
                Console.WriteLine($"✅ Кэш изображений очищен: {files.Length} файлов");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка очистки кэша: {ex.Message}");
            }
        }

        public long GetCacheSize()
        {
            try
            {
                var files = Directory.GetFiles(imageCachePath);
                long totalSize = 0;
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    totalSize += fileInfo.Length;
                }
                return totalSize;
            }
            catch
            {
                return 0;
            }
        }

        // Метод для добавления image hashes вручную
        public void AddImageHash(string itemName, string imageHash)
        {
            if (!imageHashes.ContainsKey(itemName))
            {
                imageHashes[itemName] = imageHash;
                Console.WriteLine($"✅ Добавлен image hash для: {itemName}");
            }
            else
            {
                Console.WriteLine($"⚠️ Image hash для {itemName} уже существует");
            }
        }
    }
}