using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace InventoryTrackerApp
{
    public class SalesAnalysisForm : Form
    {
        private DataService dataService;
        private DataGridView dgvSales;

        public SalesAnalysisForm(DataService dataService)
        {
            this.dataService = dataService;
            InitializeForm();
            CreateControls();
            LoadSalesAnalysis();
        }

        private void InitializeForm()
        {
            this.Text = "💰 Анализ продаж и реальной прибыли";
            this.Size = new Size(1100, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
        }

        private void CreateControls()
        {
            // Заголовок
            Label titleLabel = new Label();
            titleLabel.Text = "💰 РЕАЛЬНАЯ ПРИБЫЛЬ ОТ ПРОДАЖ";
            titleLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.BackColor = Color.FromArgb(46, 204, 113);
            titleLabel.Location = new Point(0, 0);
            titleLabel.Size = new Size(1100, 50);
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(titleLabel);

            int yPos = 60;

            // Панель информации
            Panel infoPanel = new Panel();
            infoPanel.Location = new Point(20, yPos);
            infoPanel.Size = new Size(1060, 30);
            infoPanel.BackColor = Color.LightYellow;
            infoPanel.BorderStyle = BorderStyle.FixedSingle;

            Label infoLabel = new Label();
            infoLabel.Text = "💡 Отображаются все завершенные продажи с расчетом реальной прибыли";
            infoLabel.Dock = DockStyle.Fill;
            infoLabel.TextAlign = ContentAlignment.MiddleCenter;
            infoLabel.Font = new Font("Arial", 9, FontStyle.Italic);
            infoLabel.ForeColor = Color.DarkBlue;
            infoPanel.Controls.Add(infoLabel);
            this.Controls.Add(infoPanel);

            yPos += 40;

            // Таблица продаж
            dgvSales = new DataGridView();
            dgvSales.Location = new Point(20, yPos);
            dgvSales.Size = new Size(1060, 400);
            dgvSales.BackgroundColor = Color.White;
            dgvSales.RowHeadersVisible = false;
            dgvSales.Font = new Font("Arial", 8);
            dgvSales.AllowUserToAddRows = false;
            dgvSales.ReadOnly = true;
            this.Controls.Add(dgvSales);

            yPos += 410;

            // Кнопки
            Button btnRefresh = new Button();
            btnRefresh.Text = "🔄 Обновить";
            btnRefresh.Size = new Size(100, 30);
            btnRefresh.Location = new Point(20, yPos);
            btnRefresh.BackColor = Color.LightGreen;
            btnRefresh.Font = new Font("Arial", 9, FontStyle.Bold);
            btnRefresh.Click += (s, e) => LoadSalesAnalysis();
            this.Controls.Add(btnRefresh);

            Button btnClose = new Button();
            btnClose.Text = "Закрыть";
            btnClose.Size = new Size(80, 30);
            btnClose.Location = new Point(130, yPos);
            btnClose.BackColor = Color.LightCoral;
            btnClose.Font = new Font("Arial", 9, FontStyle.Bold);
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);

            // Панель итогов
            Panel summaryPanel = new Panel();
            summaryPanel.Location = new Point(220, yPos);
            summaryPanel.Size = new Size(860, 30);
            summaryPanel.BackColor = Color.LightGoldenrodYellow;
            this.Controls.Add(summaryPanel);
        }

        private void LoadSalesAnalysis()
        {
            try
            {
                List<SalesAnalysis> salesData = dataService.GetSalesAnalysis();
                UpdateDataGrid(salesData);
                UpdateSummary(salesData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки анализа продаж: {ex.Message}", "Ошибка");
            }
        }

        private void UpdateDataGrid(List<SalesAnalysis> salesData)
        {
            dgvSales.Columns.Clear();

            if (salesData.Count == 0)
            {
                dgvSales.Columns.Add("Empty", "Информация");
                dgvSales.Rows.Add("📊 Нет данных о продажах. Продажи появятся здесь после совершения сделок.");
                return;
            }

            // Создаем колонки
            string[] columns = {
    "Дата продажи", "Игра", "Предмет", "Кол-во",
    "Цена продажи", "Выручка", "Комиссия",
    "Чистая выручка", "Себестоимость",
    "Реальная прибыль", "ROI %", "Партии покупки"
};

            foreach (string col in columns)
                dgvSales.Columns.Add(col, col);

            // Заполняем данными
            foreach (SalesAnalysis sale in salesData)
            {
                int idx = dgvSales.Rows.Add(
                    sale.SaleDate.ToString("dd.MM.yyyy HH:mm"),
                    sale.Game,
                    sale.ItemName,
                    sale.QuantitySold + " шт.",
                    sale.SalePrice.ToString("0.00") + " руб.",
                    sale.TotalSale.ToString("0.00") + " руб.",
                    sale.CommissionAmount.ToString("0.00") + " руб.",
                    sale.NetSaleAmount.ToString("0.00") + " руб.",
                    sale.PurchaseCost.ToString("0.00") + " руб.",
                    sale.RealProfit.ToString("0.00") + " руб.",
                    sale.ProfitPercentage.ToString("0.00") + "%",
                    sale.BatchInfo
                );

                // Цветовое кодирование по прибыли
                if (sale.RealProfit > 0)
                {
                    dgvSales.Rows[idx].DefaultCellStyle.BackColor = Color.Honeydew;
                    dgvSales.Rows[idx].DefaultCellStyle.ForeColor = Color.DarkGreen;
                }
                else if (sale.RealProfit < 0)
                {
                    dgvSales.Rows[idx].DefaultCellStyle.BackColor = Color.LavenderBlush;
                    dgvSales.Rows[idx].DefaultCellStyle.ForeColor = Color.DarkRed;
                }
                else
                {
                    dgvSales.Rows[idx].DefaultCellStyle.BackColor = Color.LightYellow;
                }
            }

            // Настраиваем ширину колонок
            dgvSales.Columns[0].Width = 120; // Дата продажи
            dgvSales.Columns[1].Width = 100; // Игра
            dgvSales.Columns[2].Width = 150; // Предмет
            dgvSales.Columns[3].Width = 70;  // Кол-во
            dgvSales.Columns[4].Width = 90;  // Цена продажи
            dgvSales.Columns[5].Width = 90;  // Выручка
            dgvSales.Columns[6].Width = 100; // Себестоимость
            dgvSales.Columns[7].Width = 100; // Реальная прибыль
            dgvSales.Columns[8].Width = 70;  // ROI %
            dgvSales.Columns[9].Width = 150; // Партии покупки
        }

        private void UpdateSummary(List<SalesAnalysis> salesData)
        {
            if (salesData.Count == 0) return;

            decimal totalSales = salesData.Sum(x => x.TotalSale);
            decimal totalProfit = salesData.Sum(x => x.RealProfit);
            decimal totalCost = salesData.Sum(x => x.PurchaseCost);
            decimal avgROI = salesData.Average(x => x.ProfitPercentage);

            int profitableSales = salesData.Count(x => x.RealProfit > 0);
            int losingSales = salesData.Count(x => x.RealProfit < 0);

            // Находим панель итогов
            Panel summaryPanel = this.Controls.OfType<Panel>()
                .FirstOrDefault(p => p.BackColor == Color.LightGoldenrodYellow);

            if (summaryPanel != null)
            {
                summaryPanel.Controls.Clear();

                Label summaryLabel = new Label();
                summaryLabel.Text = $"💰 ОБЩАЯ ПРИБЫЛЬ: {totalProfit:0.00} руб. | 📈 Всего продаж: {salesData.Count} | ✅ Прибыльных: {profitableSales} | 🔴 Убыточных: {losingSales} | 🎯 Средний ROI: {avgROI:0.00}%";
                summaryLabel.Dock = DockStyle.Fill;
                summaryLabel.TextAlign = ContentAlignment.MiddleCenter;
                summaryLabel.Font = new Font("Arial", 9, FontStyle.Bold);
                summaryLabel.ForeColor = totalProfit >= 0 ? Color.Green : Color.Red;
                summaryPanel.Controls.Add(summaryLabel);
            }
        }
    }
}