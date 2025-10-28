using System;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryTrackerApp
{
    public class AddOrderFillForm : Form
    {
        private DataService dataService;
        private Order order;
        private NumericUpDown nudQuantity;
        private NumericUpDown nudPrice;
        private TextBox txtNotes;
        private Label lblAutoTransaction;

        public AddOrderFillForm(DataService dataService, Order order)
        {
            this.dataService = dataService;
            this.order = order;
            InitializeForm();
            CreateControls();
        }

        private void InitializeForm()
        {
            this.Text = "📥 Добавление исполнения ордера";
            this.Size = new Size(450, 400); // Увеличили высоту для информационного сообщения
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
        }

        private void CreateControls()
        {
            // Заголовок
            Label titleLabel = new Label();
            titleLabel.Text = "📥 ДОБАВЛЕНИЕ ИСПОЛНЕНИЯ";
            titleLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.BackColor = Color.FromArgb(241, 196, 15);
            titleLabel.Dock = DockStyle.Top;
            titleLabel.Height = 50;
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(titleLabel);

            // Информация об ордере
            Panel infoPanel = new Panel();
            infoPanel.Dock = DockStyle.Top;
            infoPanel.Height = 75;
            infoPanel.BackColor = Color.FromArgb(245, 245, 245);
            infoPanel.Padding = new Padding(15);
            this.Controls.Add(infoPanel);

            Label lblOrderInfo = new Label();
            lblOrderInfo.Text = $"Ордер: {order.Game} | {order.Item}\n" +
                               $"Тип: {order.Type} | Цель: {order.TargetQuantity} шт. по {order.TargetPrice:0.00} руб.\n" +
                               $"Исполнено: {order.FilledQuantity} шт. | Осталось: {order.RemainingQuantity} шт.";
            lblOrderInfo.Dock = DockStyle.Fill;
            lblOrderInfo.Font = new Font("Arial", 9);
            lblOrderInfo.TextAlign = ContentAlignment.MiddleLeft;
            infoPanel.Controls.Add(lblOrderInfo);

            // Основная панель
            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(20);
            mainPanel.BackColor = Color.White;
            this.Controls.Add(mainPanel);

            int y = 125;

            // Информация об автоматической сделке
            lblAutoTransaction = new Label();
            lblAutoTransaction.Text = $"💡 Автоматически будет создана сделка: {order.Type} {order.Item}";
            lblAutoTransaction.Location = new Point(20, y);
            lblAutoTransaction.Size = new Size(400, 20);
            lblAutoTransaction.Font = new Font("Arial", 9, FontStyle.Bold);
            lblAutoTransaction.ForeColor = Color.DarkBlue;
            mainPanel.Controls.Add(lblAutoTransaction);

            y += 30;

            // Количество
            Label lblQuantity = new Label();
            lblQuantity.Text = "📦 Количество:";
            lblQuantity.Location = new Point(20, y);
            lblQuantity.Size = new Size(120, 20);
            lblQuantity.Font = new Font("Arial", 9, FontStyle.Bold);
            mainPanel.Controls.Add(lblQuantity);

            nudQuantity = new NumericUpDown();
            nudQuantity.Location = new Point(150, y - 3);
            nudQuantity.Size = new Size(150, 25);
            nudQuantity.Minimum = 1;
            nudQuantity.Maximum = order.RemainingQuantity;
            nudQuantity.Value = 1;
            nudQuantity.Font = new Font("Arial", 9);
            nudQuantity.ValueChanged += NudQuantity_ValueChanged;
            mainPanel.Controls.Add(nudQuantity);

            Label lblMaxQuantity = new Label();
            lblMaxQuantity.Text = $"(макс: {order.RemainingQuantity})";
            lblMaxQuantity.Location = new Point(310, y);
            lblMaxQuantity.Size = new Size(100, 20);
            lblMaxQuantity.Font = new Font("Arial", 8);
            lblMaxQuantity.ForeColor = Color.Gray;
            mainPanel.Controls.Add(lblMaxQuantity);

            y += 40;

            // Цена
            Label lblPrice = new Label();
            lblPrice.Text = "💰 Цена:";
            lblPrice.Location = new Point(20, y);
            lblPrice.Size = new Size(120, 20);
            lblPrice.Font = new Font("Arial", 9, FontStyle.Bold);
            mainPanel.Controls.Add(lblPrice);

            nudPrice = new NumericUpDown();
            nudPrice.Location = new Point(150, y - 3);
            nudPrice.Size = new Size(150, 25);
            nudPrice.DecimalPlaces = 2;
            nudPrice.Minimum = 0.01m;
            nudPrice.Maximum = 1000000;
            nudPrice.Value = order.TargetPrice;
            nudPrice.Font = new Font("Arial", 9);
            nudPrice.ValueChanged += NudPrice_ValueChanged;
            mainPanel.Controls.Add(nudPrice);

            Label lblPriceCurrency = new Label();
            lblPriceCurrency.Text = "руб.";
            lblPriceCurrency.Location = new Point(310, y);
            lblPriceCurrency.Size = new Size(40, 20);
            lblPriceCurrency.Font = new Font("Arial", 9);
            mainPanel.Controls.Add(lblPriceCurrency);

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
            txtNotes.Size = new Size(250, 60);
            txtNotes.Multiline = true;
            txtNotes.ScrollBars = ScrollBars.Vertical;
            txtNotes.Font = new Font("Arial", 9);
            mainPanel.Controls.Add(txtNotes);

            y += 80;

            // Кнопки
            Button btnSave = new Button();
            btnSave.Text = "💾 Сохранить исполнение";
            btnSave.Location = new Point(150, y);
            btnSave.Size = new Size(140, 35);
            btnSave.BackColor = Color.FromArgb(46, 204, 113);
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Font = new Font("Arial", 9, FontStyle.Bold);
            btnSave.Click += BtnSave_Click;
            mainPanel.Controls.Add(btnSave);

            Button btnCancel = new Button();
            btnCancel.Text = "❌ Отмена";
            btnCancel.Location = new Point(300, y);
            btnCancel.Size = new Size(100, 35);
            btnCancel.BackColor = Color.FromArgb(231, 76, 60);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Font = new Font("Arial", 9, FontStyle.Bold);
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            mainPanel.Controls.Add(btnCancel);

            UpdateAutoTransactionInfo();
        }

        private void NudQuantity_ValueChanged(object sender, EventArgs e)
        {
            UpdateAutoTransactionInfo();
        }

        private void NudPrice_ValueChanged(object sender, EventArgs e)
        {
            UpdateAutoTransactionInfo();
        }

        private void UpdateAutoTransactionInfo()
        {
            int quantity = (int)nudQuantity.Value;
            decimal price = nudPrice.Value;
            decimal total = quantity * price;

            lblAutoTransaction.Text = $"💡 Автоматически будет создана сделка: {order.Type} '{order.Item}' " +
                                    $"{quantity} шт. по {price:0.00} руб. (итого: {total:0.00} руб.)";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (ValidateForm())
            {
                try
                {
                    var fill = new OrderFill
                    {
                        Quantity = (int)nudQuantity.Value,
                        Price = nudPrice.Value,
                        Notes = txtNotes.Text.Trim()
                    };

                    if (dataService.AddOrderFill(order.Id, fill))
                    {
                        MessageBox.Show($"✅ Исполнение ордера добавлено!\n\n" +
                                      $"📦 Создана сделка: {order.Type} '{order.Item}'\n" +
                                      $"💰 {fill.Quantity} шт. по {fill.Price:0.00} руб.\n\n" +
                                      $"💡 Сделка автоматически добавлена в историю и инвентарь",
                                      "Успех",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Information);

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления исполнения: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool ValidateForm()
        {
            if (nudQuantity.Value <= 0)
            {
                MessageBox.Show("Количество должно быть больше 0!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                nudQuantity.Focus();
                return false;
            }

            if (nudQuantity.Value > order.RemainingQuantity)
            {
                MessageBox.Show($"Нельзя добавить больше {order.RemainingQuantity} шт.!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                nudQuantity.Focus();
                return false;
            }

            if (nudPrice.Value <= 0)
            {
                MessageBox.Show("Цена должна быть больше 0!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                nudPrice.Focus();
                return false;
            }

            return true;
        }
    }
}