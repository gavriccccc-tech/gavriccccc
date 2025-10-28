using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Windows.Forms;
using System.Linq;

namespace InventoryTrackerApp
{
    public class SteamPriceService
    {
        private Dictionary<string, decimal> manualPrices;
        private Dictionary<string, decimal> webPrices;
        private Dictionary<string, DateTime> priceLastUpdated;
        private List<PriceHistory> priceHistory;
        private readonly string manualPricesFilePath;
        private readonly string webPricesFilePath;
        private readonly string priceHistoryFilePath;
        private HttpClient httpClient;
        private bool isWebAvailable = true;

        public SteamPriceService()
        {
            string appDataPath = Application.StartupPath;
            manualPricesFilePath = Path.Combine(appDataPath, "manual_prices.json");
            webPricesFilePath = Path.Combine(appDataPath, "web_prices_cache.json");
            priceHistoryFilePath = Path.Combine(appDataPath, "price_history.json"); // НОВАЯ СТРОКА

            manualPrices = new Dictionary<string, decimal>();
            webPrices = new Dictionary<string, decimal>();
            priceLastUpdated = new Dictionary<string, DateTime>();
            priceHistory = new List<PriceHistory>(); // НОВАЯ СТРОКА

            httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            LoadManualPrices();
            LoadWebPricesCache();
            LoadPriceHistory(); // НОВАЯ СТРОКА
        }

        public decimal GetItemPrice(string itemName, string game)
        {
            string cacheKey = $"{game}_{itemName}";
            decimal currentPrice = 0;
            string source = "";

            // 1. ПРИОРИТЕТ: Ручные цены (пользователь всегда прав)
            if (manualPrices.ContainsKey(cacheKey))
            {
                currentPrice = manualPrices[cacheKey];
                source = "MANUAL";
            }
            // 2. ВТОРОЙ ПРИОРИТЕТ: Кэшированные веб-цены (не старше 24 часов)
            else if (webPrices.ContainsKey(cacheKey) &&
                priceLastUpdated.ContainsKey(cacheKey) &&
                DateTime.Now - priceLastUpdated[cacheKey] < TimeSpan.FromHours(24))
            {
                currentPrice = webPrices[cacheKey];
                source = "STEAM_WEB";
            }

            // Сохраняем текущую цену в историю (если цена > 0)
            if (currentPrice > 0)
            {
                SaveCurrentPriceToHistory(itemName, game, currentPrice, source);
            }

            return currentPrice;
        }

        public string GetPriceSource(string itemName, string game)
        {
            string cacheKey = $"{game}_{itemName}";

            if (manualPrices.ContainsKey(cacheKey))
                return "РУЧНАЯ";

            if (webPrices.ContainsKey(cacheKey) &&
                priceLastUpdated.ContainsKey(cacheKey) &&
                DateTime.Now - priceLastUpdated[cacheKey] < TimeSpan.FromHours(24))
                return "STEAM WEB";

            return "НЕТ ДАННЫХ";
        }

