using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace InventoryTrackerApp
{
    public class PortfolioAnalysisForm : Form
    {
        private DataService dataService;
        private DataGridView dgvAnalysis;
        private Panel overallStatsPanel;
        private Panel summaryPanel;
        private TableLayoutPanel overallStatsTable;

        public PortfolioAnalysisForm(DataService dataService)
        {
            this.dataService = dataService;
            InitializeForm();
            CreateControls();
            LoadAnalysis();
        }

        private void InitializeForm()
        {
            this.Text = "📈 Анализ портфеля";
            this.Size = new Size(1500, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.MinimumSize = new Size(1600, 500);
        }

        private void CreateControls()
        {
            // Заголовок
            Label titleLabel = new Label();
            titleLabel.Text = "📈 ДЕТАЛЬНЫЙ АНАЛИЗ ПОРТФЕЛЯ";
            titleLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.BackColor = Color.FromArgb(52, 152, 219);
            titleLabel.Dock = DockStyle.Top;
            titleLabel.Height = 50;
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(titleLabel);

            // Главный контейнер
            Panel mainContainer = new Panel();
            mainContainer.Dock = DockStyle.Fill;
            mainContainer.Padding = new Padding(20);
            mainContainer.BackColor = Color.White;
            this.Controls.Add(mainContainer);

            // Панель управления
            Panel controlPanel = new Panel();
            controlPanel.Dock = DockStyle.Top;
            controlPanel.Height = 40;
            controlPanel.BackColor = Color.LightGray;
            controlPanel.Padding = new Padding(10);
            mainContainer.Controls.Add(controlPanel);

            // Комиссия Steam
            Label lblCommission = new Label();
            lblCommission.Text = "Комиссия Steam:";
            lblCommission.Location = new Point(10, 10);
            lblCommission.AutoSize = true;
            lblCommission.Font = new Font("Arial", 9, FontStyle.Bold);
            controlPanel.Controls.Add(lblCommission);

            Label lblCommissionValue = new Label();
            lblCommissionValue.Text = "13%";
            lblCommissionValue.Location = new Point(120, 10);
            lblCommissionValue.AutoSize = true;
            lblCommissionValue.Font = new Font("Arial", 9, FontStyle.Bold);
            lblCommissionValue.ForeColor = Color.Red;
            controlPanel.Controls.Add(lblCommissionValue);

            // Кнопки
            Button btnRefresh = new Button();
            btnRefresh.Text = "🔄 Обновить";
            btnRefresh.Size = new Size(100, 25);
            btnRefresh.Location = new Point(200, 7);
            btnRefresh.BackColor = Color.LightGreen;
            btnRefresh.Font = new Font("Arial", 8, FontStyle.Bold);
            btnRefresh.Click += (s, e) => LoadAnalysis();
            controlPanel.Controls.Add(btnRefresh);

            Button btnClose = new Button();
            btnClose.Text = "Закрыть";
            btnClose.Size = new Size(80, 25);
            btnClose.Location = new Point(310, 7);
            btnClose.BackColor = Color.LightCoral;
            btnClose.Font = new Font("Arial", 8, FontStyle.Bold);
            btnClose.Click += (s, e) => this.Close();
            controlPanel.Controls.Add(btnClose);

            // Панель общей статистики
            overallStatsPanel = new Panel();
            overallStatsPanel.Dock = DockStyle.Top;
            overallStatsPanel.Height = 70;
            overallStatsPanel.BackColor = Color.FromArgb(240, 248, 255);
            overallStatsPanel.BorderStyle = BorderStyle.FixedSingle;
            overallStatsPanel.Padding = new Padding(10);
            overallStatsPanel.Margin = new Padding(0, 10, 0, 10);
            mainContainer.Controls.Add(overallStatsPanel);

            // Таблица анализа
            dgvAnalysis = new DataGridView();
            dgvAnalysis.Dock = DockStyle.Fill;
            dgvAnalysis.BackgroundColor = Color.White;
            dgvAnalysis.RowHeadersVisible = false;
            dgvAnalysis.Font = new Font("Arial", 9);
            dgvAnalysis.AllowUserToAddRows = false;
            dgvAnalysis.ReadOnly = true;
            dgvAnalysis.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvAnalysis.Margin = new Padding(0, 0, 0, 10);
            mainContainer.Controls.Add(dgvAnalysis);

            // Панель итогов по партиям
            summaryPanel = new Panel();
            summaryPanel.Dock = DockStyle.Bottom;
            summaryPanel.Height = 50;
            summaryPanel.BackColor = Color.LightGoldenrodYellow;
            summaryPanel.BorderStyle = BorderStyle.FixedSingle;
            summaryPanel.Padding = new Padding(10);
            mainContainer.Controls.Add(summaryPanel);

            // Устанавливаем порядок отображения
            mainContainer.Controls.SetChildIndex(summaryPanel, 0);
            mainContainer.Controls.SetChildIndex(dgvAnalysis, 1);
            mainContainer.Controls.SetChildIndex(overallStatsPanel, 2);
            mainContainer.Controls.SetChildIndex(controlPanel, 3);
        }

        private void LoadAnalysis()
        {
            try
            {
                Console.WriteLine("=== НАЧАЛО АНАЛИЗА ===");

                List<PortfolioItemAnalysis> analysisData = CalculatePortfolioAnalysis();
                Console.WriteLine($"Найдено данных для анализа: {analysisData.Count}");

                UpdateDataGrid(analysisData);
                UpdateOverallStats(analysisData);
                UpdateSummary(analysisData);

                Console.WriteLine("=== АНАЛИЗ ЗАВЕРШЕН ===");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки анализа: {ex.Message}", "Ошибка");
                Console.WriteLine($"ОШИБКА: {ex.Message}");
            }
        }

        private List<PortfolioItemAnalysis> CalculatePortfolioAnalysis()
        {
            List<PortfolioItemAnalysis> result = new List<PortfolioItemAnalysis>();
            List<InventoryItem> inventory = dataService.GetInventory();
            const decimal steamCommission = 0.13m;

            Console.WriteLine($"Всего предметов в инвентаре: {inventory.Count}");

            foreach (InventoryItem item in inventory)
            {
                Console.WriteLine($"Обрабатываем предмет: {item.Game} | {item.Name} | Кол-во: {item.Quantity}");

                if (item.Quantity == 0)
                {
                    Console.WriteLine($"  Пропускаем - нет в наличии");
                    continue;
                }

                List<InventoryBatch> batches = dataService.GetItemBatches(item.Game, item.Name);
                Console.WriteLine($"  Найдено партий: {batches.Count}");

                // ПОЛУЧАЕМ ТЕКУЩУЮ И ВЧЕРАШНЮЮ ЦЕНУ
                var priceData = dataService.GetPriceWithHistory(item.Name, item.Game);
                decimal currentPrice = priceData.currentPrice;
                decimal yesterdayPrice = priceData.yesterdayPrice;
                decimal priceChange = priceData.change;
                decimal priceChangePercent = priceData.changePercent;

                Console.WriteLine($"  Текущая цена: {currentPrice}, Вчерашняя: {yesterdayPrice}");

                // Если цена не установлена, пропускаем
                if (currentPrice == 0)
                {
                    Console.WriteLine($"  Пропускаем - цена не установлена");
                    continue;
                }

                // Анализ по партиям
                foreach (InventoryBatch batch in batches.Where(b => b.Quantity > 0))
                {
                    Console.WriteLine($"    Партия: {batch.BatchId} | Кол-во: {batch.Quantity} | Цена покупки: {batch.Price}");

                    PortfolioItemAnalysis analysis = new PortfolioItemAnalysis
                    {
                        Game = item.Game,
                        ItemName = item.Name,
                        BatchId = batch.BatchId,
                        PurchaseDate = batch.PurchaseDate,
                        Quantity = batch.Quantity,
                        PurchasePrice = batch.Price,
                        CurrentPrice = currentPrice,
                        PreviousPrice = yesterdayPrice, // НОВОЕ ПОЛЕ
                        PriceChange = priceChange, // НОВОЕ ПОЛЕ
                        PriceChangePercent = priceChangePercent, // НОВОЕ ПОЛЕ
                        TotalPurchase = batch.Total
                    };

                    // Расчеты с учетом комиссии
                    analysis.TotalSalePotential = analysis.Quantity * analysis.CurrentPrice;
                    analysis.CommissionAmount = analysis.TotalSalePotential * steamCommission;
                    analysis.NetSaleAmount = analysis.TotalSalePotential - analysis.CommissionAmount;
                    analysis.GrossProfit = analysis.NetSaleAmount - analysis.TotalPurchase;
                    analysis.ProfitPercentage = analysis.TotalPurchase > 0 ?
                        (analysis.GrossProfit / analysis.TotalPurchase) * 100 : 0;

                    // Рекомендация
                    analysis.Recommendation = GetRecommendation(analysis.GrossProfit, analysis.ProfitPercentage);

                    Console.WriteLine($"      Потенц. выручка: {analysis.TotalSalePotential}");
                    Console.WriteLine($"      Комиссия: {analysis.CommissionAmount}");
                    Console.WriteLine($"      Чистая выручка: {analysis.NetSaleAmount}");
                    Console.WriteLine($"      Прибыль: {analysis.GrossProfit}");

                    result.Add(analysis);
                }
            }

            return result.OrderByDescending(x => x.GrossProfit).ToList();
        }

        private string GetRecommendation(decimal grossProfit, decimal profitPercentage)
        {
            if (profitPercentage > 50)
                return "🚀 ВЫСОКАЯ ПРИБЫЛЬ";
            else if (profitPercentage > 20)
                return "📈 ХОРОШАЯ ПРИБЫЛЬ";
            else if (profitPercentage > 5)
                return "✅ ПРИБЫЛЬ";
            else if (profitPercentage > -5)
                return "⚪ НЕЙТРАЛЬНО";
            else if (profitPercentage > -20)
                return "📉 НЕБОЛЬШОЙ УБЫТОК";
            else
                return "🔴 БОЛЬШОЙ УБЫТОК";
        }

        private void UpdateDataGrid(List<PortfolioItemAnalysis> analysisData)
        {
            dgvAnalysis.Columns.Clear();

            if (analysisData.Count == 0)
            {
                dgvAnalysis.Columns.Add("Empty", "Информация");
                dgvAnalysis.Rows.Add("📊 Нет данных для анализа. Добавьте сделки и установите цены через 'Ручные цены'.");
                return;
            }

            // Создаем колонки - ДОБАВЛЯЕМ НОВЫЕ КОЛОНКИ ДЛЯ ЦЕН
            dgvAnalysis.Columns.Add("Game", "Игра");
            dgvAnalysis.Columns.Add("Item", "Предмет");
            dgvAnalysis.Columns.Add("Date", "Дата покупки");
            dgvAnalysis.Columns.Add("Batch", "Партия");
            dgvAnalysis.Columns.Add("Qty", "Кол-во");
            dgvAnalysis.Columns.Add("BuyPrice", "Цена покупки");
            dgvAnalysis.Columns.Add("PrevPrice", "Вчерашняя цена"); // НОВАЯ КОЛОНКА
            dgvAnalysis.Columns.Add("CurPrice", "Текущая цена");
            dgvAnalysis.Columns.Add("PriceChange", "Изменение цены"); // НОВАЯ КОЛОНКА
            dgvAnalysis.Columns.Add("TotalBuy", "Сумма покупки");
            dgvAnalysis.Columns.Add("PotSale", "Потенц. выручка");
            dgvAnalysis.Columns.Add("Commission", "Комиссия");
            dgvAnalysis.Columns.Add("NetSale", "Чистая выручка");
            dgvAnalysis.Columns.Add("Profit", "Прибыль");
            dgvAnalysis.Columns.Add("ROI", "ROI %");
            dgvAnalysis.Columns.Add("Recommend", "Рекомендация");

            // Настраиваем ширины колонок
            dgvAnalysis.Columns["Game"].MinimumWidth = 80;
            dgvAnalysis.Columns["Item"].MinimumWidth = 150;
            dgvAnalysis.Columns["Date"].MinimumWidth = 80;
            dgvAnalysis.Columns["Batch"].MinimumWidth = 70;
            dgvAnalysis.Columns["Qty"].MinimumWidth = 60;
            dgvAnalysis.Columns["BuyPrice"].MinimumWidth = 90;
            dgvAnalysis.Columns["PrevPrice"].MinimumWidth = 90; // НОВАЯ КОЛОНКА
            dgvAnalysis.Columns["CurPrice"].MinimumWidth = 90;
            dgvAnalysis.Columns["PriceChange"].MinimumWidth = 90; // НОВАЯ КОЛОНКА
            dgvAnalysis.Columns["TotalBuy"].MinimumWidth = 100;
            dgvAnalysis.Columns["PotSale"].MinimumWidth = 110;
            dgvAnalysis.Columns["Commission"].MinimumWidth = 80;
            dgvAnalysis.Columns["NetSale"].MinimumWidth = 110;
            dgvAnalysis.Columns["Profit"].MinimumWidth = 90;
            dgvAnalysis.Columns["ROI"].MinimumWidth = 70;
            dgvAnalysis.Columns["Recommend"].MinimumWidth = 130;

            // Заполняем данными
            foreach (PortfolioItemAnalysis analysis in analysisData)
            {
                // Форматируем изменение цены
                string priceChangeText = analysis.PriceChange == 0 ? "0.00 ₽" :
                    analysis.PriceChange > 0 ? $"+{analysis.PriceChange:0.00} ₽" :
                    $"{analysis.PriceChange:0.00} ₽";

                string priceChangePercentText = analysis.PriceChangePercent == 0 ? "0.0%" :
                    analysis.PriceChangePercent > 0 ? $"+{analysis.PriceChangePercent:0.0}%" :
                    $"{analysis.PriceChangePercent:0.0}%";

                int idx = dgvAnalysis.Rows.Add(
                    analysis.Game,
                    analysis.ItemName,
                    analysis.PurchaseDate.ToString("dd.MM.yy"),
                    analysis.BatchId.Substring(0, 6) + "...",
                    analysis.Quantity + " шт.",
                    analysis.PurchasePrice.ToString("0.00") + " ₽",
                    analysis.PreviousPrice > 0 ? analysis.PreviousPrice.ToString("0.00") + " ₽" : "Н/Д", // ВЧЕРАШНЯЯ ЦЕНА
                    analysis.CurrentPrice.ToString("0.00") + " ₽",
                    $"{priceChangeText} ({priceChangePercentText})", // ИЗМЕНЕНИЕ ЦЕНЫ
                    analysis.TotalPurchase.ToString("0.00") + " ₽",
                    analysis.TotalSalePotential.ToString("0.00") + " ₽",
                    analysis.CommissionAmount.ToString("0.00") + " ₽",
                    analysis.NetSaleAmount.ToString("0.00") + " ₽",
                    analysis.GrossProfit.ToString("0.00") + " ₽",
                    analysis.ProfitPercentage.ToString("0.0") + "%",
                    analysis.Recommendation
                );

                // Цветовое кодирование для изменения цены
                DataGridViewRow row = dgvAnalysis.Rows[idx];
                if (analysis.PriceChange > 0)
                {
                    row.Cells["PriceChange"].Style.ForeColor = Color.DarkGreen;
                    row.Cells["PriceChange"].Style.Font = new Font(dgvAnalysis.Font, FontStyle.Bold);
                }
                else if (analysis.PriceChange < 0)
                {
                    row.Cells["PriceChange"].Style.ForeColor = Color.DarkRed;
                    row.Cells["PriceChange"].Style.Font = new Font(dgvAnalysis.Font, FontStyle.Bold);
                }

                // Основное цветовое кодирование по прибыли
                Color backColor = GetColorForProfit(analysis.ProfitPercentage);
                row.DefaultCellStyle.BackColor = backColor;

                if (backColor == Color.LavenderBlush || backColor == Color.LightCoral)
                {
                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
                }
            }
        }

        private Color GetColorForProfit(decimal profitPercentage)
        {
            if (profitPercentage > 20) return Color.LightGreen;
            if (profitPercentage > 5) return Color.PaleGreen;
            if (profitPercentage > -5) return Color.LightYellow;
            if (profitPercentage > -20) return Color.LavenderBlush;
            return Color.LightCoral;
        }

        private void UpdateOverallStats(List<PortfolioItemAnalysis> analysisData)
        {
            if (overallStatsPanel != null)
            {
                overallStatsPanel.Controls.Clear();

                if (analysisData.Count == 0)
                {
                    Label noDataLabel = new Label();
                    noDataLabel.Text = "📊 Нет данных для анализа";
                    noDataLabel.Dock = DockStyle.Fill;
                    noDataLabel.TextAlign = ContentAlignment.MiddleCenter;
                    noDataLabel.Font = new Font("Arial", 12, FontStyle.Bold);
                    noDataLabel.ForeColor = Color.Gray;
                    overallStatsPanel.Controls.Add(noDataLabel);
                    return;
                }

                decimal totalPotentialSale = analysisData.Sum(x => x.TotalSalePotential);
                decimal totalCommission = analysisData.Sum(x => x.CommissionAmount);
                decimal totalNetSale = analysisData.Sum(x => x.NetSaleAmount);
                decimal totalProfit = analysisData.Sum(x => x.GrossProfit);

                // Создаем таблицу для общей статистики
                overallStatsTable = new TableLayoutPanel();
                overallStatsTable.Dock = DockStyle.Fill;
                overallStatsTable.ColumnCount = 4;
                overallStatsTable.RowCount = 1;
                overallStatsTable.Padding = new Padding(5);
                overallStatsTable.Margin = new Padding(0);
                overallStatsPanel.Controls.Add(overallStatsTable);

                // Настройка столбцов
                for (int i = 0; i < 4; i++)
                {
                    overallStatsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
                }

                // Общая потенциальная выручка
                AddOverallStatLabel(overallStatsTable, 0, "💰 Общая потенц. выручка:",
                    totalPotentialSale.ToString("0.00") + " руб.", Color.DarkBlue, FontStyle.Bold);

                // Общая комиссия
                AddOverallStatLabel(overallStatsTable, 1, "💸 Общая комиссия:",
                    totalCommission.ToString("0.00") + " руб.", Color.DarkOrange, FontStyle.Bold);

                // Общая чистая выручка
                AddOverallStatLabel(overallStatsTable, 2, "🏆 Общая чистая выручка:",
                    totalNetSale.ToString("0.00") + " руб.", Color.DarkGreen, FontStyle.Bold);

                // Общая прибыль
                AddOverallStatLabel(overallStatsTable, 3, "📈 Общая прибыль:",
                    totalProfit.ToString("0.00") + " руб.",
                    totalProfit >= 0 ? Color.Green : Color.Red, FontStyle.Bold);
            }
        }

        private void AddOverallStatLabel(TableLayoutPanel table, int column, string title, string value, Color color, FontStyle style)
        {
            Panel cellPanel = new Panel();
            cellPanel.Dock = DockStyle.Fill;
            cellPanel.Padding = new Padding(8, 15, 10, 15);
            cellPanel.BackColor = Color.White;
            cellPanel.BorderStyle = BorderStyle.FixedSingle;

            // Label для названия
            Label titleLabel = new Label();
            titleLabel.Text = title;
            titleLabel.Font = new Font("Arial", 9, FontStyle.Regular);
            titleLabel.ForeColor = Color.Black;
            titleLabel.Location = new Point(8, 15);
            titleLabel.AutoSize = true;
            titleLabel.TextAlign = ContentAlignment.MiddleLeft;
            cellPanel.Controls.Add(titleLabel);

            // Label для значения
            Label valueLabel = new Label();
            valueLabel.Text = value;
            valueLabel.Font = new Font("Arial", 10, style);
            valueLabel.ForeColor = color;
            valueLabel.Location = new Point(titleLabel.Right + 2, 15);
            valueLabel.AutoSize = true;
            valueLabel.TextAlign = ContentAlignment.MiddleLeft;
            cellPanel.Controls.Add(valueLabel);

            table.Controls.Add(cellPanel, column, 0);
        }

        private void UpdateSummary(List<PortfolioItemAnalysis> analysisData)
        {
            if (summaryPanel != null)
            {
                summaryPanel.Controls.Clear();

                if (analysisData.Count == 0) return;

                decimal totalPurchase = analysisData.Sum(x => x.TotalPurchase);
                decimal totalPotentialSale = analysisData.Sum(x => x.TotalSalePotential);
                decimal totalCommission = analysisData.Sum(x => x.CommissionAmount);
                decimal totalNetSale = analysisData.Sum(x => x.NetSaleAmount);
                decimal totalProfit = analysisData.Sum(x => x.GrossProfit);
                decimal avgROI = analysisData.Average(x => x.ProfitPercentage);

                int profitableItems = analysisData.Count(x => x.GrossProfit > 0);
                int losingItems = analysisData.Count(x => x.GrossProfit < 0);

                // Создаем таблицу для итогов - теперь в один ряд
                TableLayoutPanel table = new TableLayoutPanel();
                table.Dock = DockStyle.Fill;
                table.ColumnCount = 8; // Увеличиваем количество колонок
                table.RowCount = 1;    // Только одна строка
                table.Padding = new Padding(3);
                summaryPanel.Controls.Add(table);

                // Настройка столбцов - все одинаковой ширины
                for (int i = 0; i < 8; i++)
                {
                    table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5f));
                }

                // Все показатели в один ряд
                AddSummaryLabel(table, 0, 0, "💰 Прибыль:", totalProfit.ToString("0.00") + "₽",
                    totalProfit >= 0 ? Color.Green : Color.Red, FontStyle.Bold);

                AddSummaryLabel(table, 1, 0, "📈 ROI:", avgROI.ToString("0.0") + "%",
                    avgROI >= 0 ? Color.Green : Color.Red, FontStyle.Bold);

                AddSummaryLabel(table, 2, 0, "📦 Количесво сделок:", analysisData.Count.ToString(),
                    Color.Blue, FontStyle.Regular);

                AddSummaryLabel(table, 3, 0, "✅ Плюсовые сделки:", profitableItems.ToString(),
                    Color.Green, FontStyle.Regular);

                AddSummaryLabel(table, 4, 0, "🔴 Минусовые сделки:", losingItems.ToString(),
                    Color.Red, FontStyle.Regular);

                AddSummaryLabel(table, 5, 0, "💸 Комиссия:", totalCommission.ToString("0.00") + "₽",
                    Color.DarkOrange, FontStyle.Regular);

                AddSummaryLabel(table, 6, 0, "💵 Вложения:", totalPurchase.ToString("0.00") + "₽",
                    Color.Blue, FontStyle.Regular);

                AddSummaryLabel(table, 7, 0, "🎯 Чистая:", totalNetSale.ToString("0.00") + "₽",
                    Color.DarkGreen, FontStyle.Regular);
            }
        }

        private void AddSummaryLabel(TableLayoutPanel table, int column, int row, string title, string value, Color color, FontStyle style)
        {
            Panel cellPanel = new Panel();
            cellPanel.Dock = DockStyle.Fill;
            cellPanel.Padding = new Padding(3);

            // Label для названия
            Label titleLabel = new Label();
            titleLabel.Text = title;
            titleLabel.Font = new Font("Arial", 7, FontStyle.Regular); // Уменьшен шрифт
            titleLabel.ForeColor = Color.Black;
            titleLabel.Location = new Point(3, 3);
            titleLabel.AutoSize = true;
            cellPanel.Controls.Add(titleLabel);

            // Label для значения (рядом с названием)
            Label valueLabel = new Label();
            valueLabel.Text = value;
            valueLabel.Font = new Font("Arial", 8, style); // Уменьшен шрифт
            valueLabel.ForeColor = color;
            valueLabel.Location = new Point(titleLabel.Right + 1, 3); // Уменьшен отступ
            valueLabel.AutoSize = true;
            valueLabel.TextAlign = ContentAlignment.MiddleLeft;
            cellPanel.Controls.Add(valueLabel);

            table.Controls.Add(cellPanel, column, row);
        }
    }
}