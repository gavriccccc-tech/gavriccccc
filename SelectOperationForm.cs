using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace InventoryTrackerApp
{
    public class SelectOperationForm : Form
    {
        private List<CSVInventoryItem> selectedItems;
        private DataService dataService;
        public int AddedCount { get; private set; }

        public SelectOperationForm(List<CSVInventoryItem> items, DataService dataService)
        {
            this.selectedItems = items;
            this.dataService = dataService;
            this.AddedCount = 0;
            InitializeForm();
            CreateControls();
        }

        private void InitializeForm()
        {
            this.Text = "Выбор типа операции";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
        }

        private void CreateControls()
        {
            // Заголовок
            Label titleLabel = new Label();
            titleLabel.Text = "📥 ДОБАВЛЕНИЕ В СДЕЛКИ";
            titleLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.BackColor = Color.FromArgb(52, 152, 219);
            titleLabel.Location = new Point(0, 0);
            titleLabel.Size = new Size(600, 40);
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(titleLabel);

            int yPos = 50;

            // Информация о выбранных предметах
            Label infoLabel = new Label();
            infoLabel.Text = $"Выбрано предметов: {selectedItems.Count}";
            infoLabel.Location = new Point(20, yPos);
            infoLabel.Size = new Size(560, 20);
            infoLabel.Font = new Font("Arial", 10, FontStyle.Bold);
            this.Controls.Add(infoLabel);

            yPos += 30;

            // Таблица выбранных предметов
            DataGridView dgvSelected = new DataGridView();
            dgvSelected.Location = new Point(20, yPos);
            dgvSelected.Size = new Size(560, 150);
            dgvSelected.BackgroundColor = Color.White;
            dgvSelected.RowHeadersVisible = false;
            dgvSelected.Font = new Font("Arial", 9);
            dgvSelected.AllowUserToAddRows = false;
            dgvSelected.ReadOnly = true;

            dgvSelected.Columns.Add("Game", "Игра");
            dgvSelected.Columns.Add("Item", "Предмет");
            dgvSelected.Columns.Add("Quantity", "Количество");

            dgvSelected.Columns["Game"].Width = 120;
            dgvSelected.Columns["Item"].Width = 300;
            dgvSelected.Columns["Quantity"].Width = 80;

            // Заполняем таблицу
            foreach (var item in selectedItems)
            {
                int rowIndex = dgvSelected.Rows.Add(
                    item.Game,
                    item.ItemName,
                    item.Quantity + " шт."
                );

                // Подсветка
                dgvSelected.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
            }

            this.Controls.Add(dgvSelected);
            yPos += 160;

            // Тип операции
            Label lblOperation = new Label();
            lblOperation.Text = "Тип операции:";
            lblOperation.Location = new Point(20, yPos);
            lblOperation.AutoSize = true;
            lblOperation.Font = new Font("Arial", 10, FontStyle.Bold);
            this.Controls.Add(lblOperation);

            ComboBox cmbOperation = new ComboBox();
            cmbOperation.Location = new Point(150, yPos - 3);
            cmbOperation.Size = new Size(200, 25);
            cmbOperation.Font = new Font("Arial", 10);
            cmbOperation.Items.AddRange(new string[] { "Покупка", "Подарок", "Обмен", "Крафт" });
            cmbOperation.SelectedIndex = 0;
            this.Controls.Add(cmbOperation);

            yPos += 40;

            // Цена (только для покупки)
            Label lblPrice = new Label();
            lblPrice.Text = "Цена за шт. (руб):";
            lblPrice.Location = new Point(20, yPos);
            lblPrice.AutoSize = true;
            lblPrice.Font = new Font("Arial", 10, FontStyle.Bold);
            this.Controls.Add(lblPrice);

            NumericUpDown nudPrice = new NumericUpDown();
            nudPrice.Location = new Point(150, yPos - 3);
            nudPrice.Size = new Size(120, 25);
            nudPrice.Font = new Font("Arial", 10);
            nudPrice.DecimalPlaces = 2;
            nudPrice.Minimum = 0;
            nudPrice.Maximum = 1000000;
            nudPrice.Value = 0;
            this.Controls.Add(nudPrice);

            yPos += 40;

            // Панель информации
            Panel infoPanel = new Panel();
            infoPanel.Location = new Point(20, yPos);
            infoPanel.Size = new Size(560, 80);
            infoPanel.BackColor = Color.LightYellow;
            infoPanel.BorderStyle = BorderStyle.FixedSingle;

            Label operationInfo = new Label();
            operationInfo.Name = "operationInfo";
            operationInfo.Text = "💡 ПОКУПКА: вы покупаете предметы за деньги\n💰 Укажите цену покупки за штуку";
            operationInfo.Location = new Point(10, 10);
            operationInfo.Size = new Size(540, 60);
            operationInfo.Font = new Font("Arial", 9, FontStyle.Italic);
            operationInfo.ForeColor = Color.DarkBlue;
            infoPanel.Controls.Add(operationInfo);

            this.Controls.Add(infoPanel);
            yPos += 90;

            // Обработчик изменения типа операции
            cmbOperation.SelectedIndexChanged += (s, e) =>
            {
                string operation = cmbOperation.SelectedItem?.ToString() ?? "";

                switch (operation)
                {
                    case "Покупка":
                        operationInfo.Text = "💡 ПОКУПКА: вы покупаете предметы за деньги\n💰 Укажите цену покупки за штуку";
                        nudPrice.Enabled = true;
                        nudPrice.BackColor = Color.White;
                        break;
                    case "Подарок":
                        operationInfo.Text = "🎁 ПОДАРОК: вы получаете предметы бесплатно\n💰 Цена автоматически установится в 0";
                        nudPrice.Enabled = false;
                        nudPrice.Value = 0;
                        nudPrice.BackColor = Color.LightGray;
                        break;
                    case "Обмен":
                        operationInfo.Text = "🔄 ОБМЕН: вы получаете предметы по обмену\n💰 Цена автоматически установится в 0";
                        nudPrice.Enabled = false;
                        nudPrice.Value = 0;
                        nudPrice.BackColor = Color.LightGray;
                        break;
                    case "Крафт":
                        operationInfo.Text = "⚒️ КРАФТ: вы создаете предмет в игре\n💰 Цена автоматически установится в 0";
                        nudPrice.Enabled = false;
                        nudPrice.Value = 0;
                        nudPrice.BackColor = Color.LightGray;
                        break;
                }
            };

            // Кнопки
            Button btnAddAll = new Button();
            btnAddAll.Text = "✅ Добавить все сделки";
            btnAddAll.Location = new Point(50, yPos);
            btnAddAll.Size = new Size(200, 35);
            btnAddAll.BackColor = Color.FromArgb(46, 204, 113);
            btnAddAll.ForeColor = Color.White;
            btnAddAll.Font = new Font("Arial", 10, FontStyle.Bold);
            btnAddAll.Click += (s, e) => AddTransactions(cmbOperation, nudPrice);
            this.Controls.Add(btnAddAll);

            Button btnCancel = new Button();
            btnCancel.Text = "❌ Отмена";
            btnCancel.Location = new Point(260, yPos);
            btnCancel.Size = new Size(200, 35);
            btnCancel.BackColor = Color.LightGray;
            btnCancel.Font = new Font("Arial", 10, FontStyle.Bold);
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);
        }

        private void AddTransactions(ComboBox cmbOperation, NumericUpDown nudPrice)
        {
            string operation = cmbOperation.SelectedItem?.ToString() ?? "";
            decimal price = nudPrice.Value;

            int added = 0;
            foreach (var item in selectedItems)
            {
                // Создаем транзакцию для каждого предмета
                var transaction = new Transaction
                {
                    Game = item.Game,
                    Item = item.ItemName,
                    Operation = operation,
                    Quantity = item.Quantity,
                    Price = operation == "Покупка" ? price : 0
                };

                dataService.AddTransaction(transaction);
                added++;
            }

            this.AddedCount = added;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}