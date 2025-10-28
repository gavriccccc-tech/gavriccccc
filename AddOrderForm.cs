using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace InventoryTrackerApp
{
    public class AddOrderForm : Form
    {
        private DataService dataService;
        private Order existingOrder;
        private ComboBox cmbGame;
        private ComboBox cmbItem;
        private ComboBox cmbType;
        private NumericUpDown nudTargetPrice;
        private NumericUpDown nudTargetQuantity;
        private TextBox txtNotes;

        public AddOrderForm(DataService dataService, Order order = null)
        {
            this.dataService = dataService;
            this.existingOrder = order;
            InitializeForm();
            CreateControls();
            LoadData();
        }

        private void InitializeForm()
        {
            this.Text = existingOrder == null ? "➕ Новый ордер" : "✏️ Редактирование ордера";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
        }

        private void CreateControls()
        {
            // Заголовок
            Label titleLabel = new Label();
            titleLabel.Text = existingOrder == null ? "➕ НОВЫЙ ОРДЕР" : "✏️ РЕДАКТИРОВАНИЕ ОРДЕРА";
            titleLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.BackColor = existingOrder == null ? Color.FromArgb(46, 204, 113) : Color.FromArgb(52, 152, 219);
            titleLabel.Dock = DockStyle.Top;
            titleLabel.Height = 20;
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(titleLabel);

            // Основная панель
            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(20);
            mainPanel.BackColor = Color.White;
            this.Controls.Add(mainPanel);

            int y = 20;

            // Игра
            Label lblGame = new Label();
            lblGame.Text = "🎮 Игра:";
            lblGame.Location = new Point(20, y);
            lblGame.Size = new Size(120, 20);
            lblGame.Font = new Font("Arial", 9, FontStyle.Bold);
            mainPanel.Controls.Add(lblGame);

            cmbGame = new ComboBox();
            cmbGame.Location = new Point(150, y - 3);
            cmbGame.Size = new Size(300, 25);
            cmbGame.DropDownStyle = ComboBoxStyle.DropDown;
            cmbGame.Font = new Font("Arial", 9);
            mainPanel.Controls.Add(cmbGame);

            y += 40;

            // Предмет
            Label lblItem = new Label();
            lblItem.Text = "📦 Предмет:";
            lblItem.Location = new Point(20, y);
            lblItem.Size = new Size(120, 20);
            lblItem.Font = new Font("Arial", 9, FontStyle.Bold);
            mainPanel.Controls.Add(lblItem);

            cmbItem = new ComboBox();
            cmbItem.Location = new Point(150, y - 3);
            cmbItem.Size = new Size(300, 25);
            cmbItem.DropDownStyle = ComboBoxStyle.DropDown;
            cmbItem.Font = new Font("Arial", 9);
            mainPanel.Controls.Add(cmbItem);

            y += 40;

            // Тип ордера
            Label lblType = new Label();
            lblType.Text = "📊 Тип:";
            lblType.Location = new Point(20, y);
            lblType.Size = new Size(120, 20);
            lblType.Font = new Font("Arial", 9, FontStyle.Bold);
            mainPanel.Controls.Add(lblType);

            cmbType = new ComboBox();
            cmbType.Location = new Point(150, y - 3);
            cmbType.Size = new Size(150, 25);
            cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbType.Items.AddRange(new string[] { "Покупка", "Продажа" });
            cmbType.SelectedIndex = 0;
            cmbType.Font = new Font("Arial", 9);
            mainPanel.Controls.Add(cmbType);

            y += 40;

            // Целевая цена
            Label lblTargetPrice = new Label();
            lblTargetPrice.Text = "💰 Целевая цена:";
            lblTargetPrice.Location = new Point(20, y);
            lblTargetPrice.Size = new Size(120, 20);
            lblTargetPrice.Font = new Font("Arial", 9, FontStyle.Bold);
            mainPanel.Controls.Add(lblTargetPrice);

            nudTargetPrice = new NumericUpDown();
            nudTargetPrice.Location = new Point(150, y - 3);
            nudTargetPrice.Size = new Size(150, 25);
            nudTargetPrice.DecimalPlaces = 2;
            nudTargetPrice.Minimum = 0.01m;
            nudTargetPrice.Maximum = 1000000;
            nudTargetPrice.Font = new Font("Arial", 9);
            mainPanel.Controls.Add(nudTargetPrice);

            Label lblPriceCurrency = new Label();
            lblPriceCurrency.Text = "руб.";
            lblPriceCurrency.Location = new Point(310, y);
            lblPriceCurrency.Size = new Size(40, 20);
            lblPriceCurrency.Font = new Font("Arial", 9);
            mainPanel.Controls.Add(lblPriceCurrency);

            y += 40;

            // Целевое количество
            Label lblTargetQuantity = new Label();
            lblTargetQuantity.Text = "📦 Целевое количество:";
            lblTargetQuantity.Location = new Point(20, y);
            lblTargetQuantity.Size = new Size(120, 20);
            lblTargetQuantity.Font = new Font("Arial", 9, FontStyle.Bold);
            mainPanel.Controls.Add(lblTargetQuantity);

            nudTargetQuantity = new NumericUpDown();
            nudTargetQuantity.Location = new Point(150, y - 3);
            nudTargetQuantity.Size = new Size(150, 25);
            nudTargetQuantity.Minimum = 1;
            nudTargetQuantity.Maximum = 10000;
            nudTargetQuantity.Font = new Font("Arial", 9);
            mainPanel.Controls.Add(nudTargetQuantity);

            y += 40;

            // Заметки
            Label lblNotes = new Label();
            lblNotes.Text = "📝 Заметки:";
            lblNotes.Location = new Point(20, y);
            lblNotes.Size = new Size(120, 20);
            lblNotes.Font = new Font("Arial", 9, FontStyle.Bold);
            mainPanel.Controls.Add(lblNotes);

            txtNotes = new TextBox();
            txtNotes.Location = new Point(150, y - 3);
            txtNotes.Size = new Size(300, 60);
            txtNotes.Multiline = true;
            txtNotes.ScrollBars = ScrollBars.Vertical;
            txtNotes.Font = new Font("Arial", 9);
            mainPanel.Controls.Add(txtNotes);

            y += 80;

            // Кнопки
            Button btnSave = new Button();
            btnSave.Text = "💾 Сохранить";
            btnSave.Location = new Point(150, y);
            btnSave.Size = new Size(120, 35);
            btnSave.BackColor = Color.FromArgb(46, 204, 113);
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Font = new Font("Arial", 9, FontStyle.Bold);
            btnSave.Click += BtnSave_Click;
            mainPanel.Controls.Add(btnSave);

            Button btnCancel = new Button();
            btnCancel.Text = "❌ Отмена";
            btnCancel.Location = new Point(280, y);
            btnCancel.Size = new Size(120, 35);
            btnCancel.BackColor = Color.FromArgb(231, 76, 60);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Font = new Font("Arial", 9, FontStyle.Bold);
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            mainPanel.Controls.Add(btnCancel);
        }

        private void LoadData()
        {
            // Загружаем уникальные игры и предметы
            var transactions = dataService.GetTransactions();
            var games = transactions.Select(t => t.Game).Distinct().OrderBy(g => g).ToList();
            var items = transactions.Select(t => t.Item).Distinct().OrderBy(i => i).ToList();

            cmbGame.Items.Clear();
            cmbGame.Items.AddRange(games.ToArray());

            cmbItem.Items.Clear();
            cmbItem.Items.AddRange(items.ToArray());

            // Загружаем кастомные предметы
            var customItems = dataService.GetCustomItems();
            foreach (var item in customItems)
            {
                if (!cmbItem.Items.Contains(item))
                    cmbItem.Items.Add(item);
            }

            // Если редактируем существующий ордер
            if (existingOrder != null)
            {
                cmbGame.Text = existingOrder.Game;
                cmbItem.Text = existingOrder.Item;
                cmbType.Text = existingOrder.Type;
                nudTargetPrice.Value = existingOrder.TargetPrice;
                nudTargetQuantity.Value = existingOrder.TargetQuantity;
                txtNotes.Text = existingOrder.Notes;

                // Блокируем некоторые поля при редактировании
                cmbGame.Enabled = false;
                cmbItem.Enabled = false;
                cmbType.Enabled = false;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (ValidateForm())
            {
                try
                {
                    var order = existingOrder ?? new Order();
                    order.Game = cmbGame.Text.Trim();
                    order.Item = cmbItem.Text.Trim();
                    order.Type = cmbType.Text;
                    order.TargetPrice = nudTargetPrice.Value;
                    order.TargetQuantity = (int)nudTargetQuantity.Value;
                    order.Notes = txtNotes.Text.Trim();

                    if (existingOrder == null)
                    {
                        dataService.AddOrder(order);
                    }
                    else
                    {
                        dataService.UpdateOrder(order);
                    }

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения ордера: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(cmbGame.Text))
            {
                MessageBox.Show("Выберите игру!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbGame.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(cmbItem.Text))
            {
                MessageBox.Show("Введите название предмета!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbItem.Focus();
                return false;
            }

            if (nudTargetPrice.Value <= 0)
            {
                MessageBox.Show("Цена должна быть больше 0!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                nudTargetPrice.Focus();
                return false;
            }

            if (nudTargetQuantity.Value <= 0)
            {
                MessageBox.Show("Количество должно быть больше 0!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                nudTargetQuantity.Focus();
                return false;
            }

            return true;
        }
    }
}