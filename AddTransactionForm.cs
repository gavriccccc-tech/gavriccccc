using System;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryTrackerApp
{
    public class AddTransactionForm : Form
    {
        public Transaction NewTransaction { get; private set; }
        private DataService dataService;

        private readonly Color PrimaryColor = Color.FromArgb(41, 128, 185);
        private readonly Color AccentColor = Color.FromArgb(46, 204, 113);

        private ComboBox cmbGame;
        private ComboBox cmbItem;
        private ComboBox cmbOperation;
        private NumericUpDown nudQuantity;
        private NumericUpDown nudPrice;

        public AddTransactionForm(DataService dataService)
        {
            this.dataService = dataService;
            InitializeModernForm();
            CreateModernControls();
        }

        private void InitializeModernForm()
        {
            this.Text = "➕ Новая сделка";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void CreateModernControls()
        {
            // Заголовок
            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 50;
            headerPanel.BackColor = PrimaryColor;
            this.Controls.Add(headerPanel);

            Label titleLabel = new Label();
            titleLabel.Text = "➕ НОВАЯ СДЕЛКА";
            titleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.Dock = DockStyle.Fill;
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            headerPanel.Controls.Add(titleLabel);

            // Основной контент
            Panel contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = Color.White;
            contentPanel.Padding = new Padding(30, 20, 30, 20);
            this.Controls.Add(contentPanel);

            int yPos = 65;

            // Поля формы
            yPos = CreateFormField(contentPanel, "🎮 Игра:", yPos);
            yPos = CreateFormField(contentPanel, "📦 Предмет:", yPos);
            yPos = CreateFormField(contentPanel, "⚡ Тип операции:", yPos);
            yPos = CreateFormField(contentPanel, "🔢 Количество:", yPos);
            yPos = CreateFormField(contentPanel, "💰 Цена (руб):", yPos);

            // Информационная панель
            Panel infoPanel = new Panel();
            infoPanel.Location = new Point(30, yPos);
            infoPanel.Size = new Size(440, 80);
            infoPanel.BackColor = Color.FromArgb(240, 248, 255);
            infoPanel.BorderStyle = BorderStyle.FixedSingle;
            infoPanel.Padding = new Padding(15);

            Label infoLabel = new Label();
            infoLabel.Name = "infoLabel";
            infoLabel.Text = "💡 Покупка/Продажа: обычные сделки за деньги\n💡 Подарок/Обмен: получение предметов БЕЗ оплаты";
            infoLabel.Dock = DockStyle.Fill;
            infoLabel.Font = new Font("Segoe UI", 9);
            infoLabel.ForeColor = Color.FromArgb(52, 152, 219);
            infoPanel.Controls.Add(infoLabel);
            contentPanel.Controls.Add(infoPanel);

            yPos += 90;

            // Кнопки действий
            Panel buttonPanel = new Panel();
            buttonPanel.Location = new Point(30, yPos);
            buttonPanel.Size = new Size(440, 50);
            contentPanel.Controls.Add(buttonPanel);

            Button btnAdd = CreateModernButton("✅ Добавить сделку", AccentColor, 0);
            Button btnCancel = CreateModernButton("❌ Отмена", Color.FromArgb(149, 165, 166), 220);

            btnAdd.Click += (sender, e) => AddTransaction();
            btnCancel.Click += (sender, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            buttonPanel.Controls.Add(btnAdd);
            buttonPanel.Controls.Add(btnCancel);

            // Заполняем данные
            InitializeGameComboBox();
            InitializeItemComboBox();
            InitializeOperationComboBox();

            // Обработчики
            cmbOperation.SelectedIndexChanged += (sender, e) =>
            {
                UpdateOperationInfo(infoLabel, cmbOperation.SelectedItem?.ToString() ?? "");
                UpdatePriceFieldState(cmbOperation.SelectedItem?.ToString() ?? "");
            };

            UpdateOperationInfo(infoLabel, cmbOperation.SelectedItem?.ToString() ?? "");
        }

        private int CreateFormField(Panel parent, string labelText, int yPos)
        {
            Label label = new Label();
            label.Text = labelText;
            label.Location = new Point(0, yPos);
            label.Size = new Size(150, 25);
            label.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            label.ForeColor = Color.FromArgb(52, 73, 94);
            label.TextAlign = ContentAlignment.MiddleLeft;
            parent.Controls.Add(label);

            Control field;
            if (labelText.Contains("Количество"))
            {
                nudQuantity = new NumericUpDown();
                nudQuantity.Location = new Point(160, yPos);
                nudQuantity.Size = new Size(200, 35);
                nudQuantity.Font = new Font("Segoe UI", 10);
                nudQuantity.BorderStyle = BorderStyle.FixedSingle;
                nudQuantity.BackColor = Color.White;
                nudQuantity.Minimum = 1;
                nudQuantity.Maximum = 10000;
                nudQuantity.Value = 1;
                field = nudQuantity;
            }
            else if (labelText.Contains("Цена"))
            {
                nudPrice = new NumericUpDown();
                nudPrice.Location = new Point(160, yPos);
                nudPrice.Size = new Size(200, 35);
                nudPrice.Font = new Font("Segoe UI", 10);
                nudPrice.BorderStyle = BorderStyle.FixedSingle;
                nudPrice.BackColor = Color.White;
                nudPrice.DecimalPlaces = 2;
                nudPrice.Minimum = 0;
                nudPrice.Maximum = 100000000;
                nudPrice.Value = 0;
                field = nudPrice;
            }
            else if (labelText.Contains("Игра"))
            {
                cmbGame = new ComboBox();
                cmbGame.Location = new Point(160, yPos);
                cmbGame.Size = new Size(200, 35);
                cmbGame.Font = new Font("Segoe UI", 10);
                cmbGame.DropDownStyle = ComboBoxStyle.DropDownList;
                cmbGame.BackColor = Color.White;
                cmbGame.FlatStyle = FlatStyle.Flat;
                field = cmbGame;
            }
            else if (labelText.Contains("Предмет"))
            {
                cmbItem = new ComboBox();
                cmbItem.Location = new Point(160, yPos);
                cmbItem.Size = new Size(200, 35);
                cmbItem.Font = new Font("Segoe UI", 10);
                cmbItem.DropDownStyle = ComboBoxStyle.DropDown;
                cmbItem.BackColor = Color.White;
                cmbItem.FlatStyle = FlatStyle.Flat;
                field = cmbItem;
            }
            else // Тип операции
            {
                cmbOperation = new ComboBox();
                cmbOperation.Location = new Point(160, yPos);
                cmbOperation.Size = new Size(200, 35);
                cmbOperation.Font = new Font("Segoe UI", 10);
                cmbOperation.DropDownStyle = ComboBoxStyle.DropDownList;
                cmbOperation.BackColor = Color.White;
                cmbOperation.FlatStyle = FlatStyle.Flat;
                field = cmbOperation;
            }

            parent.Controls.Add(field);
            return yPos + 45;
        }

        private Button CreateModernButton(string text, Color color, int x)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, 0);
            btn.Size = new Size(200, 45);
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;

            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(color, 0.1f);
            btn.MouseLeave += (s, e) => btn.BackColor = color;

            return btn;
        }

        private void InitializeGameComboBox()
        {
            cmbGame.Items.AddRange(new string[] {
                "Counter-Strike 2", "Dota 2", "PUBG: BATTLEGROUNDS", "Rust",
                "Team Fortress 2", "Apex Legends", "Call of Duty", "Escape from Tarkov"
            });
            cmbGame.SelectedIndex = 0;
        }

        private void InitializeItemComboBox()
        {
            var customItems = dataService.GetCustomItems();
            cmbItem.Items.AddRange(customItems.ToArray());
            if (cmbItem.Items.Count > 0)
                cmbItem.SelectedIndex = 0;
        }

        private void InitializeOperationComboBox()
        {
            cmbOperation.Items.AddRange(new string[] { "Покупка", "Продажа", "Подарок", "Обмен", "Крафт" });
            cmbOperation.SelectedIndex = 0;
        }

        private void UpdateOperationInfo(Label infoLabel, string operation)
        {
            string info = "";
            switch (operation)
            {
                case "Покупка":
                    info = "💵 ПОКУПКА: Вы покупаете предметы за деньги\n💰 Цена: стоимость покупки за штуку";
                    break;
                case "Продажа":
                    info = "💰 ПРОДАЖА: Вы продаете предметы за деньги\n💵 Цена: стоимость продажи за штуку\n⚠️ Проверка: достаточно ли предметов в инвентаре";
                    break;
                case "Подарок":
                    info = "🎁 ПОДАРОК: Вы ПОЛУЧАЕТЕ предметы БЕСПЛАТНО\n💰 Цена: автоматически 0 (бесплатно)";
                    break;
                case "Обмен":
                    info = "🔄 ОБМЕН: Вы ПОЛУЧАЕТЕ предметы по обмену\n💰 Цена: автоматически 0 (без денег)";
                    break;
                case "Крафт":
                    info = "⚒️ КРАФТ: Вы создаете предмет в игре\n💰 Цена: автоматически 0";
                    break;
                default:
                    info = "💡 Выберите тип операции";
                    break;
            }
            infoLabel.Text = info;
        }

        private void UpdatePriceFieldState(string operation)
        {
            if (operation == "Подарок" || operation == "Обмен" || operation == "Крафт")
            {
                nudPrice.Value = 0;
                nudPrice.Enabled = false;
                nudPrice.BackColor = Color.FromArgb(240, 240, 240);
            }
            else
            {
                nudPrice.Enabled = true;
                nudPrice.BackColor = Color.White;
            }
        }

        private void AddTransaction()
        {
            string game = cmbGame.SelectedItem?.ToString() ?? "";
            string item = cmbItem.Text.Trim();
            string operation = cmbOperation.SelectedItem?.ToString() ?? "";
            int quantity = (int)nudQuantity.Value;
            decimal price = nudPrice.Value;

            if (string.IsNullOrEmpty(game))
            {
                MessageBox.Show("Выберите игру!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(item))
            {
                MessageBox.Show("Введите название предмета!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (operation == "Продажа")
            {
                if (!dataService.CanSellItem(game, item, quantity))
                {
                    int availableQuantity = dataService.GetItemQuantity(game, item);
                    MessageBox.Show($"Недостаточно предметов для продажи!\n\nВ инвентаре: {availableQuantity} шт.\nПытаетесь продать: {quantity} шт.",
                                   "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else if (operation == "Подарок" || operation == "Обмен" || operation == "Крафт")
            {
                price = 0;
                quantity = Math.Abs(quantity);
            }

            dataService.AddCustomItem(item);

            NewTransaction = new Transaction
            {
                Game = game,
                Item = item,
                Operation = operation,
                Quantity = operation == "Продажа" ? -quantity : quantity,
                Price = price
            };

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}