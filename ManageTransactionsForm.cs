using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace InventoryTrackerApp
{
    public class ManageTransactionsForm : Form
    {
        private DataService dataService;
        private DataGridView dgvTransactions;
        private List<Transaction> allTransactions;

        public ManageTransactionsForm(DataService dataService)
        {
            this.dataService = dataService;
            InitializeForm();
            CreateControls();
            LoadTransactions();
        }

        private void InitializeForm()
        {
            this.Text = "🗑️ Управление сделками";
            this.Size = new Size(1200, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
        }

        private void CreateControls()
        {
            // Заголовок
            Label titleLabel = new Label();
            titleLabel.Text = "🗑️ УПРАВЛЕНИЕ СДЕЛКАМИ";
            titleLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.BackColor = Color.FromArgb(231, 76, 60);
            titleLabel.Location = new Point(0, 0);
            titleLabel.Size = new Size(1200, 50);
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(titleLabel);

            int yPos = 60;

            // Панель информации
            Panel infoPanel = new Panel();
            infoPanel.Location = new Point(20, yPos);
            infoPanel.Size = new Size(1160, 40);
            infoPanel.BackColor = Color.LightYellow;
            infoPanel.BorderStyle = BorderStyle.FixedSingle;

            Label infoLabel = new Label();
            infoLabel.Text = "💡 Выберите сделку для удаления. Внимание: удаление сделки пересчитает весь инвентарь!";
            infoLabel.Dock = DockStyle.Fill;
            infoLabel.TextAlign = ContentAlignment.MiddleCenter;
            infoLabel.Font = new Font("Arial", 9, FontStyle.Italic);
            infoLabel.ForeColor = Color.DarkBlue;
            infoPanel.Controls.Add(infoLabel);

            this.Controls.Add(infoPanel);
            yPos += 50;

            // Таблица сделок
            dgvTransactions = new DataGridView();
            dgvTransactions.Location = new Point(20, yPos);
            dgvTransactions.Size = new Size(1160, 400);
            dgvTransactions.BackgroundColor = Color.White;
            dgvTransactions.RowHeadersVisible = false;
            dgvTransactions.Font = new Font("Arial", 9);
            dgvTransactions.AllowUserToAddRows = false;
            dgvTransactions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTransactions.ReadOnly = true;
            this.Controls.Add(dgvTransactions);

            yPos += 410;

            // Панель кнопок
            Panel buttonPanel = new Panel();
            buttonPanel.Location = new Point(20, yPos);
            buttonPanel.Size = new Size(1160, 50);
            this.Controls.Add(buttonPanel);

            // Кнопка удаления
            Button btnDelete = new Button();
            btnDelete.Text = "🗑️ Удалить выбранную сделку";
            btnDelete.Size = new Size(200, 35);
            btnDelete.Location = new Point(10, 7);
            btnDelete.BackColor = Color.FromArgb(231, 76, 60);
            btnDelete.ForeColor = Color.White;
            btnDelete.Font = new Font("Arial", 10, FontStyle.Bold);
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += BtnDelete_Click;
            buttonPanel.Controls.Add(btnDelete);

            // Кнопка обновления
            Button btnRefresh = new Button();
            btnRefresh.Text = "🔄 Обновить";
            btnRefresh.Size = new Size(120, 35);
            btnRefresh.Location = new Point(220, 7);
            btnRefresh.BackColor = Color.LightGreen;
            btnRefresh.Font = new Font("Arial", 10, FontStyle.Bold);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadTransactions();
            buttonPanel.Controls.Add(btnRefresh);

            // Кнопка закрытия
            Button btnClose = new Button();
            btnClose.Text = "Закрыть";
            btnClose.Size = new Size(100, 35);
            btnClose.Location = new Point(350, 7);
            btnClose.BackColor = Color.LightGray;
            btnClose.Font = new Font("Arial", 10, FontStyle.Bold);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            buttonPanel.Controls.Add(btnClose);

            // Статистика
            Label statsLabel = new Label();
            statsLabel.Name = "statsLabel";
            statsLabel.Text = "Загрузка...";
            statsLabel.Location = new Point(460, 15);
            statsLabel.Size = new Size(500, 20);
            statsLabel.Font = new Font("Arial", 9, FontStyle.Bold);
            statsLabel.ForeColor = Color.DarkBlue;
            buttonPanel.Controls.Add(statsLabel);
        }

        private void LoadTransactions()
        {
            allTransactions = dataService.GetTransactions();
            UpdateDataGrid();
            UpdateStatistics();
        }

        private void UpdateDataGrid()
        {
            dgvTransactions.Columns.Clear();

            if (allTransactions.Count == 0)
            {
                dgvTransactions.Columns.Add("Empty", "Информация");
                dgvTransactions.Rows.Add("📊 Нет сделок для отображения");
                return;
            }

            // Создаем колонки
            string[] columns = {
                "ID", "Дата", "Игра", "Предмет", "Тип операции",
                "Количество", "Цена за шт.", "Общая сумма", "Действия"
            };

            foreach (string col in columns)
                dgvTransactions.Columns.Add(col, col);

            // Заполняем данными
            foreach (var transaction in allTransactions.OrderByDescending(t => t.Date))
            {
                int rowIndex = dgvTransactions.Rows.Add(
                    transaction.Id.Substring(0, 8) + "...",
                    transaction.Date.ToString("dd.MM.yyyy HH:mm"),
                    transaction.Game,
                    transaction.Item,
                    transaction.Operation,
                    transaction.Quantity.ToString() + " шт.",
                    transaction.Price.ToString("0.00") + " руб.",
                    transaction.Total.ToString("0.00") + " руб.",
                    "❌ Удалить"
                );

                // Цветовое кодирование по типу операции
                Color backColor = GetTransactionColor(transaction.Operation);
                dgvTransactions.Rows[rowIndex].DefaultCellStyle.BackColor = backColor;

                if (backColor == Color.LavenderBlush || backColor == Color.LightCoral)
                {
                    dgvTransactions.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.DarkRed;
                }
            }

            // Настраиваем ширину колонок
            dgvTransactions.Columns["ID"].Width = 80;
            dgvTransactions.Columns["Дата"].Width = 120;
            dgvTransactions.Columns["Игра"].Width = 120;
            dgvTransactions.Columns["Предмет"].Width = 200;
            dgvTransactions.Columns["Тип операции"].Width = 100;
            dgvTransactions.Columns["Количество"].Width = 80;
            dgvTransactions.Columns["Цена за шт."].Width = 100;
            dgvTransactions.Columns["Общая сумма"].Width = 100;
            dgvTransactions.Columns["Действия"].Width = 80;

            // Добавляем обработчик клика по кнопке удаления
            dgvTransactions.CellClick += DgvTransactions_CellClick;
        }

        private Color GetTransactionColor(string operation)
        {
            switch (operation)
            {
                case "Покупка":
                    return Color.LavenderBlush;
                case "Продажа":
                    return Color.Honeydew;
                case "Подарок":
                    return Color.AliceBlue;
                case "Обмен":
                    return Color.LightCyan;
                case "Крафт":
                    return Color.LightYellow;
                default:
                    return Color.White;
            }
        }

        private void UpdateStatistics()
        {
            var statsLabel = this.Controls.Find("statsLabel", true).FirstOrDefault() as Label;
            if (statsLabel != null)
            {
                int totalTransactions = allTransactions.Count;
                int purchases = allTransactions.Count(t => t.Operation == "Покупка");
                int sales = allTransactions.Count(t => t.Operation == "Продажа");
                int gifts = allTransactions.Count(t => t.Operation == "Подарок");

                statsLabel.Text = $"📊 Всего: {totalTransactions} | Покупок: {purchases} | Продаж: {sales} | Подарков: {gifts}";
            }
        }

        private void DgvTransactions_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != dgvTransactions.Columns["Действия"].Index)
                return;

            if (e.RowIndex >= allTransactions.Count)
                return;

            var transaction = allTransactions
                .OrderByDescending(t => t.Date)
                .ElementAt(e.RowIndex);

            DeleteTransaction(transaction);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvTransactions.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите сделку для удаления!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRow = dgvTransactions.SelectedRows[0];
            if (selectedRow.Cells["ID"].Value == null)
                return;

            string transactionIdShort = selectedRow.Cells["ID"].Value.ToString();
            var transaction = allTransactions.FirstOrDefault(t =>
                t.Id.StartsWith(transactionIdShort.Replace("...", "")));

            if (transaction != null)
            {
                DeleteTransaction(transaction);
            }
        }

        private void DeleteTransaction(Transaction transaction)
        {
            string details = $"\nДата: {transaction.Date:dd.MM.yyyy HH:mm}" +
                           $"\nИгра: {transaction.Game}" +
                           $"\nПредмет: {transaction.Item}" +
                           $"\nОперация: {transaction.Operation}" +
                           $"\nКоличество: {transaction.Quantity} шт." +
                           $"\nЦена: {transaction.Price:0.00} руб.";

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить эту сделку?{details}",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                bool success = dataService.RemoveTransaction(transaction.Id);

                if (success)
                {
                    MessageBox.Show("Сделка успешно удалена!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadTransactions(); // Перезагружаем список
                }
                else
                {
                    MessageBox.Show("Не удалось удалить сделку!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}