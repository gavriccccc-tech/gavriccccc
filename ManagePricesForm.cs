using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryTrackerApp
{
    public class ManagePricesForm : Form
    {
        private DataService dataService;
        private DataGridView dgvPrices;

        public ManagePricesForm(DataService dataService)
        {
            this.dataService = dataService;
            InitializeForm();
            CreateControls();
            LoadPrices();
        }

        private void InitializeForm()
        {
            this.Text = "💰 Управление ценами Steam";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
        }

        private void CreateControls()
        {
            // Заголовок
            Label titleLabel = new Label();
            titleLabel.Text = "💰 РУЧНОЕ УПРАВЛЕНИЕ ЦЕНАМИ STEAM";
            titleLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.BackColor = Color.FromArgb(52, 152, 219);
            titleLabel.Location = new Point(0, 0);
            titleLabel.Size = new Size(800, 50);
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(titleLabel);

            int yPos = 60;

            // Информация
            Label infoLabel = new Label();
            infoLabel.Text = "Здесь вы можете установить собственные цены для предметов. Ручные цены имеют приоритет над автоматическими.";
            infoLabel.Location = new Point(20, yPos);
            infoLabel.Size = new Size(760, 30);
            infoLabel.Font = new Font("Arial", 9, FontStyle.Italic);
            infoLabel.ForeColor = Color.DarkBlue;
            infoLabel.TextAlign = ContentAlignment.MiddleLeft;
            this.Controls.Add(infoLabel);
            yPos += 40;

            // Таблица цен
            dgvPrices = new DataGridView();
            dgvPrices.Location = new Point(20, yPos);
            dgvPrices.Size = new Size(760, 300);
            dgvPrices.BackgroundColor = Color.White;
            dgvPrices.RowHeadersVisible = false;
            dgvPrices.Font = new Font("Arial", 9);
            dgvPrices.AllowUserToAddRows = false;

            // Колонки
            dgvPrices.Columns.Add("Game", "Игра");
            dgvPrices.Columns.Add("Item", "Предмет");
            dgvPrices.Columns.Add("ManualPrice", "Ручная цена (руб)");
            dgvPrices.Columns.Add("AutoPrice", "Авто цена (руб)");
            dgvPrices.Columns.Add("Status", "Статус");

            dgvPrices.Columns["Game"].Width = 150;
            dgvPrices.Columns["Item"].Width = 200;
            dgvPrices.Columns["ManualPrice"].Width = 120;
            dgvPrices.Columns["AutoPrice"].Width = 120;
            dgvPrices.Columns["Status"].Width = 120;

            // Разрешаем редактирование только колонки с ручной ценой
            dgvPrices.Columns["ManualPrice"].ReadOnly = false;
            dgvPrices.Columns["Game"].ReadOnly = true;
            dgvPrices.Columns["Item"].ReadOnly = true;
            dgvPrices.Columns["AutoPrice"].ReadOnly = true;
            dgvPrices.Columns["Status"].ReadOnly = true;

            this.Controls.Add(dgvPrices);
            yPos += 310;

            // Кнопки
            Button btnSave = new Button();
            btnSave.Text = "💾 Сохранить все цены";
            btnSave.Location = new Point(20, yPos);
            btnSave.Size = new Size(180, 35);
            btnSave.BackColor = Color.FromArgb(46, 204, 113);
            btnSave.ForeColor = Color.White;
            btnSave.Font = new Font("Arial", 10, FontStyle.Bold);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            Button btnClear = new Button();
            btnClear.Text = "🗑️ Очистить все ручные цены";
            btnClear.Location = new Point(210, yPos);
            btnClear.Size = new Size(200, 35);
            btnClear.BackColor = Color.FromArgb(231, 76, 60);
            btnClear.ForeColor = Color.White;
            btnClear.Font = new Font("Arial", 10, FontStyle.Bold);
            btnClear.FlatStyle = FlatStyle.Flat;
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += BtnClear_Click;
            this.Controls.Add(btnClear);

            Button btnClose = new Button();
            btnClose.Text = "Закрыть";
            btnClose.Location = new Point(680, yPos);
            btnClose.Size = new Size(100, 35);
            btnClose.BackColor = Color.LightGray;
            btnClose.Font = new Font("Arial", 10, FontStyle.Bold);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        private void LoadPrices()
        {
            dgvPrices.Rows.Clear();

            var inventory = dataService.GetInventory();
            var manualPrices = dataService.GetAllManualPrices();

            foreach (var item in inventory)
            {
                string cacheKey = $"{item.Game}_{item.Name}";
                decimal autoPrice = dataService.GetSteamPrice(item.Name, item.Game);
                decimal? manualPrice = manualPrices.ContainsKey(cacheKey) ? manualPrices[cacheKey] : (decimal?)null;
                string status = manualPrice.HasValue ? "РУЧНАЯ" : "АВТО";

                int rowIndex = dgvPrices.Rows.Add(
                    item.Game,
                    item.Name,
                    manualPrice.HasValue ? manualPrice.Value.ToString("0.00") : "",
                    autoPrice.ToString("0.00"),
                    status
                );

                // Подсветка строк
                if (status == "РУЧНАЯ")
                {
                    dgvPrices.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
                }
            }

            if (dgvPrices.Rows.Count == 0)
            {
                dgvPrices.Rows.Add("", "Нет предметов в инвентаре", "", "", "");
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                int savedCount = 0;

                foreach (DataGridViewRow row in dgvPrices.Rows)
                {
                    if (row.IsNewRow) continue;

                    string game = row.Cells["Game"].Value?.ToString();
                    string item = row.Cells["Item"].Value?.ToString();
                    string manualPriceStr = row.Cells["ManualPrice"].Value?.ToString();

                    if (!string.IsNullOrEmpty(game) && !string.IsNullOrEmpty(item))
                    {
                        if (!string.IsNullOrEmpty(manualPriceStr) && decimal.TryParse(manualPriceStr, out decimal manualPrice))
                        {
                            dataService.SetManualPrice(item, game, manualPrice);
                            savedCount++;
                            row.Cells["Status"].Value = "РУЧНАЯ";
                            row.DefaultCellStyle.BackColor = Color.LightGreen;
                        }
                        else if (string.IsNullOrEmpty(manualPriceStr))
                        {
                            // Если поле пустое, удаляем ручную цену
                            dataService.RemoveManualPrice(item, game);
                            row.Cells["Status"].Value = "АВТО";
                            row.DefaultCellStyle.BackColor = Color.White;
                        }
                    }
                }

                MessageBox.Show($"Сохранено {savedCount} ручных цен!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении цен: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Очистить ВСЕ ручные цены? Это действие нельзя отменить.",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                // Используем метод для очистки всех цен
                var manualPrices = dataService.GetAllManualPrices();
                foreach (var price in manualPrices)
                {
                    // Разбираем cacheKey на game и item
                    string[] parts = price.Key.Split('_');
                    if (parts.Length >= 2)
                    {
                        string game = parts[0];
                        string item = string.Join("_", parts, 1, parts.Length - 1);
                        dataService.RemoveManualPrice(item, game);
                    }
                }

                LoadPrices();
                MessageBox.Show("Все ручные цены очищены!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}