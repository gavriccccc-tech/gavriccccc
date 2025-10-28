using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace InventoryTrackerApp
{
    public class Transaction
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public string Game { get; set; }
        public string Item { get; set; }
        public string Operation { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        [JsonIgnore]
        public decimal Total => Price * Quantity;
    }

    public class InventoryItem
    {
        public string Game { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPurchase { get; set; }
        public decimal TotalSale { get; set; }

        [JsonIgnore]
        public decimal Profit => TotalSale - TotalPurchase;

        [JsonIgnore]
        public decimal ROI => TotalPurchase > 0 ? (Profit / TotalPurchase) * 100 : 0;

        // Новое свойство для учета подарков
        [JsonIgnore]
        public int GiftQuantity => Batches?.Where(b => b.Price == 0).Sum(b => b.Quantity) ?? 0;

        public List<InventoryBatch> Batches { get; set; } = new List<InventoryBatch>();
    }

    public class InventoryBatch
    {
        public string BatchId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total => Price * Quantity;
    }

    public class AppData
    {
        public List<Transaction> Transactions { get; set; }
        public List<InventoryItem> Inventory { get; set; }
        public List<PriceHistory> PriceHistory { get; set; } = new List<PriceHistory>();
        public List<Order> Orders { get; set; } = new List<Order>(); // НОВОЕ ПОЛЕ ДЛЯ ОРДЕРОВ
        public DateTime LastSave { get; set; }
    }

    public class Statistics
    {
        public int TotalTransactions { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal ROI { get; set; }
    }

    public class InventoryItemWithPrice
    {
        public InventoryItem Item { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal PotentialProfit { get; set; }
        public string Recommendation { get; set; }
        public string PriceSource { get; set; }
    }

    public class PortfolioItemAnalysis
    {
        public string Game { get; set; }
        public string ItemName { get; set; }
        public string BatchId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal PreviousPrice { get; set; }
        public decimal PriceChange { get; set; }
        public decimal PriceChangePercent { get; set; }
        public decimal TotalPurchase { get; set; }
        public decimal TotalSalePotential { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal NetSaleAmount { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal ProfitPercentage { get; set; }
        public string Recommendation { get; set; }
    }

    public class SalesAnalysis
    {
        public string Game { get; set; }
        public string ItemName { get; set; }
        public DateTime SaleDate { get; set; }
        public int QuantitySold { get; set; }
        public decimal SalePrice { get; set; }
        public decimal TotalSale { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal NetSaleAmount { get; set; }
        public decimal PurchaseCost { get; set; }
        public decimal RealProfit { get; set; }
        public decimal ProfitPercentage { get; set; }
        public string BatchInfo { get; set; }
    }

    public class PriceHistory
    {
        public string ItemName { get; set; }
        public string Game { get; set; }
        public decimal Price { get; set; }
        public DateTime Date { get; set; }
        public string Source { get; set; }
    }
}