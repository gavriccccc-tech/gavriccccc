using System;
using System.Collections.Generic;
using System.Drawing; // ДОБАВИТЬ ЭТУ СТРОКУ
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventoryTrackerApp
{
    public class DataService
    {
        private List<Transaction> transactions;
        private List<InventoryItem> inventory;
        private List<string> customItems;
        private SteamPriceService steamPriceService;
        private SteamImageService imageService;

        private readonly string dataFilePath;
        private readonly string backupFolderPath;
        private readonly string customItemsFilePath;

        // Комиссия Steam 13%
        private const decimal STEAM_COMMISSION = 0.13m;

        public DataService()
        {
            string appDataPath = Application.StartupPath;
            dataFilePath = Path.Combine(appDataPath, "inventory_data.json");
            backupFolderPath = Path.Combine(appDataPath, "backups");
            customItemsFilePath = Path.Combine(appDataPath, "custom_items.txt");

            transactions = new List<Transaction>();
            inventory = new List<InventoryItem>();
            customItems = new List<string>();
            steamPriceService = new SteamPriceService();
            imageService = new SteamImageService(); // ЗАМЕНИЛИ НА SteamImageService

            Directory.CreateDirectory(backupFolderPath);
            LoadData();
            LoadCustomItems();
        }

        #region File Operations

        private void LoadData()
        {
            try
            {
                if (File.Exists(dataFilePath))
                {
                    string json = File.ReadAllText(dataFilePath);
                    var data = JsonSerializer.Deserialize<AppData>(json);

                    transactions = data.Transactions ?? new List<Transaction>();
                    inventory = data.Inventory ?? new List<InventoryItem>();

                    // Восстанавливаем партии из транзакций
                    RebuildInventoryFromTransactions();

                    Console.WriteLine($"✅ Загружено {transactions.Count} сделок и {inventory.Count} предметов");
                }
                else
                {
                    LoadSampleData();
                    SaveData();
                    Console.WriteLine("✅ Загружены демо-данные");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}\nЗагружаем демо-данные.", "Ошибка");
                LoadSampleData();
            }
        }

        // ПОЛНОСТЬЮ ПЕРЕСТРАИВАЕМ ИНВЕНТАРЬ ИЗ ТРАНЗАКЦИЙ
        private void RebuildInventoryFromTransactions()
        {
            // Очищаем текущий инвентарь
            inventory.Clear();

            // Группируем транзакции по предметам
            var itemsTransactions = transactions
                .GroupBy(t => new { t.Game, t.Item })
                .ToList();

            foreach (var itemGroup in itemsTransactions)
            {
                var game = itemGroup.Key.Game;
                var itemName = itemGroup.Key.Item;

                // Создаем новый предмет инвентаря
                var inventoryItem = new InventoryItem
                {
                    Game = game,
                    Name = itemName,
                    Quantity = 0,
                    TotalPurchase = 0,
                    TotalSale = 0,
                    Batches = new List<InventoryBatch>()
                };

                // Обрабатываем все транзакции по порядку даты
                var sortedTransactions = itemGroup.OrderBy(t => t.Date).ToList();

                foreach (var transaction in sortedTransactions)
                {
                    ProcessTransactionForInventory(inventoryItem, transaction);
                }

                // Пересчитываем общую стоимость покупок
                inventoryItem.TotalPurchase = inventoryItem.Batches.Sum(b => b.Total);

                // Добавляем в инвентарь только если есть остаток или были продажи
                if (inventoryItem.Quantity > 0 || inventoryItem.TotalSale > 0)
                {
                    inventory.Add(inventoryItem);
                }
            }
        }

        // ОБРАБОТКА ОДНОЙ ТРАНЗАКЦИИ ДЛЯ ИНВЕНТАРЯ
        private void ProcessTransactionForInventory(InventoryItem inventoryItem, Transaction transaction)
        {
            switch (transaction.Operation)
            {
                case "Покупка":
                    ProcessPurchase(inventoryItem, transaction);
                    break;
                case "Продажа":
                    ProcessSale(inventoryItem, transaction);
                    break;
                case "Подарок":
                case "Обмен":
                case "Крафт":
                    ProcessOtherOperation(inventoryItem, transaction);
                    break;
            }
        }

        private void ProcessPurchase(InventoryItem inventoryItem, Transaction transaction)
        {
            // Создаем новую партию
            var batch = new InventoryBatch
            {
                BatchId = transaction.Id,
                PurchaseDate = transaction.Date,
                Quantity = transaction.Quantity,
                Price = transaction.Price
            };

            inventoryItem.Batches.Add(batch);
            inventoryItem.Quantity += transaction.Quantity;
            inventoryItem.TotalPurchase += transaction.Total;
        }

        private void ProcessSale(InventoryItem inventoryItem, Transaction transaction)
        {
            int remainingToSell = transaction.Quantity;
            decimal totalCost = 0;

            // Продаем по FIFO из самых старых партий
            foreach (var batch in inventoryItem.Batches.OrderBy(b => b.PurchaseDate).ToList())
            {
                if (remainingToSell <= 0) break;

                if (batch.Quantity > 0)
                {
                    int sellFromBatch = Math.Min(batch.Quantity, remainingToSell);
                    totalCost += sellFromBatch * batch.Price;

                    batch.Quantity -= sellFromBatch;
                    remainingToSell -= sellFromBatch;
                    inventoryItem.Quantity -= sellFromBatch;

                    // Удаляем пустые партии
                    if (batch.Quantity == 0)
                    {
                        inventoryItem.Batches.Remove(batch);
                    }
                }
            }

            // Сохраняем общую сумму продажи
            inventoryItem.TotalSale += transaction.Total;
        }

        // В DataService.cs обнови метод ProcessOtherOperation:

        private void ProcessOtherOperation(InventoryItem inventoryItem, Transaction transaction)
        {
            // ПЕРЕПИСАННАЯ ЛОГИКА: для подарков, обменов и крафтов мы ПОЛУЧАЕМ предметы
            if (transaction.Quantity > 0) // Положительное количество = получаем предметы
            {
                var batch = new InventoryBatch
                {
                    BatchId = transaction.Id,
                    PurchaseDate = transaction.Date,
                    Quantity = transaction.Quantity,
                    Price = 0 // Нулевая стоимость для подарков/обменов
                };

                inventoryItem.Batches.Add(batch);
                inventoryItem.Quantity += transaction.Quantity;
                // TotalPurchase не увеличиваем, так как это подарок/обмен
            }
            else if (transaction.Quantity < 0) // Отрицательное количество = отдаем предметы (редкий случай)
            {
                // Обрабатываем отдачу предметов по FIFO
                int remainingToRemove = Math.Abs(transaction.Quantity);

                foreach (var batch in inventoryItem.Batches.OrderBy(b => b.PurchaseDate).ToList())
                {
                    if (remainingToRemove <= 0) break;

                    if (batch.Quantity > 0)
                    {
                        int removeFromBatch = Math.Min(batch.Quantity, remainingToRemove);
                        batch.Quantity -= removeFromBatch;
                        remainingToRemove -= removeFromBatch;
                        inventoryItem.Quantity -= removeFromBatch;

                        // Удаляем пустые партии
                        if (batch.Quantity == 0)
                        {
                            inventoryItem.Batches.Remove(batch);
                        }
                    }
                }
            }
        }

        public void SaveData()
        {
            try
            {
                CreateBackup();

                var data = new AppData
                {
                    Transactions = transactions,
                    Inventory = inventory,
                    LastSave = DateTime.Now
                };

                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(dataFilePath, json);

                Console.WriteLine($"✅ Данные сохранены: {transactions.Count} сделок, {inventory.Count} предметов");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения данных: {ex.Message}", "Ошибка");
            }
        }

        private void CreateBackup()
        {
            try
            {
                if (!File.Exists(dataFilePath)) return;

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFile = Path.Combine(backupFolderPath, $"backup_inventory_{timestamp}.json");
                File.Copy(dataFilePath, backupFile, true);
                CleanupOldBackups();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания бэкапа: {ex.Message}");
            }
        }

        private void CleanupOldBackups()
        {
            try
            {
                var backupFiles = Directory.GetFiles(backupFolderPath, "backup_inventory_*.json")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .ToList();

                for (int i = 10; i < backupFiles.Count; i++)
                {
                    backupFiles[i].Delete();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка очистки бэкапов: {ex.Message}");
            }
        }

        private void LoadCustomItems()
        {
            try
            {
                if (File.Exists(customItemsFilePath))
                {
                    var lines = File.ReadAllLines(customItemsFilePath);
                    customItems.Clear();
                    customItems.AddRange(lines.Where(line => !string.IsNullOrWhiteSpace(line)));
                }
                else
                {
                    customItems.AddRange(new string[] {
                        "AK-47 | Redline",
                        "AWP | Dragon Lore",
                        "M4A4 | Howl",
                        "Arcana | Lina",
                        "Butterfly Knife | Fade",
                        "Gloves | Sport"
                    });
                    SaveCustomItems();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки кастомных предметов: {ex.Message}", "Ошибка");
            }
        }

        private void SaveCustomItems()
        {
            try
            {
                File.WriteAllLines(customItemsFilePath, customItems);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения кастомных предметов: {ex.Message}", "Ошибка");
            }
        }

        #endregion

        #region Transaction Methods

        public void AddTransaction(Transaction transaction)
        {
            try
            {
                transaction.Id = Guid.NewGuid().ToString();
                transaction.Date = DateTime.Now;
                transactions.Add(transaction);

                // ПЕРЕСТРАИВАЕМ ВЕСЬ ИНВЕНТАРЬ ЗАНОВО
                RebuildInventoryFromTransactions();

                SaveData();

                Console.WriteLine($"✅ Добавлена сделка: {transaction.Game} | {transaction.Item} | {transaction.Operation} | {transaction.Quantity} шт.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления сделки: {ex.Message}", "Ошибка");
            }
        }

        public bool RemoveTransaction(string transactionId)
        {
            try
            {
                var transactionToRemove = transactions.FirstOrDefault(t => t.Id == transactionId);
                if (transactionToRemove != null)
                {
                    transactions.Remove(transactionToRemove);

                    // ПОЛНОСТЬЮ ПЕРЕСТРАИВАЕМ ИНВЕНТАРЬ
                    RebuildInventoryFromTransactions();

                    SaveData();

                    Console.WriteLine($"✅ Удалена сделка: {transactionToRemove.Game} | {transactionToRemove.Item}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления сделки: {ex.Message}", "Ошибка");
                return false;
            }
        }

        #endregion

        #region Inventory Methods

        public List<InventoryItem> GetInventory()
        {
            return new List<InventoryItem>(inventory);
        }

        public List<InventoryBatch> GetItemBatches(string game, string itemName)
        {
            var item = inventory.FirstOrDefault(i => i.Game == game && i.Name == itemName);
            return item?.Batches.Where(b => b.Quantity > 0).OrderBy(b => b.PurchaseDate).ToList() ?? new List<InventoryBatch>();
        }

        public bool CanSellItem(string game, string itemName, int quantity)
        {
            var item = inventory.FirstOrDefault(i => i.Game == game && i.Name == itemName);
            return item?.Quantity >= quantity;
        }

        public int GetItemQuantity(string game, string itemName)
        {
            var item = inventory.FirstOrDefault(i => i.Game == game && i.Name == itemName);
            return item?.Quantity ?? 0;
        }

        #endregion

        #region Price Methods

        public async Task RefreshSteamPricesAsync()
        {
            try
            {
                await steamPriceService.RefreshAllPricesAsync(inventory);
                Console.WriteLine("✅ Обновление цен из Steam завершено");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении цен: {ex.Message}", "Ошибка");
                Console.WriteLine($"❌ Ошибка обновления цен: {ex.Message}");
            }
        }

        public (decimal price, string source) GetPriceWithSource(string itemName, string game)
        {
            decimal price = steamPriceService.GetItemPrice(itemName, game);
            string source = steamPriceService.GetPriceSource(itemName, game);
            return (price, source);
        }

        public List<string> GetItemsRequiringManualPriceInput()
        {
            return steamPriceService.GetItemsRequiringManualInput(inventory);
        }

        #endregion

        #region Manual Prices Methods

        public void SetManualPrice(string itemName, string game, decimal price)
        {
            steamPriceService.SetManualPrice(itemName, game, price);
        }

        public decimal? GetManualPrice(string itemName, string game)
        {
            return steamPriceService.GetManualPrice(itemName, game);
        }

        public void RemoveManualPrice(string itemName, string game)
        {
            steamPriceService.RemoveManualPrice(itemName, game);
        }

        public Dictionary<string, decimal> GetAllManualPrices()
        {
            return steamPriceService.GetAllManualPrices();
        }

        // Добавляем метод для очистки всех ручных цен
        public void ClearAllManualPrices()
        {
            steamPriceService.ClearAllManualPrices();
        }

        #endregion

        #region Price History Methods

        public List<PriceHistory> GetPriceHistory(string itemName, string game)
        {
            return steamPriceService.GetPriceHistory(itemName, game);
        }

        public decimal GetYesterdayPrice(string itemName, string game)
        {
            return steamPriceService.GetYesterdayPrice(itemName, game);
        }

        public (decimal currentPrice, decimal yesterdayPrice, decimal change, decimal changePercent) GetPriceWithHistory(string itemName, string game)
        {
            decimal currentPrice = GetSteamPrice(itemName, game);
            decimal yesterdayPrice = GetYesterdayPrice(itemName, game);

            decimal change = currentPrice - yesterdayPrice;
            decimal changePercent = yesterdayPrice > 0 ? (change / yesterdayPrice) * 100 : 0;

            return (currentPrice, yesterdayPrice, change, changePercent);
        }

        #endregion

        #region Image Methods

        public async Task<Image> GetItemImageAsync(string itemName, string game, int size = 32)
        {
            return await imageService.GetItemImageAsync(itemName, game, size);
        }

        public void ClearImageCache()
        {
            imageService.ClearImageCache();
        }

        public long GetImageCacheSize()
        {
            return imageService.GetCacheSize();
        }

        #endregion

        #region Custom Items Methods

        public void AddCustomItem(string itemName)
        {
            if (!string.IsNullOrWhiteSpace(itemName) && !customItems.Contains(itemName))
            {
                customItems.Add(itemName);
                SaveCustomItems();
            }
        }

        public void RemoveCustomItem(string itemName)
        {
            if (customItems.Remove(itemName))
            {
                SaveCustomItems();
            }
        }

        public void ResetCustomItems()
        {
            customItems.Clear();
            LoadCustomItems();
            SaveCustomItems();
        }

        public List<string> GetCustomItems()
        {
            return new List<string>(customItems);
        }

        #endregion

        #region Steam Prices Methods

        public decimal GetSteamPrice(string itemName, string game)
        {
            try
            {
                return steamPriceService.GetItemPrice(itemName, game);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения цены: {ex.Message}", "Ошибка");
                return 0;
            }
        }

        public List<InventoryItemWithPrice> GetInventoryWithPrices()
        {
            var result = new List<InventoryItemWithPrice>();

            foreach (var item in inventory)
            {
                try
                {
                    decimal currentPrice = GetSteamPrice(item.Name, item.Game);
                    var (_, source) = GetPriceWithSource(item.Name, item.Game);

                    decimal potentialProfit = CalculateSimpleProfit(item, currentPrice);
                    string recommendation = GetSimpleRecommendation(potentialProfit);

                    result.Add(new InventoryItemWithPrice
                    {
                        Item = item,
                        CurrentPrice = currentPrice,
                        PotentialProfit = potentialProfit,
                        Recommendation = recommendation,
                        PriceSource = source
                    });
                }
                catch (Exception ex)
                {
                    result.Add(new InventoryItemWithPrice
                    {
                        Item = item,
                        CurrentPrice = 0,
                        PotentialProfit = 0,
                        Recommendation = "Ошибка загрузки цены",
                        PriceSource = "ОШИБКА"
                    });
                }
            }

            return result;
        }

        private decimal CalculateSimpleProfit(InventoryItem item, decimal currentPrice)
        {
            if (item.Quantity == 0) return 0;

            // Рассчитываем среднюю стоимость с учетом подарков (нулевая стоимость)
            decimal totalPurchaseValue = item.Batches.Where(b => b.Price > 0).Sum(b => b.Total);
            int totalPurchasedQuantity = item.Batches.Where(b => b.Price > 0).Sum(b => b.Quantity);

            decimal avgPurchase = totalPurchasedQuantity > 0 ? totalPurchaseValue / totalPurchasedQuantity : 0;

            decimal commission = 0.13m;
            decimal netPrice = currentPrice * (1 - commission);

            return (netPrice - avgPurchase) * item.Quantity;
        }

        private string GetSimpleRecommendation(decimal profit)
        {
            if (profit > 100) return "🚀 ВЫСОКАЯ ПРИБЫЛЬ";
            if (profit > 0) return "📈 ПРИБЫЛЬ";
            if (profit < -50) return "🔴 УБЫТОК";
            return "⚪ НЕЙТРАЛЬНО";
        }

        #endregion

        #region Sales Analysis Methods (с учетом комиссии и правильного FIFO)

        // Новый метод для получения анализа продаж
        public List<SalesAnalysis> GetSalesAnalysis()
        {
            List<SalesAnalysis> result = new List<SalesAnalysis>();

            // Получаем все продажи
            List<Transaction> sales = transactions
                .Where(t => t.Operation == "Продажа")
                .OrderBy(t => t.Date)
                .ToList(); // Сортируем по возрастанию даты для правильного FIFO

            // Восстанавливаем историю партий для каждой продажи
            foreach (Transaction sale in sales)
            {
                // Рассчитываем реальную прибыль для этой продажи
                SalesAnalysis analysis = CalculateSaleProfit(sale);
                if (analysis != null)
                {
                    result.Add(analysis);
                }
            }

            return result.OrderByDescending(x => x.SaleDate).ToList(); // Возвращаем в обратном порядке для удобства просмотра
        }

        private SalesAnalysis CalculateSaleProfit(Transaction sale)
        {
            try
            {
                // Получаем все партии до этой продажи (включая предыдущие продажи)
                List<InventoryBatch> currentBatches = GetBatchesAtSaleTime(sale);

                if (currentBatches.Count == 0)
                    return null;

                // Рассчитываем стоимость покупки проданных предметов по FIFO
                decimal purchaseCost = 0;
                int remainingToCalculate = sale.Quantity;
                List<string> batchInfos = new List<string>();

                // Копируем партии для работы
                List<InventoryBatch> tempBatches = currentBatches.Select(b => new InventoryBatch
                {
                    BatchId = b.BatchId,
                    PurchaseDate = b.PurchaseDate,
                    Quantity = b.Quantity,
                    Price = b.Price
                }).ToList();

                // Применяем FIFO для этой продажи
                foreach (InventoryBatch batch in tempBatches.OrderBy(b => b.PurchaseDate).ToList())
                {
                    if (remainingToCalculate <= 0) break;

                    if (batch.Quantity > 0)
                    {
                        int quantityFromBatch = Math.Min(batch.Quantity, remainingToCalculate);
                        purchaseCost += quantityFromBatch * batch.Price;
                        batchInfos.Add($"{quantityFromBatch}шт × {batch.Price} руб. (партия {batch.BatchId.Substring(0, 8)}...)");

                        batch.Quantity -= quantityFromBatch;
                        remainingToCalculate -= quantityFromBatch;
                    }
                }

                // Рассчитываем выручку с учетом комиссии Steam
                decimal grossSale = sale.Total;
                decimal commissionAmount = grossSale * STEAM_COMMISSION;
                decimal netSaleAmount = grossSale - commissionAmount;

                // Рассчитываем реальную прибыль
                decimal realProfit = netSaleAmount - purchaseCost;
                decimal profitPercentage = purchaseCost > 0 ? (realProfit / purchaseCost) * 100 : 0;

                return new SalesAnalysis
                {
                    Game = sale.Game,
                    ItemName = sale.Item,
                    SaleDate = sale.Date,
                    QuantitySold = sale.Quantity,
                    SalePrice = sale.Price,
                    TotalSale = grossSale,
                    CommissionAmount = commissionAmount,
                    NetSaleAmount = netSaleAmount,
                    PurchaseCost = purchaseCost,
                    RealProfit = realProfit,
                    ProfitPercentage = profitPercentage,
                    BatchInfo = string.Join(" + ", batchInfos)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка расчета прибыли для продажи {sale.Id}: {ex.Message}");
                return null;
            }
        }

        // Получаем состояние партий на момент конкретной продажи
        private List<InventoryBatch> GetBatchesAtSaleTime(Transaction sale)
        {
            // Находим все покупки этого предмета до даты продажи
            List<Transaction> purchasesBeforeSale = transactions
                .Where(t => t.Game == sale.Game &&
                           t.Item == sale.Item &&
                           t.Operation == "Покупка" &&
                           t.Date <= sale.Date)
                .OrderBy(t => t.Date)
                .ToList();

            // Восстанавливаем начальное состояние партий
            List<InventoryBatch> batches = purchasesBeforeSale.Select(purchase => new InventoryBatch
            {
                BatchId = purchase.Id,
                PurchaseDate = purchase.Date,
                Quantity = purchase.Quantity,
                Price = purchase.Price
            }).ToList();

            // Находим все продажи до текущей продажи
            List<Transaction> previousSales = transactions
                .Where(t => t.Game == sale.Game &&
                           t.Item == sale.Item &&
                           t.Operation == "Продажа" &&
                           t.Date < sale.Date)
                .OrderBy(t => t.Date)
                .ToList();

            // Применяем все предыдущие продажи по FIFO
            foreach (Transaction previousSale in previousSales)
            {
                ApplySaleToBatches(batches, previousSale);
            }

            return batches.Where(b => b.Quantity > 0).ToList();
        }

        // Применяет продажу к партиям по FIFO
        private void ApplySaleToBatches(List<InventoryBatch> batches, Transaction sale)
        {
            int remainingToSell = sale.Quantity;

            foreach (InventoryBatch batch in batches.OrderBy(b => b.PurchaseDate).ToList())
            {
                if (remainingToSell <= 0) break;

                if (batch.Quantity > 0)
                {
                    int sellFromBatch = Math.Min(batch.Quantity, remainingToSell);
                    batch.Quantity -= sellFromBatch;
                    remainingToSell -= sellFromBatch;
                }
            }
        }

        #endregion

        #region Statistics Methods

        public Statistics GetStatistics()
        {
            var stats = new Statistics();

            stats.TotalTransactions = transactions.Count;
            stats.TotalItems = inventory.Count(i => i.Quantity > 0);

            // Рассчитываем общую прибыль с учетом комиссии
            stats.TotalProfit = 0;
            foreach (var item in inventory)
            {
                // Для статистики используем прибыль без комиссии для совместимости
                stats.TotalProfit += item.Profit;
            }

            stats.ROI = stats.TotalProfit > 0 ? 10 : 0;

            return stats;
        }

        public void SaveToFile()
        {
            try
            {
                string data = $"Экспорт данных: {DateTime.Now}\n";
                data += $"Сделок: {transactions.Count}\n";
                data += $"Предметов: {inventory.Count}\n";

                string filePath = Path.Combine(Application.StartupPath, "export.txt");
                File.WriteAllText(filePath, data);

                MessageBox.Show("Данные экспортированы в export.txt", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка");
            }
        }

        #endregion

        #region Sample Data

        private void LoadSampleData()
        {
            transactions.Clear();
            inventory.Clear();

            // Демо-данные с разными партиями
            transactions.Add(new Transaction
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTime.Now.AddDays(-5),
                Game = "Counter-Strike 2",
                Item = "AK-47 | Redline",
                Operation = "Покупка",
                Quantity = 10,
                Price = 50.00m
            });

            transactions.Add(new Transaction
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTime.Now.AddDays(-2),
                Game = "Counter-Strike 2",
                Item = "AK-47 | Redline",
                Operation = "Покупка",
                Quantity = 10,
                Price = 60.00m
            });

            // Демо-продажа для тестирования
            transactions.Add(new Transaction
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTime.Now.AddDays(-1),
                Game = "Counter-Strike 2",
                Item = "AK-47 | Redline",
                Operation = "Продажа",
                Quantity = 5,
                Price = 80.00m
            });

            // Демо-подарок
            transactions.Add(new Transaction
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTime.Now.AddDays(-3),
                Game = "Counter-Strike 2",
                Item = "AWP | Dragon Lore",
                Operation = "Подарок",
                Quantity = 1,
                Price = 0
            });

            // Строим инвентарь из транзакций
            RebuildInventoryFromTransactions();
        }

        public List<Transaction> GetTransactions()
        {
            return new List<Transaction>(transactions);
        }

        #endregion
    }
}