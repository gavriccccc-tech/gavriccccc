using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace InventoryTrackerApp
{
    public class PriceHistoryForm : Form
    {
        private DataService dataService;
        private DataGridView dgvHistory;
        private string currentItem;
        private string currentGame;

        public PriceHistoryForm(DataService dataService, string itemName, string game)
        {
            this.dataService = dataService;
            this.currentItem = itemName;
            this.currentGame = game;
            InitializeForm();
            CreateControls();
            LoadPriceHistory();
        }

        private void InitializeForm()
        {
            this.Text = $"📈 История цен: {currentItem}";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
        }

        private void CreateControls()
        {
            // Заголовок
            Label titleLabel = new Label();
            titleLabel.Text = $"📈 ИСТОРИЯ ЦЕН: {currentItem} ({currentGame})";
            titleLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.BackColor = Color.FromArgb(52, 152, 219);
            titleLabel.Dock = DockStyle.Top;
            titleLabel.Height = 40;
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(titleLabel);

            // Таблица истории
            dgvHistory = new DataGridView();
            dgvHistory.Dock = DockStyle.Fill;
            dgvHistory.BackgroundColor = Color.White;
            dgvHistory.RowHeadersVisible = false;
            dgvHistory.ReadOnly = true;
            this.Controls.Add(dgvHistory);

            // Кнопка закрытия
            Button btnClose = new Button();
            btnClose.Text = "Закрыть";
            btnClose.Size = new Size(80, 30);
            btnClose.Location = new Point(10, 45);
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        private void LoadPriceHistory()
        {
            var history = dataService.GetPriceHistory(currentItem, currentGame);

            dgvHistory.Columns.Clear();
            dgvHistory.Columns.Add("Date", "Дата");
            dgvHistory.Columns.Add("Price", "Цена (руб)");
            dgvHistory.Columns.Add("Source", "Источник");

            foreach (var record in history.OrderByDescending(h => h.Date))
            {
                int idx = dgvHistory.Rows.Add(
                    record.Date.ToString("dd.MM.yyyy HH:mm"),
                    record.Price.ToString("0.00"),
                    record.Source
                );
            }
        }
    }
}