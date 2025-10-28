using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace InventoryTrackerApp
{
    public class OrderFillsForm : Form
    {
        private DataService dataService;
        private Order order;
        private DataGridView dgvFills;

        public OrderFillsForm(DataService dataService, Order order)
        {
            this.dataService = dataService;
            this.order = order;
            InitializeForm();
            CreateControls();
            LoadFills();
        }

        private void InitializeForm()
        {
            this.Text = $"📋 История исполнений - {order.Item}";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.MinimumSize = new Size(600, 400);
        }

        private void CreateControls()
        {
            // Заголовок
            Label titleLabel = new Label();
            titleLabel.Text = $"📋 ИСТОРИЯ ИСПОЛНЕНИЙ - {order.Item.ToUpper()}";
            titleLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.BackColor = Color.FromArgb(155, 89, 182);
            titleLabel.Dock = DockStyle.Top;
            titleLabel.Height = 50;
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(titleLabel);

            // Информация об ордере
            Panel infoPanel = new Panel();
            infoPanel.Dock = DockStyle.Top;
            infoPanel.Height = 70;
            infoPanel.BackColor = Color.FromArgb(245, 245, 245);
            infoPanel.Padding = new Padding(15);
            this.Controls.Add(infoPanel);

            Label lblOrderInfo = new Label();
            lblOrderInfo.Text = $"🎮 Игра: {order.Game} | 📊 Тип: {order.Type}\n" +
                               $"💰 Цель: {order.TargetQuantity} шт. по {order.TargetPrice:0.00} руб.\n" +
                               $"📈 Прогресс: {order.FilledQuantity}/{order.TargetQuantity} шт. ({order.ProgressPercent:0}%)";
            lblOrderInfo.Dock = DockStyle.Fill;
            lblOrderInfo.Font = new Font("Arial", 10);
            lblOrderInfo.TextAlign = ContentAlignment.MiddleLeft;
            infoPanel.Controls.Add(lblOrderInfo);

            // Панель управления
            Panel controlPanel = new Panel();
            controlPanel.Dock = DockStyle.Top;
            controlPanel.Height = 50;
            controlPanel.BackColor = Color.FromArgb(240, 240, 240);
            controlPanel.Padding = new Padding(10);
            this.Controls.Add(controlPanel);

            Button btnRemoveFill = new Button();
            btnRemoveFill.Text = "🗑️ Удалить исполнение";
            btnRemoveFill.Location = new Point(10, 10);
            btnRemoveFill.Size = new Size(150, 30);
            btnRemoveFill.BackColor = Color.FromArgb(231, 76, 60);
            btnRemoveFill.ForeColor = Color.White;
            btnRemoveFill.FlatStyle = FlatStyle.Flat;
            btnRemoveFill.FlatAppearance.BorderSize = 0;
            btnRemoveFill.Font = new Font("Arial", 9, FontStyle.Bold);
            btnRemoveFill.Click += BtnRemoveFill_Click;
            controlPanel.Controls.Add(btnRemoveFill);

            // Таблица исполнений
            dgvFills = new DataGridView();
            dgvFills.Dock = DockStyle.Fill;
            dgvFills.BackgroundColor = Color.White;
            dgvFills.RowHeadersVisible = false;
            dgvFills.Font = new Font("Arial", 9);
            dgvFills.AllowUserToAddRows = false;
            dgvFills.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvFills.ReadOnly = true;
            dgvFills.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.Controls.Add(dgvFills);

            // Статус бар
            StatusStrip statusStrip = new StatusStrip();
            statusStrip.Dock = DockStyle.Bottom;
            statusStrip.Items.Add(new ToolStripStatusLabel());
            this.Controls.Add(statusStrip);

            UpdateStatus();
        }

        private void LoadFills()
        {
            dgvFills.Columns.Clear();

            if (order.Fills.Count == 0)
            {
                dgvFills.Columns.Add("Empty", "Информация");
                dgvFills.Rows.Add("📊 Нет исполнений для этого ордера.");
                return;
            }

            // Создаем колонки
            dgvFills.Columns.Add("Date", "Дата");
            dgvFills.Columns.Add("Quantity", "Количество");
            dgvFills.Columns.Add("Price", "Цена");
            dgvFills.Columns.Add("Total", "Сумма");
            dgvFills.Columns.Add("Notes", "Заметки");

            // Настраиваем ширины
            dgvFills.Columns["Date"].Width = 150;
            dgvFills.Columns["Quantity"].Width = 100;
            dgvFills.Columns["Price"].Width = 100;
            dgvFills.Columns["Total"].Width = 100;
            dgvFills.Columns["Notes"].Width = 200;

            // Заполняем данными
            foreach (var fill in order.Fills.OrderByDescending(f => f.FillDate))
            {
                int idx = dgvFills.Rows.Add(
                    fill.FillDate.ToString("dd.MM.yyyy HH:mm"),
                    fill.Quantity + " шт.",
                    fill.Price.ToString("0.00") + " руб.",
                    fill.Total.ToString("0.00") + " руб.",
                    fill.Notes
                );

                // Сохраняем ID исполнения в Tag строки
                dgvFills.Rows[idx].Tag = fill.Id;
            }
        }

        private void BtnRemoveFill_Click(object sender, EventArgs e)
        {
            if (dgvFills.SelectedRows.Count > 0)
            {
                string fillId = dgvFills.SelectedRows[0].Tag as string;
                if (!string.IsNullOrEmpty(fillId))
                {
                    var fill = order.Fills.FirstOrDefault(f => f.Id == fillId);
                    if (fill != null)
                    {
                        var result = MessageBox.Show(
                            $"Удалить исполнение от {fill.FillDate:dd.MM.yyyy}?\n" +
                            $"Количество: {fill.Quantity} шт., Цена: {fill.Price:0.00} руб.",
                            "Подтверждение удаления",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                        if (result == DialogResult.Yes)
                        {
                            if (dataService.RemoveOrderFill(order.Id, fillId))
                            {
                                LoadFills();
                                UpdateStatus();
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите исполнение для удаления!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void UpdateStatus()
        {
            var statusStrip = this.Controls.OfType<StatusStrip>().FirstOrDefault();
            if (statusStrip != null)
            {
                var statusLabel = statusStrip.Items[0] as ToolStripStatusLabel;
                if (statusLabel != null)
                {
                    decimal totalValue = order.Fills.Sum(f => f.Total);
                    statusLabel.Text = $"📊 Всего исполнений: {order.Fills.Count} | Общая сумма: {totalValue:0.00} руб.";
                }
            }
        }
    }
}