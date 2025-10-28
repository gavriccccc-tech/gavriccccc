using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace InventoryTrackerApp
{
    public class Order
    {
        public string Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Game { get; set; }
        public string Item { get; set; }
        public string Type { get; set; } // "Покупка" или "Продажа"
        public decimal TargetPrice { get; set; }
        public int TargetQuantity { get; set; }
        public int FilledQuantity { get; set; }
        public string Status { get; set; } // "Активный", "Выполнен", "Отменен"
        public string Notes { get; set; }

        [JsonIgnore]
        public int RemainingQuantity => TargetQuantity - FilledQuantity;

        [JsonIgnore]
        public bool IsActive => Status == "Активный";

        [JsonIgnore]
        public decimal TotalValue => TargetPrice * TargetQuantity;

        [JsonIgnore]
        public decimal FilledValue => TargetPrice * FilledQuantity;

        [JsonIgnore]
        public decimal ProgressPercent => TargetQuantity > 0 ? (decimal)FilledQuantity / TargetQuantity * 100 : 0;

        public List<OrderFill> Fills { get; set; } = new List<OrderFill>();
    }

    public class OrderFill
    {
        public string Id { get; set; }
        public DateTime FillDate { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Notes { get; set; }

        [JsonIgnore]
        public decimal Total => Price * Quantity;
    }
}