        // Асинхронное получение цен из Steam Web API
        public async Task<decimal> FetchPriceFromWebAsync(string itemName, string game)
        {
            if (!isWebAvailable)
                return 0;

            string cacheKey = $"{game}_{itemName}";

            try
            {
                // Формируем URL для запроса
                string encodedItemName = Uri.EscapeDataString(itemName);
                string appId = GetGameAppId(game);

                if (appId == "0") // Игра не поддерживает маркет
                    return 0;

                string url = $"https://steamcommunity.com/market/priceoverview/?appid={appId}&currency=5&market_hash_name={encodedItemName}";

                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var priceData = JsonSerializer.Deserialize<SteamPriceResponse>(json);

                    if (priceData != null && priceData.success && !string.IsNullOrEmpty(priceData.lowest_price))
                    {
                        decimal price = ParseSteamPrice(priceData.lowest_price);

                        if (price > 0)
                        {
                            // Сохраняем в кэш
                            webPrices[cacheKey] = price;
                            priceLastUpdated[cacheKey] = DateTime.Now;
                            SaveWebPricesCache();

                            // Сохраняем в историю
                            SaveCurrentPriceToHistory(itemName, game, price, "STEAM_WEB");

                            Console.WriteLine($"✅ Получена цена из Steam: {itemName} - {price} руб.");
                            return price;
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"❌ Steam API вернул ошибку: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка получения цены из Steam Web для {itemName}: {ex.Message}");
                isWebAvailable = false; // Временно отключаем веб-запросы
            }

            return 0;
        }

        // Пакетное обновление цен для всего инвентаря
        public async Task RefreshAllPricesAsync(List<InventoryItem> inventory)
        {
            var refreshForm = new PriceRefreshForm(inventory.Count);
            refreshForm.Show();

            int updatedCount = 0;
            int errorCount = 0;
            int skippedCount = 0;

            foreach (var item in inventory)
            {
                if (refreshForm.IsDisposed)
                    break;

                refreshForm.UpdateProgress(item.Name, updatedCount + errorCount + skippedCount + 1);

                // Пропускаем предметы с ручными ценами
                if (manualPrices.ContainsKey($"{item.Game}_{item.Name}"))
                {
                    skippedCount++;
                    continue;
                }

                decimal webPrice = await FetchPriceFromWebAsync(item.Name, item.Game);
                if (webPrice > 0)
                    updatedCount++;
                else
                    errorCount++;

                // Задержка чтобы не заблокировали запросы
                await Task.Delay(1500);
            }

            refreshForm.Close();

            string message = $"✅ Обновлено цен: {updatedCount}";
            if (skippedCount > 0)
                message += $"\n⏭️ Пропущено (ручные цены): {skippedCount}";
            if (errorCount > 0)
                message += $"\n❌ Не удалось обновить: {errorCount} (требуют ручного ввода)";

            MessageBox.Show(message, "Обновление цен завершено",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private string GetGameAppId(string game)
        {
            // AppID популярных игр в Steam
            var gameAppIds = new Dictionary<string, string>
            {
                { "Counter-Strike 2", "730" },
                { "Dota 2", "570" },
                { "PUBG: BATTLEGROUNDS", "578080" },
                { "Rust", "252490" },
                { "Team Fortress 2", "440" },
                { "Apex Legends", "1172470" },
                { "Call of Duty", "1938090" },
                { "Escape from Tarkov", "0" } // Нет в Steam Marketplace
            };

            return gameAppIds.ContainsKey(game) ? gameAppIds[game] : "0";
        }

        private decimal ParseSteamPrice(string priceText)
        {
            try
            {
                if (string.IsNullOrEmpty(priceText))
                    return 0;

                // Формат: "123,45 руб." или "1.200,67 руб." или "$12.34"
                string cleanText = priceText
                    .Replace(" руб.", "")
                    .Replace(" pуб.", "") // разные варианты написания
                    .Replace(" RUB", "")
                    .Replace(" ", "")
                    .Replace("$", "")
                    .Replace("€", "")
                    .Replace(".", "") // убираем разделители тысяч
                    .Replace(",", "."); // заменяем запятую на точку для decimal

                if (decimal.TryParse(cleanText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal price))
                    return price;

                Console.WriteLine($"⚠️ Не удалось распарсить цену: {priceText}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка парсинга цены '{priceText}': {ex.Message}");
            }

            return 0;
        }

        // Загрузка и сохранение кэша веб-цен
        private void LoadWebPricesCache()
        {
            try
            {
                if (File.Exists(webPricesFilePath))
                {
                    string json = File.ReadAllText(webPricesFilePath);
                    var cacheData = JsonSerializer.Deserialize<WebPriceCache>(json);

                    // ИСПРАВЛЕНИЕ: Явное приведение типов для совместимости с C# 7.3
                    if (cacheData != null)
                    {
                        webPrices = cacheData.Prices ?? new Dictionary<string, decimal>();
                        priceLastUpdated = cacheData.LastUpdated ?? new Dictionary<string, DateTime>();
                    }
                    else
                    {
                        webPrices = new Dictionary<string, decimal>();
                        priceLastUpdated = new Dictionary<string, DateTime>();
                    }

                    Console.WriteLine($"✅ Загружено {webPrices.Count} кэшированных веб-цен");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка загрузки кэша цен: {ex.Message}");
                webPrices = new Dictionary<string, decimal>();
                priceLastUpdated = new Dictionary<string, DateTime>();
            }
        }

        private void SaveWebPricesCache()
        {
            try
            {
                var cacheData = new WebPriceCache
                {
                    Prices = webPrices,
                    LastUpdated = priceLastUpdated
                };

                string json = JsonSerializer.Serialize(cacheData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(webPricesFilePath, json);
                Console.WriteLine($"💾 Сохранено {webPrices.Count} веб-цен в кэш");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка сохранения кэша цен: {ex.Message}");
            }
        }

        // НОВЫЕ МЕТОДЫ ДЛЯ РАБОТЫ С ИСТОРИЕЙ ЦЕН
        private void LoadPriceHistory()
        {
            try
            {
                if (File.Exists(priceHistoryFilePath))
                {
                    string json = File.ReadAllText(priceHistoryFilePath);
                    priceHistory = JsonSerializer.Deserialize<List<PriceHistory>>(json) ?? new List<PriceHistory>();
                    Console.WriteLine($"✅ Загружено {priceHistory.Count} записей истории цен");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка загрузки истории цен: {ex.Message}");
                priceHistory = new List<PriceHistory>();
            }
        }

        private void SavePriceHistory()
        {
            try
            {
                string json = JsonSerializer.Serialize(priceHistory, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(priceHistoryFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка сохранения истории цен: {ex.Message}");
            }
        }

        // Метод для сохранения текущей цены в историю
        public void SaveCurrentPriceToHistory(string itemName, string game, decimal price, string source)
        {
            try
            {
                string cacheKey = $"{game}_{itemName}";
                var today = DateTime.Today;

                // Удаляем старую запись за сегодня (если есть)
                priceHistory.RemoveAll(ph =>
                    ph.ItemName == itemName &&
                    ph.Game == game &&
                    ph.Date.Date == today);

                // Добавляем новую запись
                priceHistory.Add(new PriceHistory
                {
                    ItemName = itemName,
                    Game = game,
                    Price = price,
                    Date = DateTime.Now,
                    Source = source
                });

                // Сохраняем историю
                SavePriceHistory();
                Console.WriteLine($"💾 Сохранена цена в историю: {itemName} - {price} руб.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка сохранения цены в историю: {ex.Message}");
            }
        }

        // Метод для получения вчерашней цены
        public decimal GetYesterdayPrice(string itemName, string game)
        {
            try
            {
                var yesterday = DateTime.Today.AddDays(-1);

                var yesterdayPrice = priceHistory
                    .Where(ph => ph.ItemName == itemName && ph.Game == game && ph.Date.Date == yesterday)
                    .OrderByDescending(ph => ph.Date)
                    .FirstOrDefault();

                return yesterdayPrice?.Price ?? 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка получения вчерашней цены для {itemName}: {ex.Message}");
                return 0;
            }
        }

        // Метод для получения цены за определенную дату
        public decimal GetPriceForDate(string itemName, string game, DateTime date)
        {
            try
            {
                var priceForDate = priceHistory
                    .Where(ph => ph.ItemName == itemName && ph.Game == game && ph.Date.Date == date.Date)
                    .OrderByDescending(ph => ph.Date)
                    .FirstOrDefault();

                return priceForDate?.Price ?? 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка получения цены за {date.ToShortDateString()} для {itemName}: {ex.Message}");
                return 0;
            }
        }

        public List<PriceHistory> GetPriceHistory(string itemName, string game)
        {
            return priceHistory
                .Where(ph => ph.ItemName == itemName && ph.Game == game)
                .OrderByDescending(ph => ph.Date)
                .ToList();
        }

        public List<PriceHistory> GetAllPriceHistory()
        {
            return new List<PriceHistory>(priceHistory);
        }

        // Методы для ручных цен
        public void SetManualPrice(string itemName, string game, decimal price)
        {
            string cacheKey = $"{game}_{itemName}";
            manualPrices[cacheKey] = price;
            SaveManualPrices();

            // Сохраняем в историю
            SaveCurrentPriceToHistory(itemName, game, price, "MANUAL");

            Console.WriteLine($"💾 Сохранена ручная цена: {itemName} - {price} руб.");
        }

        public decimal? GetManualPrice(string itemName, string game)
        {
            string cacheKey = $"{game}_{itemName}";
            return manualPrices.ContainsKey(cacheKey) ? manualPrices[cacheKey] : (decimal?)null;
        }

        public void RemoveManualPrice(string itemName, string game)
        {
            string cacheKey = $"{game}_{itemName}";
            if (manualPrices.ContainsKey(cacheKey))
            {
                manualPrices.Remove(cacheKey);
                SaveManualPrices();
                Console.WriteLine($"🗑️ Удалена ручная цена: {itemName}");
            }
        }

        public Dictionary<string, decimal> GetAllManualPrices()
        {
            return new Dictionary<string, decimal>(manualPrices);
        }

        public List<string> GetItemsRequiringManualInput(List<InventoryItem> inventory)
        {
            var result = new List<string>();

            foreach (var item in inventory)
            {
                string cacheKey = $"{item.Game}_{item.Name}";
                decimal price = GetItemPrice(item.Name, item.Game);

                if (price == 0 && !manualPrices.ContainsKey(cacheKey))
                {
                    result.Add($"{item.Game} - {item.Name}");
                }
            }

            return result;
        }

        private void LoadManualPrices()
        {
            try
            {
                if (File.Exists(manualPricesFilePath))
                {
                    string json = File.ReadAllText(manualPricesFilePath);
                    // ИСПРАВЛЕНИЕ: Явное приведение типов
                    var loadedPrices = JsonSerializer.Deserialize<Dictionary<string, decimal>>(json);
                    manualPrices = loadedPrices ?? new Dictionary<string, decimal>();
                    Console.WriteLine($"✅ Загружено {manualPrices.Count} ручных цен");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка загрузки ручных цен: {ex.Message}");
                manualPrices = new Dictionary<string, decimal>();
            }
        }

        private void SaveManualPrices()
        {
            try
            {
                string json = JsonSerializer.Serialize(manualPrices, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(manualPricesFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка сохранения ручных цен: {ex.Message}");
            }
        }

        public void ClearAllManualPrices()
        {
            manualPrices.Clear();
            SaveManualPrices();
            Console.WriteLine("🗑️ Очищены все ручные цены");
        }

        public decimal CalculatePotentialProfit(InventoryItem item, decimal currentPrice)
        {
            if (item.Quantity == 0) return 0;

            decimal steamCommission = 0.13m;
            decimal netSalePrice = currentPrice * (1 - steamCommission);
            decimal averagePurchasePrice = item.Quantity > 0 ? item.TotalPurchase / item.Quantity : 0;
            decimal profitPerItem = netSalePrice - averagePurchasePrice;

            return Math.Round(profitPerItem * item.Quantity, 2);
        }

        public string GetTradingRecommendation(InventoryItem item, decimal currentPrice)
        {
            if (item.Quantity == 0) return "Нет в наличии";

            decimal potentialProfit = CalculatePotentialProfit(item, currentPrice);
            decimal averagePurchasePrice = item.Quantity > 0 ? item.TotalPurchase / item.Quantity : 0;
            decimal profitPercentage = averagePurchasePrice > 0 ?
                (potentialProfit / (averagePurchasePrice * item.Quantity)) * 100 : 0;

            if (profitPercentage > 20)
                return "🚀 ВЫСОКАЯ ПРИБЫЛЬ - продавать!";
            else if (profitPercentage > 5)
                return "📈 ХОРОШАЯ ПРИБЫЛЬ -可以考虑 продать";
            else if (profitPercentage > -5)
                return "⚪ НЕЙТРАЛЬНО - держать";
            else if (profitPercentage > -20)
                return "📉 НЕБОЛЬШОЙ УБЫТОК - подождать";
            else
                return "🔴 БОЛЬШОЙ УБЫТОК - не продавать";
        }
    }

    // Модели для Steam Web API
    public class SteamPriceResponse
    {
        public bool success { get; set; }
        public string lowest_price { get; set; }
        public string median_price { get; set; }
        public string volume { get; set; }
    }

    public class WebPriceCache
    {
        public Dictionary<string, decimal> Prices { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, DateTime> LastUpdated { get; set; } = new Dictionary<string, DateTime>();
    }
}