using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace InventoryTrackerApp
{
    public class ImportInventoryForm : Form
    {
        private DataService dataService;
        private DataGridView dgvInventory;
        private List<CSVInventoryItem> loadedItems;
        private List<CSVInventoryItem> filteredItems;
        private Button btnImportSelected;
        private ComboBox cmbSortBy;
        private ComboBox cmbFilterGame;
        private TextBox txtSearch;
        private CheckBox chkOnlyRemaining;

        public ImportInventoryForm(DataService dataService)
        {
            this.dataService = dataService;
            this.loadedItems = new List<CSVInventoryItem>();
            this.filteredItems = new List<CSVInventoryItem>();
            InitializeForm();
            CreateControls();
        }

        private void InitializeForm()
        {
            this.Text = "📥 Импорт инвентаря из CSV";
            this.Size = new Size(1400, 700); // Увеличил высоту для панели управления
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
        }

        private void CreateControls()
        {
            // Заголовок
            Label titleLabel = new Label();
            titleLabel.Text = "📥 ИМПОРТ ИНВЕНТАРЯ ИЗ CSV ФАЙЛА";
            titleLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.BackColor = Color.FromArgb(52, 152, 219);
            titleLabel.Location = new Point(0, 0);
            titleLabel.Size = new Size(1400, 50);
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(titleLabel);

            int yPos = 60;

            // Панель управления
            Panel controlPanel = new Panel();
            controlPanel.Location = new Point(20, yPos);
            controlPanel.Size = new Size(1360, 80);
            controlPanel.BackColor = Color.FromArgb(240, 240, 240);
            controlPanel.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(controlPanel);

            // Кнопка загрузки файла
            Button btnLoadFile = new Button();
            btnLoadFile.Text = "📁 Загрузить CSV файл";
            btnLoadFile.Location = new Point(10, 10);
            btnLoadFile.Size = new Size(150, 30);
            btnLoadFile.BackColor = Color.FromArgb(46, 204, 113);
            btnLoadFile.ForeColor = Color.White;
            btnLoadFile.Font = new Font("Arial", 9, FontStyle.Bold);
            btnLoadFile.FlatStyle = FlatStyle.Flat;
            btnLoadFile.FlatAppearance.BorderSize = 0;
            btnLoadFile.Click += BtnLoadFile_Click;
            controlPanel.Controls.Add(btnLoadFile);

            // Поиск по названию
            Label lblSearch = new Label();
            lblSearch.Text = "Поиск:";
            lblSearch.Location = new Point(170, 15);
            lblSearch.AutoSize = true;
            lblSearch.Font = new Font("Arial", 9, FontStyle.Bold);
            controlPanel.Controls.Add(lblSearch);

            txtSearch = new TextBox();
            txtSearch.Location = new Point(220, 12);
            txtSearch.Size = new Size(150, 20);
            txtSearch.Font = new Font("Arial", 9);
            txtSearch.TextChanged += TxtSearch_TextChanged;
            controlPanel.Controls.Add(txtSearch);

            // Фильтр по игре
            Label lblFilterGame = new Label();
            lblFilterGame.Text = "Игра:";
            lblFilterGame.Location = new Point(380, 15);
            lblFilterGame.AutoSize = true;
            lblFilterGame.Font = new Font("Arial", 9, FontStyle.Bold);
            controlPanel.Controls.Add(lblFilterGame);

            cmbFilterGame = new ComboBox();
            cmbFilterGame.Location = new Point(420, 12);
            cmbFilterGame.Size = new Size(150, 21);
            cmbFilterGame.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFilterGame.Font = new Font("Arial", 9);
            cmbFilterGame.SelectedIndexChanged += FilterChanged;
            controlPanel.Controls.Add(cmbFilterGame);

            // Сортировка
            Label lblSort = new Label();
            lblSort.Text = "Сортировка:";
            lblSort.Location = new Point(580, 15);
            lblSort.AutoSize = true;
            lblSort.Font = new Font("Arial", 9, FontStyle.Bold);
            controlPanel.Controls.Add(lblSort);

            cmbSortBy = new ComboBox();
            cmbSortBy.Location = new Point(650, 12);
            cmbSortBy.Size = new Size(150, 21);
            cmbSortBy.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSortBy.Font = new Font("Arial", 9);
            cmbSortBy.Items.AddRange(new string[] {
                "По названию (А-Я)",
                "По названию (Я-А)",
                "По игре (А-Я)",
                "По игре (Я-А)",
                "По цене (↑)",
                "По цене (↓)",
                "По количеству (↑)",
                "По количеству (↓)",
                "По остатку (↑)",
                "По остатку (↓)"
            });
            cmbSortBy.SelectedIndex = 0;
            cmbSortBy.SelectedIndexChanged += SortData;
            controlPanel.Controls.Add(cmbSortBy);

            // Чекбокс "Только с остатком"
            chkOnlyRemaining = new CheckBox();
            chkOnlyRemaining.Text = "Только с остатком";
            chkOnlyRemaining.Location = new Point(810, 14);
            chkOnlyRemaining.Size = new Size(130, 20);
            chkOnlyRemaining.Font = new Font("Arial", 9);
            chkOnlyRemaining.CheckedChanged += FilterChanged;
            controlPanel.Controls.Add(chkOnlyRemaining);

            // Кнопка сброса фильтров
            Button btnResetFilters = new Button();
            btnResetFilters.Text = "🔄 Сброс";
            btnResetFilters.Location = new Point(950, 10);
            btnResetFilters.Size = new Size(80, 25);
            btnResetFilters.BackColor = Color.LightGray;
            btnResetFilters.Font = new Font("Arial", 8, FontStyle.Bold);
            btnResetFilters.FlatStyle = FlatStyle.Flat;
            btnResetFilters.FlatAppearance.BorderSize = 0;
            btnResetFilters.Click += BtnResetFilters_Click;
            controlPanel.Controls.Add(btnResetFilters);

            // Статистика
            Label lblStats = new Label();
            lblStats.Name = "lblStats";
            lblStats.Text = "Загрузите CSV файл для начала работы";
            lblStats.Location = new Point(10, 45);
            lblStats.Size = new Size(500, 20);
            lblStats.Font = new Font("Arial", 9, FontStyle.Italic);
            lblStats.ForeColor = Color.DarkBlue;
            controlPanel.Controls.Add(lblStats);

            yPos += 90;

            // Таблица инвентаря
            dgvInventory = new DataGridView();
            dgvInventory.Location = new Point(20, yPos);
            dgvInventory.Size = new Size(1360, 350);
            dgvInventory.BackgroundColor = Color.White;
            dgvInventory.RowHeadersVisible = false;
            dgvInventory.Font = new Font("Arial", 8);
            dgvInventory.AllowUserToAddRows = false;
            dgvInventory.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvInventory.MultiSelect = true;
            dgvInventory.EditMode = DataGridViewEditMode.EditOnEnter;

            CreateDataGridColumns();
            this.Controls.Add(dgvInventory);

            yPos += 360;

            // Панель действий
            Panel actionPanel = new Panel();
            actionPanel.Location = new Point(20, yPos);
            actionPanel.Size = new Size(1360, 50);
            actionPanel.BackColor = Color.FromArgb(250, 250, 250);
            this.Controls.Add(actionPanel);

            // Кнопка выделить все
            Button btnSelectAll = new Button();
            btnSelectAll.Text = "✓ Выделить все";
            btnSelectAll.Location = new Point(10, 10);
            btnSelectAll.Size = new Size(120, 30);
            btnSelectAll.BackColor = Color.LightBlue;
            btnSelectAll.Font = new Font("Arial", 9, FontStyle.Bold);
            btnSelectAll.FlatStyle = FlatStyle.Flat;
            btnSelectAll.FlatAppearance.BorderSize = 0;
            btnSelectAll.Click += BtnSelectAll_Click;
            actionPanel.Controls.Add(btnSelectAll);

            // Кнопка снять выделение
            Button btnDeselectAll = new Button();
            btnDeselectAll.Text = "✗ Снять все";
            btnDeselectAll.Location = new Point(140, 10);
            btnDeselectAll.Size = new Size(120, 30);
            btnDeselectAll.BackColor = Color.LightCoral;
            btnDeselectAll.Font = new Font("Arial", 9, FontStyle.Bold);
            btnDeselectAll.FlatStyle = FlatStyle.Flat;
            btnDeselectAll.FlatAppearance.BorderSize = 0;
            btnDeselectAll.Click += BtnDeselectAll_Click;
            actionPanel.Controls.Add(btnDeselectAll);

            // Кнопка импорта выбранных
            btnImportSelected = new Button();
            btnImportSelected.Text = "➕ Добавить выбранные в сделки";
            btnImportSelected.Location = new Point(270, 10);
            btnImportSelected.Size = new Size(200, 30);
            btnImportSelected.BackColor = Color.FromArgb(241, 196, 15);
            btnImportSelected.ForeColor = Color.Black;
            btnImportSelected.Font = new Font("Arial", 10, FontStyle.Bold);
            btnImportSelected.FlatStyle = FlatStyle.Flat;
            btnImportSelected.FlatAppearance.BorderSize = 0;
            btnImportSelected.Enabled = false;
            btnImportSelected.Click += BtnImportSelected_Click;
            actionPanel.Controls.Add(btnImportSelected);

            Button btnClose = new Button();
            btnClose.Text = "Закрыть";
            btnClose.Location = new Point(480, 10);
            btnClose.Size = new Size(80, 30);
            btnClose.BackColor = Color.LightGray;
            btnClose.Font = new Font("Arial", 9, FontStyle.Bold);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            actionPanel.Controls.Add(btnClose);

            // Добавляем обработчик для валидации ввода количества
            dgvInventory.CellValidating += DgvInventory_CellValidating;
        }

        private void CreateDataGridColumns()
        {
            dgvInventory.Columns.Clear();

            // 1. Колонка с чекбоксом
            DataGridViewCheckBoxColumn selectColumn = new DataGridViewCheckBoxColumn();
            selectColumn.Name = "Select";
            selectColumn.HeaderText = "✓";
            selectColumn.Width = 30;
            selectColumn.FalseValue = false;
            selectColumn.TrueValue = true;
            dgvInventory.Columns.Add(selectColumn);

            // 2. Игра
            DataGridViewTextBoxColumn gameColumn = new DataGridViewTextBoxColumn();
            gameColumn.Name = "Game";
            gameColumn.HeaderText = "Игра";
            gameColumn.Width = 120;
            gameColumn.ReadOnly = true;
            dgvInventory.Columns.Add(gameColumn);

            // 3. Предмет
            DataGridViewTextBoxColumn itemColumn = new DataGridViewTextBoxColumn();
            itemColumn.Name = "ItemName";
            itemColumn.HeaderText = "Предмет";
            itemColumn.Width = 200;
            itemColumn.ReadOnly = true;
            dgvInventory.Columns.Add(itemColumn);

            // 4. Всего в CSV
            DataGridViewTextBoxColumn totalQuantityColumn = new DataGridViewTextBoxColumn();
            totalQuantityColumn.Name = "TotalQuantity";
            totalQuantityColumn.HeaderText = "Всего в CSV";
            totalQuantityColumn.Width = 80;
            totalQuantityColumn.ReadOnly = true;
            dgvInventory.Columns.Add(totalQuantityColumn);

            // 5. Уже добавлено
            DataGridViewTextBoxColumn addedQuantityColumn = new DataGridViewTextBoxColumn();
            addedQuantityColumn.Name = "AddedQuantity";
            addedQuantityColumn.HeaderText = "Уже добавлено";
            addedQuantityColumn.Width = 90;
            addedQuantityColumn.ReadOnly = true;
            dgvInventory.Columns.Add(addedQuantityColumn);

            // 6. Осталось добавить
            DataGridViewTextBoxColumn remainingQuantityColumn = new DataGridViewTextBoxColumn();
            remainingQuantityColumn.Name = "RemainingQuantity";
            remainingQuantityColumn.HeaderText = "Осталось";
            remainingQuantityColumn.Width = 80;
            remainingQuantityColumn.ReadOnly = true;
            dgvInventory.Columns.Add(remainingQuantityColumn);

            // 7. Кол-во для импорта
            DataGridViewTextBoxColumn importQuantityColumn = new DataGridViewTextBoxColumn();
            importQuantityColumn.Name = "ImportQuantity";
            importQuantityColumn.HeaderText = "Импортировать";
            importQuantityColumn.Width = 90;
            importQuantityColumn.ReadOnly = false;
            dgvInventory.Columns.Add(importQuantityColumn);

            // 8. Цена
            DataGridViewTextBoxColumn priceColumn = new DataGridViewTextBoxColumn();
            priceColumn.Name = "CurrentPrice";
            priceColumn.HeaderText = "Цена";
            priceColumn.Width = 80;
            priceColumn.ReadOnly = true;
            dgvInventory.Columns.Add(priceColumn);

            // 9. Тип (CS2)
            DataGridViewTextBoxColumn typeColumn = new DataGridViewTextBoxColumn();
            typeColumn.Name = "Type";
            typeColumn.HeaderText = "Тип";
            typeColumn.Width = 80;
            typeColumn.ReadOnly = true;
            dgvInventory.Columns.Add(typeColumn);

            // 10. Качество (CS2)
            DataGridViewTextBoxColumn qualityColumn = new DataGridViewTextBoxColumn();
            qualityColumn.Name = "Quality";
            qualityColumn.HeaderText = "Качество";
            qualityColumn.Width = 80;
            qualityColumn.ReadOnly = true;
            dgvInventory.Columns.Add(qualityColumn);

            // 11. Exterior (CS2)
            DataGridViewTextBoxColumn exteriorColumn = new DataGridViewTextBoxColumn();
            exteriorColumn.Name = "Exterior";
            exteriorColumn.HeaderText = "Exterior";
            exteriorColumn.Width = 80;
            exteriorColumn.ReadOnly = true;
            dgvInventory.Columns.Add(exteriorColumn);
        }

        private void BtnSelectAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvInventory.Rows)
            {
                if (!row.IsNewRow && row.Cells["Select"].ReadOnly == false)
                {
                    row.Cells["Select"].Value = true;
                }
            }
        }

        private void BtnDeselectAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvInventory.Rows)
            {
                if (!row.IsNewRow)
                {
                    row.Cells["Select"].Value = false;
                }
            }
        }

        private void BtnResetFilters_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            cmbFilterGame.SelectedIndex = -1;
            cmbSortBy.SelectedIndex = 0;
            chkOnlyRemaining.Checked = false;
            ApplyFiltersAndSort();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFiltersAndSort();
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            ApplyFiltersAndSort();
        }

        private void SortData(object sender, EventArgs e)
        {
            ApplyFiltersAndSort();
        }

        private void ApplyFiltersAndSort()
        {
            if (loadedItems.Count == 0) return;

            // Фильтрация
            filteredItems = loadedItems.Where(item =>
            {
                // Поиск по названию
                if (!string.IsNullOrEmpty(txtSearch.Text) &&
                    !item.ItemName.ToLower().Contains(txtSearch.Text.ToLower()))
                    return false;

                // Фильтр по игре
                if (cmbFilterGame.SelectedItem != null &&
                    item.Game != cmbFilterGame.SelectedItem.ToString())
                    return false;

                // Фильтр "Только с остатком"
                if (chkOnlyRemaining.Checked)
                {
                    int alreadyAdded = dataService.GetItemQuantity(item.Game, item.ItemName);
                    int remaining = Math.Max(0, item.Quantity - alreadyAdded);
                    if (remaining <= 0)
                        return false;
                }

                return true;
            }).ToList();

            // Сортировка
            switch (cmbSortBy.SelectedIndex)
            {
                case 0: // По названию (А-Я)
                    filteredItems = filteredItems.OrderBy(x => x.ItemName).ToList();
                    break;
                case 1: // По названию (Я-А)
                    filteredItems = filteredItems.OrderByDescending(x => x.ItemName).ToList();
                    break;
                case 2: // По игре (А-Я)
                    filteredItems = filteredItems.OrderBy(x => x.Game).ThenBy(x => x.ItemName).ToList();
                    break;
                case 3: // По игре (Я-А)
                    filteredItems = filteredItems.OrderByDescending(x => x.Game).ThenBy(x => x.ItemName).ToList();
                    break;
                case 4: // По цене (↑)
                    filteredItems = filteredItems.OrderBy(x => ParsePrice(x.CurrentPrice)).ToList();
                    break;
                case 5: // По цене (↓)
                    filteredItems = filteredItems.OrderByDescending(x => ParsePrice(x.CurrentPrice)).ToList();
                    break;
                case 6: // По количеству (↑)
                    filteredItems = filteredItems.OrderBy(x => x.Quantity).ToList();
                    break;
                case 7: // По количеству (↓)
                    filteredItems = filteredItems.OrderByDescending(x => x.Quantity).ToList();
                    break;
                case 8: // По остатку (↑)
                    filteredItems = filteredItems.OrderBy(x =>
                    {
                        int alreadyAdded = dataService.GetItemQuantity(x.Game, x.ItemName);
                        return Math.Max(0, x.Quantity - alreadyAdded);
                    }).ToList();
                    break;
                case 9: // По остатку (↓)
                    filteredItems = filteredItems.OrderByDescending(x =>
                    {
                        int alreadyAdded = dataService.GetItemQuantity(x.Game, x.ItemName);
                        return Math.Max(0, x.Quantity - alreadyAdded);
                    }).ToList();
                    break;
            }

            UpdateDataGrid();
            UpdateStatistics();
        }

        private decimal ParsePrice(string priceText)
        {
            if (string.IsNullOrEmpty(priceText) || priceText == "N/A")
                return 0;

            try
            {
                // Убираем валюту и лишние символы
                string cleanText = priceText
                    .Replace(" руб.", "")
                    .Replace(" pуб.", "")
                    .Replace(" RUB", "")
                    .Replace(" ", "")
                    .Replace("$", "")
                    .Replace("€", "")
                    .Replace("\"", "")
                    .Replace("đóá.", "")
                    .Replace(",", ".");

                if (decimal.TryParse(cleanText, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal price))
                    return price;

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private void UpdateStatistics()
        {
            int totalItems = filteredItems.Count;
            int totalQuantity = filteredItems.Sum(x => x.Quantity);
            int selectedCount = dgvInventory.Rows.Cast<DataGridViewRow>()
                .Count(row => !row.IsNewRow && row.Cells["Select"].Value is bool isSelected && isSelected);

            var statsLabel = this.Controls.Find("lblStats", true).FirstOrDefault() as Label;
            if (statsLabel != null)
            {
                statsLabel.Text = $"📊 Показано: {totalItems} предметов | 📦 Всего: {totalQuantity} шт. | ✅ Выбрано: {selectedCount}";
            }
        }

        private void DgvInventory_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex == dgvInventory.Columns["ImportQuantity"].Index)
            {
                string newValue = e.FormattedValue?.ToString();

                if (!string.IsNullOrEmpty(newValue))
                {
                    if (!int.TryParse(newValue, out int quantity) || quantity <= 0)
                    {
                        MessageBox.Show("Введите положительное число для количества", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        e.Cancel = true;
                        return;
                    }

                    if (e.RowIndex >= 0 && e.RowIndex < dgvInventory.Rows.Count)
                    {
                        var remainingQuantityCell = dgvInventory.Rows[e.RowIndex].Cells["RemainingQuantity"];
                        if (remainingQuantityCell.Value != null)
                        {
                            string remainingStr = remainingQuantityCell.Value.ToString();
                            if (int.TryParse(remainingStr, out int remainingQuantity))
                            {
                                if (quantity > remainingQuantity)
                                {
                                    MessageBox.Show($"Количество не может превышать {remainingQuantity} (осталось добавить)", "Ошибка",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    e.Cancel = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void BtnLoadFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                openFileDialog.Title = "Выберите CSV файл инвентаря";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        LoadCSVFile(openFileDialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка загрузки файла: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void LoadCSVFile(string filePath)
        {
            loadedItems.Clear();
            dgvInventory.Rows.Clear();

            var lines = File.ReadAllLines(filePath);
            if (lines.Length == 0) return;

            // Читаем заголовок чтобы определить структуру CSV
            var headers = ParseCSVLine(lines[0]);
            bool hasCs2Attributes = headers.Contains("Type") && headers.Contains("Exterior");

            // Пропускаем заголовок
            for (int i = 1; i < lines.Length; i++)
            {
                var columns = ParseCSVLine(lines[i]);
                if (columns.Length >= 6)
                {
                    var item = new CSVInventoryItem
                    {
                        Game = GetColumnValue(headers, columns, "Game"),
                        ItemName = GetColumnValue(headers, columns, "ItemName"),
                        Quantity = int.TryParse(GetColumnValue(headers, columns, "Quantity"), out int qty) ? qty : 1,
                        CurrentPrice = GetColumnValue(headers, columns, "CurrentPrice"),
                        Marketable = GetColumnValue(headers, columns, "Marketable"),
                        Tradable = GetColumnValue(headers, columns, "Tradable")
                    };

                    if (hasCs2Attributes)
                    {
                        item.Type = GetColumnValue(headers, columns, "Type");
                        item.Category = GetColumnValue(headers, columns, "Category");
                        item.Quality = GetColumnValue(headers, columns, "Quality");
                        item.Rarity = GetColumnValue(headers, columns, "Rarity");
                        item.Exterior = GetColumnValue(headers, columns, "Exterior");
                        item.WeaponType = GetColumnValue(headers, columns, "WeaponType");
                    }

                    loadedItems.Add(item);
                }
            }

            // Заполняем фильтр игр
            var games = loadedItems.Select(x => x.Game).Distinct().OrderBy(x => x).ToList();
            cmbFilterGame.Items.Clear();
            cmbFilterGame.Items.AddRange(games.ToArray());

            ApplyFiltersAndSort();
            btnImportSelected.Enabled = loadedItems.Count > 0;

            MessageBox.Show($"✅ Загружено {loadedItems.Count} предметов из CSV файла\n\n" +
                "🎯 ВОЗМОЖНОСТИ:\n" +
                "• Поиск по названию предмета\n" +
                "• Фильтрация по игре\n" +
                "• Сортировка по разным параметрам\n" +
                "• Показывать только предметы с остатком\n" +
                "• Цветовая подсветка статуса добавления",
                "Успешная загрузка",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void UpdateDataGrid()
        {
            dgvInventory.Rows.Clear();

            foreach (var item in filteredItems)
            {
                int alreadyAdded = dataService.GetItemQuantity(item.Game, item.ItemName);
                int remaining = Math.Max(0, item.Quantity - alreadyAdded);

                int rowIndex = dgvInventory.Rows.Add(
                    false, // Checkbox
                    item.Game,
                    item.ItemName,
                    item.Quantity, // Всего в CSV
                    alreadyAdded, // Уже добавлено
                    remaining, // Осталось добавить
                    remaining, // Кол-во для импорта
                    item.CurrentPrice,
                    item.Type ?? "",
                    item.Quality ?? "",
                    item.Exterior ?? ""
                );

                // Подсветка строк
                Color rowColor = GetRowColor(alreadyAdded, item.Quantity, remaining);
                dgvInventory.Rows[rowIndex].DefaultCellStyle.BackColor = rowColor;

                // Если все уже добавлено, делаем строку неактивной
                if (remaining == 0)
                {
                    dgvInventory.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.Gray;
                    dgvInventory.Rows[rowIndex].Cells["Select"].ReadOnly = true;
                    dgvInventory.Rows[rowIndex].Cells["ImportQuantity"].ReadOnly = true;
                }
            }

            UpdateStatistics();
        }

        private Color GetRowColor(int alreadyAdded, int totalQuantity, int remaining)
        {
            if (alreadyAdded == 0) return Color.White; // Не добавлялись
            if (alreadyAdded >= totalQuantity) return Color.LightGreen; // Все добавлены
            if (alreadyAdded > 0) return Color.LightYellow; // Часть добавлена
            return Color.White;
        }

        private string GetColumnValue(string[] headers, string[] columns, string columnName)
        {
            int index = Array.IndexOf(headers, columnName);
            return index >= 0 && index < columns.Length ? columns[index] : "";
        }

        private string[] ParseCSVLine(string line)
        {
            var result = new List<string>();
            var current = "";
            var inQuotes = false;

            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current);
                    current = "";
                }
                else
                {
                    current += c;
                }
            }

            result.Add(current);
            return result.ToArray();
        }

        private void BtnImportSelected_Click(object sender, EventArgs e)
        {
            var selectedItems = new List<CSVInventoryItem>();

            for (int i = 0; i < dgvInventory.Rows.Count; i++)
            {
                var row = dgvInventory.Rows[i];
                if (row.Cells["Select"].Value is bool isSelected && isSelected)
                {
                    var originalItem = filteredItems[i];

                    int importQuantity = 0;
                    var importQuantityCell = row.Cells["ImportQuantity"];
                    if (importQuantityCell.Value != null)
                    {
                        string quantityStr = importQuantityCell.Value.ToString();
                        if (int.TryParse(quantityStr, out int customQuantity) && customQuantity > 0)
                        {
                            importQuantity = customQuantity;
                        }
                    }

                    var remainingCell = row.Cells["RemainingQuantity"];
                    if (remainingCell.Value != null)
                    {
                        string remainingStr = remainingCell.Value.ToString();
                        if (int.TryParse(remainingStr, out int remainingQuantity))
                        {
                            importQuantity = Math.Min(importQuantity, remainingQuantity);
                        }
                    }

                    if (importQuantity > 0)
                    {
                        var selectedItem = new CSVInventoryItem
                        {
                            Game = originalItem.Game,
                            ItemName = originalItem.ItemName,
                            Quantity = importQuantity
                        };

                        selectedItems.Add(selectedItem);
                    }
                }
            }

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Выберите предметы для добавления в сделки", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var selectForm = new SelectOperationForm(selectedItems, dataService))
            {
                if (selectForm.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show($"✅ Успешно добавлено {selectForm.AddedCount} сделок!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Обновляем данные
                    ApplyFiltersAndSort();
                }
            }
        }
    }

    public class CSVInventoryItem
    {
        public string Game { get; set; }
        public string ItemName { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public string Quality { get; set; }
        public string Rarity { get; set; }
        public string Exterior { get; set; }
        public string WeaponType { get; set; }
        public int Quantity { get; set; }
        public string CurrentPrice { get; set; }
        public string Marketable { get; set; }
        public string Tradable { get; set; }
    }
}