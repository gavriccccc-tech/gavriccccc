using System;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryTrackerApp
{
    public class PriceRefreshForm : Form
    {
        private ProgressBar progressBar;
        private Label statusLabel;
        private Label currentItemLabel;
        private int totalItems;
        private Button btnCancel;

        public PriceRefreshForm(int totalItems)
        {
            this.totalItems = totalItems;
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = "🔄 Обновление цен из Steam";
            this.Size = new Size(500, 180);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            // Заголовок
            Label titleLabel = new Label();
            titleLabel.Text = "🔄 ПОЛУЧЕНИЕ АКТУАЛЬНЫХ ЦЕН ИЗ STEAM";
            titleLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.BackColor = Color.FromArgb(52, 152, 219);
            titleLabel.Location = new Point(0, 0);
            titleLabel.Size = new Size(500, 30);
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(titleLabel);

            // Текущий предмет
            currentItemLabel = new Label();
            currentItemLabel.Text = "Подготовка...";
            currentItemLabel.Location = new Point(20, 40);
            currentItemLabel.Size = new Size(460, 20);
            currentItemLabel.Font = new Font("Arial", 9, FontStyle.Bold);
            currentItemLabel.ForeColor = Color.DarkBlue;
            this.Controls.Add(currentItemLabel);

            // Статус
            statusLabel = new Label();
            statusLabel.Text = $"Обработано: 0 из {totalItems}";
            statusLabel.Location = new Point(20, 65);
            statusLabel.Size = new Size(460, 15);
            statusLabel.Font = new Font("Arial", 8);
            statusLabel.ForeColor = Color.Gray;
            this.Controls.Add(statusLabel);

            // Прогресс бар
            progressBar = new ProgressBar();
            progressBar.Location = new Point(20, 85);
            progressBar.Size = new Size(460, 25);
            progressBar.Minimum = 0;
            progressBar.Maximum = totalItems;
            progressBar.Style = ProgressBarStyle.Continuous;
            this.Controls.Add(progressBar);

            // Кнопка отмены
            btnCancel = new Button();
            btnCancel.Text = "❌ ОТМЕНА";
            btnCancel.Location = new Point(210, 115);
            btnCancel.Size = new Size(80, 30);
            btnCancel.BackColor = Color.FromArgb(231, 76, 60);
            btnCancel.ForeColor = Color.White;
            btnCancel.Font = new Font("Arial", 8, FontStyle.Bold);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
            this.Controls.Add(btnCancel);
        }

        public void UpdateProgress(string currentItem, int processedCount)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;

            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<string, int>(UpdateProgress), currentItem, processedCount);
                    return;
                }

                currentItemLabel.Text = $"🔍 Запрос цены: {TruncateText(currentItem, 50)}";
                statusLabel.Text = $"Обработано: {processedCount} из {totalItems} предметов";
                progressBar.Value = Math.Min(processedCount, totalItems);

                // Обновляем заголовок окна
                this.Text = $"🔄 Обновление цен ({processedCount}/{totalItems})";
            }
            catch (ObjectDisposedException)
            {
                // Форма уже закрыта, игнорируем
            }
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength - 3) + "...";
        }
    }
}