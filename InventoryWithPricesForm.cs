using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventoryTrackerApp
{
    public class InventoryWithPricesForm : Form
    {
        private DataService dataService;
        private DataGridView dgv;

        public InventoryWithPricesForm(DataService dataService)
        {
            this.dataService = dataService;
            InitializeForm();
            CreateProperForm();
            LoadData();
        }

        private void InitializeForm()
        {
            this.Text = "💰 Ручные цены Steam";
            this.Size = new Size(1200, 600); // Увеличил ширину для изображений
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
        }

        private void CreateProperForm()
        {
            // Заголовок - ВЕРХ
            Label titleLabel = new Label();
            titleLabel.Text = "💰 РУЧНЫЕ ЦЕНЫ STEAM";
            titleLabel.Font = new Font("Arial", 14, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.BackColor = Color.SteelBlue;
            titleLabel.Location = new Point(0, 0);
            titleLabel.Size = new Size(1200, 50);
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(titleLabel);

            // Панель управления - ПОД ЗАГОЛОВКОМ
            Panel controlPanel = new Panel();
            controlPanel.Location = new Point(0, 50);
            controlPanel.Size = new Size(1200, 50);
            controlPanel.BackColor = Color.LightGray;
            this.Controls.Add(controlPanel);

            Button btnManualPrices = new Button();
            btnManualPrices.Text = "⚙️ Управление ценами";
            btnManualPrices.Size = new Size(150, 30);
            btnManualPrices.Location = new Point(20, 10);
            btnManualPrices.BackColor = Color.LightBlue;
            btnManualPrices.Font = new Font("Arial", 9, FontStyle.Bold);
            btnManualPrices.Click += (s, e) => ShowManagePricesForm();
            controlPanel.Controls.Add(btnManualPrices);

            Button btnRefresh = new Button();
            btnRefresh.Text = "🔄 Обновить";
            btnRefresh.Size = new Size(100, 30);
            btnRefresh.Location = new Point(180, 10);
            btnRefresh.BackColor = Color.LightGreen;
            btnRefresh.Font = new Font("Arial", 9, FontStyle.Bold);
            btnRefresh.Click += (s, e) => LoadData();
            controlPanel.Controls.Add(btnRefresh);

            // Кнопка управления кэшем изображений
            Button btnImageCache = new Button();
            btnImageCache.Text = "🖼️ Управление кэшем";
            btnImageCache.Size = new Size(140, 30);
            btnImageCache.Location = new Point(290, 10);
            btnImageCache.BackColor = Color.LightSalmon;
            btnImageCache.Font = new Font("Arial", 9, FontStyle.Bold);
            btnImageCache.Click += (s, e) => ShowImageCacheManagement();
            controlPanel.Controls.Add(btnImageCache);

            Button btnClose = new Button();
            btnClose.Text = "Закрыть";
            btnClose.Size = new Size(80, 30);
            btnClose.Location = new Point(440, 10);
            btnClose.BackColor = Color.LightCoral;
            btnClose.Font = new Font("Arial", 9, FontStyle.Bold);
            btnClose.Click += (s, e) => this.Close();
            controlPanel.Controls.Add(btnClose);

            // Статус бар - ПОД ПАНЕЛЬЮ УПРАВЛЕНИЯ
            Panel statusPanel = new Panel();
            statusPanel.Location = new Point(0, 100);
            statusPanel.Size = new Size(1200, 30);
            statusPanel.BackColor = Color.LightYellow;
            this.Controls.Add(statusPanel);

            Label statusLabel = new Label();
            statusLabel.Name = "statusLabel";
            statusLabel.Text = "Загрузка...";
            statusLabel.Location = new Point(10, 5);
            statusLabel.Size = new Size(1180, 20);
            statusLabel.Font = new Font("Arial", 9, FontStyle.Bold);
            statusLabel.ForeColor = Color.DarkBlue;
            statusPanel.Controls.Add(statusLabel);

            // Таблица - ПОД СТАТУС БАРОМ
            dgv = new DataGridView();
            dgv.Name = "dataGridView";
            dgv.Location = new Point(10, 140);
            dgv.Size = new Size(1180, 420);
            dgv.BackgroundColor = Color.White;
            dgv.ReadOnly = true;
            dgv.RowHeadersVisible = false;
            dgv.Font = new Font("Arial", 9);
            dgv.AllowUserToAddRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowTemplate.Height = 40; // Увеличиваем высоту строк для изображений
            this.Controls.Add(dgv);
        }

        private async void LoadData()
        {
            try
            {
                UpdateStatus("🔄 Загрузка данных и изображений...");

                var pricesData = dataService.GetInventoryWithPrices();
                int itemsWithPrice = pricesData.Count(p => p.CurrentPrice > 0);
                int itemsWithoutPrice = pricesData.Count(p => p.CurrentPrice == 0);

                UpdateStatus($"Загружено {pricesData.Count} предметов. С ценами: {itemsWithPrice}, Без цен: {itemsWithoutPrice}");

                await UpdateDataGridWithImages(pricesData);
            }
            catch (Exception ex)
            {
                UpdateStatus($"❌ Ошибка: {ex.Message}");
            }
        }

        private async Task UpdateDataGridWithImages(List<InventoryItemWithPrice> data)
        {
            dgv.Columns.Clear();
            dgv.Rows.Clear();

            if (data.Count == 0)
            {
                dgv.Columns.Add("Empty", "Информация");
                dgv.Rows.Add("📦 Инвентарь пуст! Добавьте сделки через 'Добавить сделку'");
                dgv.Rows[0].DefaultCellStyle.Font = new Font("Arial", 11, FontStyle.Bold);
                dgv.Rows[0].DefaultCellStyle.ForeColor = Color.DarkBlue;
                dgv.Rows[0].DefaultCellStyle.BackColor = Color.LightYellow;
                dgv.Rows[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                return;
            }

            // Создаем колонки с изображением
            var imageColumn = new DataGridViewImageColumn();
            imageColumn.Name = "Image";
            imageColumn.HeaderText = "";
            imageColumn.Width = 40;
            imageColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
            dgv.Columns.Add(imageColumn);

            // Остальные колонки
            string[] columns = {
                "Игра", "Предмет", "Кол-во",
                "Цена покупки", "Текущая цена", "Источник", "Прибыль", "Статус", "Рекомендация"
            };

            foreach (string col in columns)
                dgv.Columns.Add(col, col);

            // Показываем прогресс
            UpdateStatus($"🔄 Загрузка изображений... 0/{data.Count}");

            // Загружаем изображения и заполняем таблицу
            for (int i = 0; i < data.Count; i++)
            {
                var item = data[i];
                var inventoryItem = item.Item;

                // Обновляем статус каждые 5 предметов
                if (i % 5 == 0 || i == data.Count - 1)
                {
                    UpdateStatus($"🔄 Загрузка изображений... {i + 1}/{data.Count}");
                    await Task.Delay(10); // Даем UI обновиться
                }

                decimal avgPrice = inventoryItem.Quantity > 0 ? inventoryItem.TotalPurchase / inventoryItem.Quantity : 0;

                string status = "";
                Color backColor = Color.White;
                Color foreColor = Color.Black;

                if (inventoryItem.Quantity == 0)
                {
                    status = "Нет в наличии";
                    backColor = Color.LightGray;
                    foreColor = Color.Gray;
                }
                else if (item.CurrentPrice == 0)
                {
                    status = "❌ НЕТ ЦЕНЫ";
                    backColor = Color.LightCoral;
                    foreColor = Color.DarkRed;
                }
                else if (item.PotentialProfit > 0)
                {
                    status = "✅ ПРИБЫЛЬ";
                    backColor = Color.Honeydew;
                    foreColor = Color.DarkGreen;
                }
                else if (item.PotentialProfit < 0)
                {
                    status = "🔴 УБЫТОК";
                    backColor = Color.LavenderBlush;
                    foreColor = Color.DarkRed;
                }
                else
                {
                    status = "⚪ В НОЛЬ";
                    backColor = Color.LightYellow;
                    foreColor = Color.DarkOrange;
                }

                // Загружаем изображение
                Image itemImage = await dataService.GetItemImageAsync(inventoryItem.Name, inventoryItem.Game, 48);

                int idx = dgv.Rows.Add(
                    itemImage, // Изображение в первой колонке
                    inventoryItem.Game,
                    inventoryItem.Name,
                    inventoryItem.Quantity + " шт.",
                    avgPrice.ToString("0.00") + " руб.",
                    item.CurrentPrice > 0 ? item.CurrentPrice.ToString("0.00") + " руб." : "❌ НЕТ ЦЕНЫ",
                    item.PriceSource,
                    item.PotentialProfit.ToString("0.00") + " руб.",
                    status,
                    item.Recommendation
                );

                // Подсветка для источников цен
                if (item.PriceSource == "РУЧНАЯ")
                {
                    backColor = Color.LightCyan;
                    if (status == "✅ ПРИБЫЛЬ") backColor = Color.FromArgb(200, 255, 200);
                    if (status == "🔴 УБЫТОК") backColor = Color.FromArgb(255, 200, 200);
                }
                else if (item.PriceSource == "STEAM WEB")
                {
                    backColor = Color.LightGreen;
                }

                dgv.Rows[idx].DefaultCellStyle.BackColor = backColor;
                dgv.Rows[idx].DefaultCellStyle.ForeColor = foreColor;
            }

            // Настраиваем ширину колонок
            dgv.Columns["Image"].Width = 40;
            dgv.Columns["Игра"].Width = 120;
            dgv.Columns["Предмет"].Width = 200;
            dgv.Columns["Кол-во"].Width = 80;
            dgv.Columns["Цена покупки"].Width = 120;
            dgv.Columns["Текущая цена"].Width = 120;
            dgv.Columns["Источник"].Width = 100;
            dgv.Columns["Прибыль"].Width = 120;
            dgv.Columns["Статус"].Width = 100;
            dgv.Columns["Рекомендация"].Width = 150;

            UpdateStatus($"✅ Загружено {data.Count} предметов с изображениями");
        }

        private void ShowManagePricesForm()
        {
            using (var form = new ManagePricesForm(dataService))
                form.ShowDialog();

            // Перезагружаем данные после закрытия формы управления ценами
            LoadData();
        }

        private void ShowImageCacheManagement()
        {
            long cacheSize = dataService.GetImageCacheSize();
            string cacheInfo = $"Размер кэша: {cacheSize / 1024 / 1024} MB\n\nОчистить кэш изображений?";

            var result = MessageBox.Show(cacheInfo, "Управление кэшем изображений",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                dataService.ClearImageCache();
                LoadData(); // Перезагружаем чтобы обновились изображения
                MessageBox.Show("Кэш изображений очищен!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void UpdateStatus(string message)
        {
            foreach (Control control in this.Controls)
            {
                if (control is Panel panel && panel.BackColor == Color.LightYellow)
                {
                    foreach (Control label in panel.Controls)
                    {
                        if (label is Label statusLabel)
                        {
                            statusLabel.Text = message;
                            return;
                        }
                    }
                }
            }
        }
    }
}