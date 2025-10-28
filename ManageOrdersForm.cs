using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace InventoryTrackerApp
{
    public class ManageOrdersForm : Form
    {
        private DataService dataService;
        private DataGridView dgvOrders;
        private List<Order> allOrders;
        private Button btnAddOrder;
        private Button btnEditOrder;
        private Button btnAddFill;
        private Button btnViewFills;
        private Button btnCompleteOrder;
        private Button btnCancelOrder;
        private Button btnRemoveOrder;

        public ManageOrdersForm(DataService dataService)
        {
            this.dataService = dataService;
            this.allOrders = new List<Order>();
            InitializeForm();
            CreateControls();
            LoadOrders();
        }

        private void InitializeForm()
        {
            this.Text = "📊 Управление ордерами";
            this.Size = new Size(1400, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.MinimumSize = new Size(1200, 500);
        }

        private void CreateControls()
        {
            // Сначала создаем таблицу - ОНА ДОЛЖНА БЫТЬ ПЕРВОЙ
            dgvOrders = new DataGridView();
            dgvOrders.Dock = DockStyle.Fill;
            dgvOrders.BackgroundColor = Color.White;
            dgvOrders.RowHeadersVisible = false;
            dgvOrders.Font = new Font("Arial", 9);
            dgvOrders.AllowUserToAddRows = false;
            dgvOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOrders.ReadOnly = true;
            dgvOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvOrders.RowTemplate.Height = 30;
            dgvOrders.CellFormatting += DgvOrders_CellFormatting;

            // Добавляем таблицу ПЕРВОЙ
            this.Controls.Add(dgvOrders);

            // Заголовок - ДОБАВЛЯЕМ ПОСЛЕ ТАБЛИЦЫ
            Label titleLabel = new Label();
            titleLabel.Text = "📊 УПРАВЛЕНИЕ ОРДЕРАМИ";
            titleLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.BackColor = Color.FromArgb(155, 89, 182);
            titleLabel.Dock = DockStyle.Top;
            titleLabel.Height = 50;
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(titleLabel);

            // Панель управления - ДОБАВЛЯЕМ ПОСЛЕ ТАБЛИЦЫ
            Panel controlPanel = new Panel();
            controlPanel.Dock = DockStyle.Top;
            controlPanel.Height = 40;
            controlPanel.BackColor = Color.FromArgb(240, 240, 240);
            controlPanel.Padding = new Padding(10);
            this.Controls.Add(controlPanel);

            // Кнопки управления
            btnAddOrder = CreateControlButton("➕ Новый ордер", Color.FromArgb(46, 204, 113), 10);
            btnAddOrder.Click += (s, e) => ShowAddOrderForm();

            btnEditOrder = CreateControlButton("✏️ Редактировать", Color.FromArgb(52, 152, 219), 205);
            btnEditOrder.Click += (s, e) => EditSelectedOrder();

            btnAddFill = CreateControlButton("📥 Добавить исполнение", Color.FromArgb(241, 196, 15), 400);
            btnAddFill.Click += (s, e) => AddFillToSelectedOrder();

            btnViewFills = CreateControlButton("📋 История исполнений", Color.FromArgb(52, 152, 219), 595);
            btnViewFills.Click += (s, e) => ViewOrderFills();

            btnCompleteOrder = CreateControlButton("✅ Завершить ордер", Color.FromArgb(46, 204, 113), 790);
            btnCompleteOrder.Click += (s, e) => CompleteSelectedOrder();

            btnCancelOrder = CreateControlButton("❌ Отменить ордер", Color.FromArgb(231, 76, 60), 985);
            btnCancelOrder.Click += (s, e) => CancelSelectedOrder();

            btnRemoveOrder = CreateControlButton("🗑️ Удалить ордер", Color.FromArgb(192, 57, 43), 1180);
            btnRemoveOrder.Click += (s, e) => RemoveSelectedOrder();

            controlPanel.Controls.AddRange(new Control[] {
                btnAddOrder, btnEditOrder, btnAddFill, btnViewFills,
                btnCompleteOrder, btnCancelOrder, btnRemoveOrder
            });

            // Панель фильтров - ДОБАВЛЯЕМ ПОСЛЕ ТАБЛИЦЫ
            Panel filterPanel = new Panel();
            filterPanel.Dock = DockStyle.Top;
            filterPanel.Height = 40;
            filterPanel.BackColor = Color.FromArgb(250, 250, 250);
            filterPanel.Padding = new Padding(10, 5, 110, 5);
            this.Controls.Add(filterPanel);

            // Фильтр по статусу
            Label lblFilter = new Label();
            lblFilter.Text = "Фильтр:";
            lblFilter.Location = new Point(10, 10);
            lblFilter.AutoSize = true;
            lblFilter.Font = new Font("Arial", 9, FontStyle.Bold);
            filterPanel.Controls.Add(lblFilter);

            ComboBox cmbFilter = new ComboBox();
            cmbFilter.Location = new Point(60, 7);
            cmbFilter.Size = new Size(150, 21);
            cmbFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFilter.Items.AddRange(new string[] { "Все ордера", "Только активные", "Только выполненные", "Только отмененные" });
            cmbFilter.SelectedIndex = 0;
            cmbFilter.SelectedIndexChanged += (s, e) => ApplyFilter(cmbFilter.SelectedIndex);
            filterPanel.Controls.Add(cmbFilter);

            // Поиск
            Label lblSearch = new Label();
            lblSearch.Text = "Поиск:";
            lblSearch.Location = new Point(220, 10);
            lblSearch.AutoSize = true;
            lblSearch.Font = new Font("Arial", 9, FontStyle.Bold);
            filterPanel.Controls.Add(lblSearch);

            TextBox txtSearch = new TextBox();
            txtSearch.Location = new Point(270, 7);
            txtSearch.Size = new Size(200, 20);
            txtSearch.TextChanged += (s, e) => ApplySearch(txtSearch.Text);
            filterPanel.Controls.Add(txtSearch);

            // Статус бар - ДОБАВЛЯЕМ ПОСЛЕДНИМ
            StatusStrip statusStrip = new StatusStrip();
            statusStrip.Dock = DockStyle.Bottom;
            statusStrip.Items.Add(new ToolStripStatusLabel());
            this.Controls.Add(statusStrip);

            // ВАЖНО: Устанавливаем правильный порядок Z-Order
            this.Controls.SetChildIndex(dgvOrders, 0);       // Таблица - самый нижний слой
            this.Controls.SetChildIndex(filterPanel, 1);     // Фильтры
            this.Controls.SetChildIndex(controlPanel, 2);    // Панель управления
            this.Controls.SetChildIndex(titleLabel, 3);      // Заголовок
            this.Controls.SetChildIndex(statusStrip, 4);     // Статус бар

            UpdateStatus();
        }

        private Button CreateControlButton(string text, Color color, int x)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, 10);
            btn.Size = new Size(190, 35);
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Arial", 8, FontStyle.Bold);
            return btn;
        }

        private void LoadOrders()
        {
            try
            {
                var orders = dataService.GetOrders();
                allOrders.Clear();
                if (orders != null)
                {
                    allOrders.AddRange(orders);
                }
                UpdateDataGrid();
                UpdateStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ордеров: {ex.Message}", "Ошибка");
            }
        }

        private void UpdateDataGrid()
        {
            dgvOrders.Columns.Clear();

            if (allOrders == null || allOrders.Count == 0)
            {
                dgvOrders.Columns.Add("Empty", "Информация");
                dgvOrders.Rows.Add("📊 Нет ордеров. Создайте первый ордер через кнопку 'Новый ордер'.");
                dgvOrders.Rows[0].Height = 50;
                return;
            }

            // Создаем колонки
            dgvOrders.Columns.Add("StatusIcon", "Статус");
            dgvOrders.Columns.Add("Type", "Тип");
            dgvOrders.Columns.Add("Game", "Игра");
            dgvOrders.Columns.Add("Item", "Предмет");
            dgvOrders.Columns.Add("TargetPrice", "Целевая цена");
            dgvOrders.Columns.Add("Progress", "Прогресс");
            dgvOrders.Columns.Add("Filled", "Исполнено");
            dgvOrders.Columns.Add("Remaining", "Осталось");
            dgvOrders.Columns.Add("Created", "Создан");
            dgvOrders.Columns.Add("Completed", "Завершен");
            dgvOrders.Columns.Add("Notes", "Заметки");

            // Настраиваем ширины
            dgvOrders.Columns["StatusIcon"].Width = 50;
            dgvOrders.Columns["Type"].Width = 90;
            dgvOrders.Columns["Game"].Width = 130;
            dgvOrders.Columns["Item"].Width = 220;
            dgvOrders.Columns["TargetPrice"].Width = 100;
            dgvOrders.Columns["Progress"].Width = 120;
            dgvOrders.Columns["Filled"].Width = 90;
            dgvOrders.Columns["Remaining"].Width = 90;
            dgvOrders.Columns["Created"].Width = 130;
            dgvOrders.Columns["Completed"].Width = 130;
            dgvOrders.Columns["Notes"].Width = 220;

            // Заполняем данными
            foreach (var order in allOrders.OrderByDescending(o => o.CreatedDate))
            {
                string statusIcon = order.Status == "Активный" ? "🟢" :
                                  order.Status == "Выполнен" ? "✅" : "🔴";

                string progress = $"{order.ProgressPercent:0}% ({order.FilledQuantity}/{order.TargetQuantity})";

                int idx = dgvOrders.Rows.Add(
                    statusIcon,
                    order.Type,
                    order.Game,
                    order.Item,
                    order.TargetPrice.ToString("0.00") + " руб.",
                    progress,
                    order.FilledQuantity + " шт.",
                    order.RemainingQuantity + " шт.",
                    order.CreatedDate.ToString("dd.MM.yy HH:mm"),
                    order.CompletedDate?.ToString("dd.MM.yy HH:mm") ?? "-",
                    order.Notes
                );

                // Сохраняем ID ордера в Tag строки
                dgvOrders.Rows[idx].Tag = order.Id;
            }
        }

        private void DgvOrders_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvOrders.Rows.Count) return;

            var row = dgvOrders.Rows[e.RowIndex];
            string orderId = row.Tag as string;
            var order = allOrders?.FirstOrDefault(o => o.Id == orderId);

            if (order != null)
            {
                // Цвет строки в зависимости от статуса
                if (order.Status == "Активный")
                {
                    row.DefaultCellStyle.BackColor = Color.LightCyan;
                }
                else if (order.Status == "Выполнен")
                {
                    row.DefaultCellStyle.BackColor = Color.LightGreen;
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.LightGray;
                }

                // Прогресс бар для колонки прогресса
                if (e.ColumnIndex == dgvOrders.Columns["Progress"].Index)
                {
                    e.Value = CreateProgressBarText(order.ProgressPercent);
                }
            }
        }

        private string CreateProgressBarText(decimal percent)
        {
            int bars = (int)(percent / 5);
            string progressBar = new string('█', bars) + new string('░', 20 - bars);
            return $"{progressBar} {percent:0}%";
        }

        private void ApplyFilter(int filterIndex)
        {
            if (allOrders == null) return;

            List<Order> filteredOrders;
            switch (filterIndex)
            {
                case 0:
                    filteredOrders = allOrders;
                    break;
                case 1:
                    filteredOrders = allOrders.Where(o => o.Status == "Активный").ToList();
                    break;
                case 2:
                    filteredOrders = allOrders.Where(o => o.Status == "Выполнен").ToList();
                    break;
                case 3:
                    filteredOrders = allOrders.Where(o => o.Status == "Отменен").ToList();
                    break;
                default:
                    filteredOrders = allOrders;
                    break;
            }

            UpdateDataGridWithFilter(filteredOrders);
        }

        private void ApplySearch(string searchText)
        {
            if (allOrders == null) return;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                UpdateDataGrid();
                return;
            }

            var filteredOrders = allOrders.Where(o =>
                o.Item.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                o.Game.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                (o.Notes != null && o.Notes.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
            ).ToList();

            UpdateDataGridWithFilter(filteredOrders);
        }

        private void UpdateDataGridWithFilter(List<Order> filteredOrders)
        {
            dgvOrders.Rows.Clear();

            if (filteredOrders == null || filteredOrders.Count == 0)
            {
                dgvOrders.Columns.Clear();
                dgvOrders.Columns.Add("Empty", "Информация");
                dgvOrders.Rows.Add("📊 Ордеры не найдены по заданным критериям.");
                dgvOrders.Rows[0].Height = 50;
                return;
            }

            foreach (var order in filteredOrders.OrderByDescending(o => o.CreatedDate))
            {
                string statusIcon = order.Status == "Активный" ? "🟢" :
                                  order.Status == "Выполнен" ? "✅" : "🔴";

                string progress = $"{order.ProgressPercent:0}% ({order.FilledQuantity}/{order.TargetQuantity})";

                int idx = dgvOrders.Rows.Add(
                    statusIcon,
                    order.Type,
                    order.Game,
                    order.Item,
                    order.TargetPrice.ToString("0.00") + " руб.",
                    progress,
                    order.FilledQuantity + " шт.",
                    order.RemainingQuantity + " шт.",
                    order.CreatedDate.ToString("dd.MM.yy HH:mm"),
                    order.CompletedDate?.ToString("dd.MM.yy HH:mm") ?? "-",
                    order.Notes
                );

                dgvOrders.Rows[idx].Tag = order.Id;
            }
        }

        private string GetSelectedOrderId()
        {
            if (dgvOrders.SelectedRows.Count > 0)
            {
                return dgvOrders.SelectedRows[0].Tag as string;
            }
            return null;
        }

        private Order GetSelectedOrder()
        {
            string orderId = GetSelectedOrderId();
            return orderId != null ? dataService.GetOrder(orderId) : null;
        }

        private void ShowAddOrderForm()
        {
            using (var form = new AddOrderForm(dataService))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadOrders();
                }
            }
        }

        private void EditSelectedOrder()
        {
            var order = GetSelectedOrder();
            if (order != null)
            {
                using (var form = new AddOrderForm(dataService, order))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadOrders();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите ордер для редактирования!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void AddFillToSelectedOrder()
        {
            var order = GetSelectedOrder();
            if (order != null && order.IsActive)
            {
                using (var form = new AddOrderFillForm(dataService, order))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadOrders();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите активный ордер для добавления исполнения!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ViewOrderFills()
        {
            var order = GetSelectedOrder();
            if (order != null)
            {
                using (var form = new OrderFillsForm(dataService, order))
                {
                    form.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("Выберите ордер для просмотра исполнений!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void CompleteSelectedOrder()
        {
            var order = GetSelectedOrder();
            if (order != null && order.IsActive)
            {
                var result = MessageBox.Show(
                    $"Завершить ордер {order.Item}? Это отметит ордер как выполненный.",
                    "Подтверждение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    order.Status = "Выполнен";
                    order.CompletedDate = DateTime.Now;
                    dataService.UpdateOrder(order);
                    LoadOrders();
                }
            }
            else
            {
                MessageBox.Show("Выберите активный ордер для завершения!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void CancelSelectedOrder()
        {
            var order = GetSelectedOrder();
            if (order != null && order.IsActive)
            {
                var result = MessageBox.Show(
                    $"Отменить ордер {order.Item}?",
                    "Подтверждение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    order.Status = "Отменен";
                    order.CompletedDate = DateTime.Now;
                    dataService.UpdateOrder(order);
                    LoadOrders();
                }
            }
            else
            {
                MessageBox.Show("Выберите активный ордер для отмены!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void RemoveSelectedOrder()
        {
            var order = GetSelectedOrder();
            if (order != null)
            {
                var result = MessageBox.Show(
                    $"Удалить ордер {order.Item}? Это действие нельзя отменить.",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    dataService.RemoveOrder(order.Id);
                    LoadOrders();
                }
            }
            else
            {
                MessageBox.Show("Выберите ордер для удаления!", "Информация",
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
                    if (allOrders == null)
                    {
                        statusLabel.Text = "📊 Ошибка загрузки ордеров";
                    }
                    else
                    {
                        int total = allOrders.Count;
                        int active = allOrders.Count(o => o.IsActive);
                        int completed = allOrders.Count(o => o.Status == "Выполнен");
                        int cancelled = allOrders.Count(o => o.Status == "Отменен");

                        statusLabel.Text = $"📊 Всего: {total} | Активные: {active} | Выполненные: {completed} | Отмененные: {cancelled}";
                    }
                }
            }
        }
    }
}