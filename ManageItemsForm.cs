using System;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryTrackerApp
{
    public class ManageItemsForm : Form
    {
        private DataService dataService;
        private ListBox listBoxItems;
        private TextBox txtNewItem;

        public ManageItemsForm(DataService dataService)
        {
            this.dataService = dataService;
            InitializeForm();
            CreateProperForm();
            LoadItems();
        }

        private void InitializeForm()
        {
            this.Text = "Управление предметами";
            this.Size = new Size(500, 450); // ← Увеличена высота
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
        }

        private void CreateProperForm()
        {
            // Заголовок
            Label titleLabel = new Label();
            titleLabel.Text = "🎯 УПРАВЛЕНИЕ ПРЕДМЕТАМИ";
            titleLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.BackColor = Color.FromArgb(230, 126, 34);
            titleLabel.Location = new Point(0, 0);
            titleLabel.Size = new Size(500, 50);
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(titleLabel);

            int yPos = 60; // Начинаем ниже заголовка

            // Список предметов
            Label lblItems = new Label();
            lblItems.Text = "Список предметов:";
            lblItems.Location = new Point(20, yPos);
            lblItems.AutoSize = true;
            lblItems.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblItems.ForeColor = Color.Black;
            this.Controls.Add(lblItems);
            yPos += 25;

            listBoxItems = new ListBox();
            listBoxItems.Location = new Point(20, yPos);
            listBoxItems.Size = new Size(460, 150);
            listBoxItems.Font = new Font("Arial", 9);
            listBoxItems.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(listBoxItems);
            yPos += 160;

            // Добавление нового предмета
            Label lblNewItem = new Label();
            lblNewItem.Text = "Добавить новый предмет:";
            lblNewItem.Location = new Point(20, yPos);
            lblNewItem.AutoSize = true;
            lblNewItem.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblNewItem.ForeColor = Color.Black;
            this.Controls.Add(lblNewItem);
            yPos += 25;

            txtNewItem = new TextBox();
            txtNewItem.Location = new Point(20, yPos);
            txtNewItem.Size = new Size(300, 25);
            txtNewItem.Font = new Font("Arial", 9);
            txtNewItem.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(txtNewItem);

            Button btnAdd = new Button();
            btnAdd.Text = "➕ Добавить";
            btnAdd.Location = new Point(330, yPos - 1);
            btnAdd.Size = new Size(150, 27);
            btnAdd.BackColor = Color.FromArgb(46, 204, 113);
            btnAdd.ForeColor = Color.White;
            btnAdd.Font = new Font("Arial", 9, FontStyle.Bold);
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;
            this.Controls.Add(btnAdd);
            yPos += 40;

            // Кнопки управления - РАЗМЕЩЕНЫ ПРАВИЛЬНО
            Button btnRemove = new Button();
            btnRemove.Text = "🗑️ Удалить выбранный";
            btnRemove.Location = new Point(20, yPos);
            btnRemove.Size = new Size(150, 35);
            btnRemove.BackColor = Color.FromArgb(231, 76, 60);
            btnRemove.ForeColor = Color.White;
            btnRemove.Font = new Font("Arial", 9, FontStyle.Bold);
            btnRemove.FlatStyle = FlatStyle.Flat;
            btnRemove.FlatAppearance.BorderSize = 0;
            btnRemove.Click += BtnRemove_Click;
            this.Controls.Add(btnRemove);

            Button btnReset = new Button();
            btnReset.Text = "🔄 Сбросить список";
            btnReset.Location = new Point(180, yPos);
            btnReset.Size = new Size(150, 35);
            btnReset.BackColor = Color.FromArgb(241, 196, 15);
            btnReset.ForeColor = Color.Black;
            btnReset.Font = new Font("Arial", 9, FontStyle.Bold);
            btnReset.FlatStyle = FlatStyle.Flat;
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.Click += BtnReset_Click;
            this.Controls.Add(btnReset);

            Button btnClose = new Button();
            btnClose.Text = "Закрыть";
            btnClose.Location = new Point(340, yPos);
            btnClose.Size = new Size(140, 35);
            btnClose.BackColor = Color.LightGray;
            btnClose.Font = new Font("Arial", 9, FontStyle.Bold);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
            yPos += 45;

            // Информационная панель
            Panel infoPanel = new Panel();
            infoPanel.Location = new Point(20, yPos);
            infoPanel.Size = new Size(460, 40);
            infoPanel.BackColor = Color.LightYellow;
            infoPanel.BorderStyle = BorderStyle.FixedSingle;

            Label infoLabel = new Label();
            infoLabel.Text = "💡 Добавляйте предметы, которые часто покупаете/продаете";
            infoLabel.Dock = DockStyle.Fill;
            infoLabel.TextAlign = ContentAlignment.MiddleCenter;
            infoLabel.Font = new Font("Arial", 8, FontStyle.Italic);
            infoLabel.ForeColor = Color.DarkBlue;
            infoPanel.Controls.Add(infoLabel);
            this.Controls.Add(infoPanel);
        }

        private void LoadItems()
        {
            listBoxItems.Items.Clear();
            var items = dataService.GetCustomItems();
            foreach (var item in items)
            {
                listBoxItems.Items.Add(item);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string newItem = txtNewItem.Text.Trim();
            if (string.IsNullOrEmpty(newItem))
            {
                MessageBox.Show("Введите название предмета!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dataService.AddCustomItem(newItem);
            LoadItems();
            txtNewItem.Clear();

            MessageBox.Show($"Предмет '{newItem}' добавлен!", "Успех",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (listBoxItems.SelectedItem == null)
            {
                MessageBox.Show("Выберите предмет для удаления!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedItem = listBoxItems.SelectedItem.ToString();
            var result = MessageBox.Show($"Удалить предмет '{selectedItem}'?",
                "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                dataService.RemoveCustomItem(selectedItem);
                LoadItems();
                MessageBox.Show("Предмет удален!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Сбросить все предметы к стандартному списку?",
                "Подтверждение сброса", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                dataService.ResetCustomItems();
                LoadItems();
                MessageBox.Show("Список предметов сброшен!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}