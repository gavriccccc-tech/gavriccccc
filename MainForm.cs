using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace InventoryTrackerApp
{
    public partial class MainForm : Form
    {
        private DataService dataService;
        private Panel sidebarPanel;
        private Panel contentPanel;
        private Button currentActiveButton;

        // Цветовая схема
        private readonly Color PrimaryColor = Color.FromArgb(41, 128, 185);
        private readonly Color SecondaryColor = Color.FromArgb(52, 152, 219);
        private readonly Color AccentColor = Color.FromArgb(46, 204, 113);
        private readonly Color DarkColor = Color.FromArgb(44, 62, 80);
        private readonly Color LightColor = Color.FromArgb(236, 240, 241);

        public MainForm()
        {
            InitializeComponent();
            InitializeServices();
            CreateModernInterface();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.Name = "MainForm";
            this.ResumeLayout(false);
        }

        private void InitializeServices()
        {
            dataService = new DataService();
        }

        private void CreateModernInterface()
        {
            this.Text = "🎮 Inventory Tracker - CS2 & DOTA 2";
            this.Size = new Size(1420, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = LightColor;
            this.Font = new Font("Segoe UI", 9);

            CreateSidebar();
            CreateContentArea();
        }

        private void CreateSidebar()
        {
            sidebarPanel = new Panel();
            sidebarPanel.Dock = DockStyle.Left;
            sidebarPanel.Width = 250;
            sidebarPanel.BackColor = DarkColor;
            this.Controls.Add(sidebarPanel);

            // Логотип
            Panel logoPanel = new Panel();
            logoPanel.Dock = DockStyle.Top;
            logoPanel.Height = 120;
            logoPanel.BackColor = Color.FromArgb(33, 47, 61);
            sidebarPanel.Controls.Add(logoPanel);

            Label logoLabel = new Label();
            logoLabel.Text = "🎮\nINVENTORY\nTRACKER";
            logoLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            logoLabel.ForeColor = Color.White;
            logoLabel.Dock = DockStyle.Fill;
            logoLabel.TextAlign = ContentAlignment.MiddleCenter;
            logoPanel.Controls.Add(logoLabel);

            // Панель для скролла
            Panel scrollPanel = new Panel();
            scrollPanel.Dock = DockStyle.Fill;
            scrollPanel.AutoScroll = true;
            sidebarPanel.Controls.Add(scrollPanel);

            // Навигация с верхним отступом 140
            FlowLayoutPanel navPanel = new FlowLayoutPanel();
            navPanel.Dock = DockStyle.Fill;
            navPanel.FlowDirection = FlowDirection.TopDown;
            navPanel.WrapContents = false;
            navPanel.Padding = new Padding(10, 110, 10, 10);
            scrollPanel.Controls.Add(navPanel);

            // Кнопки навигации
            CreateNavButton(navPanel, "➕ Добавить сделку", new EventHandler((s, e) => ShowAddTransactionForm()));
            CreateNavButton(navPanel, "🗑️ Управление сделками", new EventHandler((s, e) => ShowManageTransactionsForm())); // НОВАЯ КНОПКА
            CreateNavButton(navPanel, "📋 История сделок", new EventHandler((s, e) => ShowTransactions()));
            CreateNavButton(navPanel, "📦 Текущий инвентарь", new EventHandler((s, e) => ShowInventoryForm()));
            CreateNavButton(navPanel, "💹 Анализ цен", new EventHandler((s, e) => ShowSteamPrices()));
            CreateNavButton(navPanel, "🎯 Управление предметами", new EventHandler((s, e) => ShowManageItemsForm()));
            CreateNavButton(navPanel, "💰 Реальная прибыль", new EventHandler((s, e) => ShowSalesAnalysisForm()));
            CreateNavButton(navPanel, "📈 Анализ портфеля", new EventHandler((s, e) => ShowPortfolioAnalysisForm()));
            CreateNavButton(navPanel, "🔄 Авто цены Steam", new EventHandler((s, e) => RefreshSteamPrices()));
            CreateNavButton(navPanel, "📥 Импорт из CSV", new EventHandler((s, e) => ShowImportCSVForm()));
            CreateNavButton(navPanel, "⚙️ Управление ценами", new EventHandler((s, e) => ShowManagePricesForm()));
            CreateNavButton(navPanel, "🖼️ Управление кэшем изображений", new EventHandler((s, e) => ShowImageCacheManagement())); // НОВАЯ КНОПКА
        }

        private void CreateNavButton(FlowLayoutPanel parent, string text, EventHandler handler)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Height = 45;
            btn.Width = 230;
            btn.Margin = new Padding(0, 5, 0, 5);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = DarkColor;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 10);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(15, 0, 0, 0);

            btn.Click += handler;

            btn.MouseEnter += (s, e) =>
            {
                if (btn != currentActiveButton)
                    btn.BackColor = Color.FromArgb(52, 73, 94);
            };

            btn.MouseLeave += (s, e) =>
            {
                if (btn != currentActiveButton)
                    btn.BackColor = DarkColor;
            };

            btn.Click += (s, e) => SetActiveButton(btn);

            parent.Controls.Add(btn);
        }

        private void SetActiveButton(Button activeBtn)
        {
            if (currentActiveButton != null)
                currentActiveButton.BackColor = DarkColor;

            currentActiveButton = activeBtn;
            if (currentActiveButton != null)
                currentActiveButton.BackColor = PrimaryColor;
        }

        private void CreateContentArea()
        {
            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = LightColor;
            this.Controls.Add(contentPanel);

            CreateHeader();
            ShowDashboard();
        }

        private void CreateHeader()
        {
            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 80;
            headerPanel.BackColor = Color.White;
            headerPanel.Padding = new Padding(20, 0, 20, 0);
            contentPanel.Controls.Add(headerPanel);

            // Статистика
            Label statsLabel = new Label();
            statsLabel.Name = "statsLabel";
            statsLabel.Text = GetStatsText();
            statsLabel.Dock = DockStyle.Left;
            statsLabel.TextAlign = ContentAlignment.MiddleLeft;
            statsLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            statsLabel.ForeColor = DarkColor;
            headerPanel.Controls.Add(statsLabel);

            // Кнопка обновления
            Button refreshBtn = new Button();
            refreshBtn.Text = "🔄 Обновить";
            refreshBtn.Size = new Size(120, 35);
            refreshBtn.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            refreshBtn.Location = new Point(headerPanel.Width - 140, 20);
            refreshBtn.BackColor = PrimaryColor;
            refreshBtn.ForeColor = Color.White;
            refreshBtn.FlatStyle = FlatStyle.Flat;
            refreshBtn.FlatAppearance.BorderSize = 0;
            refreshBtn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            refreshBtn.Click += (s, e) => RefreshStats();
            headerPanel.Controls.Add(refreshBtn);
        }

        private void ShowDashboard()
        {
            ClearContent();

            // Заголовок
            Label titleLabel = new Label();
            titleLabel.Text = "📊 Обзор инвентаря";
            titleLabel.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            titleLabel.ForeColor = DarkColor;
            titleLabel.Size = new Size(400, 50);
            titleLabel.Location = new Point(270, 80);
            contentPanel.Controls.Add(titleLabel);

            // Получаем статистику по играм
            var inventory = dataService.GetInventory();
            var stats = dataService.GetStatistics();

            // Группируем по играм
            var gamesStats = inventory
                .Where(i => i.Quantity > 0)
                .GroupBy(i => i.Game)
                .Select(g => new {
                    Game = g.Key,
                    ItemsCount = g.Count(),
                    TotalQuantity = g.Sum(i => i.Quantity),
                    TotalValue = g.Sum(i => i.TotalPurchase)
                })
                .OrderByDescending(g => g.TotalValue)
                .ToList();

            int cardWidth = 260;
            int cardHeight = 120;
            int spacing = 20;
            int startX = 280;
            int startY = 140;

            // Карточка 1: Общие сделки
            CreateStatCard(startX, startY, cardWidth, cardHeight,
                "📋 Сделок", stats.TotalTransactions.ToString(),
                PrimaryColor, "Всего операций");

            // Карточка 2: Предметы по играм (самая ценная игра)
            string topGameInfo = gamesStats.Count > 0 ?
                $"{gamesStats[0].Game}: {gamesStats[0].ItemsCount} пред." :
                "Нет предметов";

            CreateStatCard(startX + cardWidth + spacing, startY, cardWidth, cardHeight,
                "🎮 Основная игра", topGameInfo,
                SecondaryColor, gamesStats.Count > 0 ? $"{gamesStats[0].TotalQuantity} шт." : "Пусто");

            // Карточка 3: Прибыль
            CreateStatCard(startX + (cardWidth + spacing) * 2, startY, cardWidth, cardHeight,
                "💰 Прибыль", stats.TotalProfit.ToString("0.00") + " руб.",
                AccentColor, "Общая прибыль");

            // Карточка 4: Всего игр
            CreateStatCard(startX + (cardWidth + spacing) * 3, startY, cardWidth, cardHeight,
                "📊 Всего игр", gamesStats.Count.ToString(),
                Color.FromArgb(155, 89, 182), "С предметами");

            // Детальная статистика по играм (ниже)
            if (gamesStats.Count > 0)
            {
                Label gamesStatsLabel = new Label();
                gamesStatsLabel.Text = "🎯 Статистика по играм";
                gamesStatsLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
                gamesStatsLabel.ForeColor = DarkColor;
                gamesStatsLabel.Size = new Size(300, 40);
                gamesStatsLabel.Location = new Point(280, 280);
                contentPanel.Controls.Add(gamesStatsLabel);

                // Создаем панель для статистики игр
                Panel gamesPanel = new Panel();
                gamesPanel.Location = new Point(280, 330);
                gamesPanel.Size = new Size(600, 150);
                gamesPanel.BackColor = Color.White;
                gamesPanel.BorderStyle = BorderStyle.FixedSingle;
                gamesPanel.Padding = new Padding(10);
                contentPanel.Controls.Add(gamesPanel);

                // Заполняем статистику игр
                int gameY = 10;
                foreach (var gameStat in gamesStats.Take(4)) // Показываем первые 4 игры
                {
                    Label gameLabel = new Label();
                    gameLabel.Text = $"{gameStat.Game}: {gameStat.ItemsCount} предметов, {gameStat.TotalQuantity} шт., {gameStat.TotalValue:0.00} руб.";
                    gameLabel.Font = new Font("Segoe UI", 9);
                    gameLabel.ForeColor = DarkColor;
                    gameLabel.Location = new Point(10, gameY);
                    gameLabel.AutoSize = true;
                    gamesPanel.Controls.Add(gameLabel);
                    gameY += 25;
                }

                if (gamesStats.Count > 4)
                {
                    Label moreLabel = new Label();
                    moreLabel.Text = $"... и еще {gamesStats.Count - 4} игр";
                    moreLabel.Font = new Font("Segoe UI", 8, FontStyle.Italic);
                    moreLabel.ForeColor = Color.Gray;
                    moreLabel.Location = new Point(10, gameY);
                    moreLabel.AutoSize = true;
                    gamesPanel.Controls.Add(moreLabel);
                }

                // Быстрые действия (подвинуты ниже)
                Label quickActionsLabel = new Label();
                quickActionsLabel.Text = "🚀 Быстрые действия";
                quickActionsLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
                quickActionsLabel.ForeColor = DarkColor;
                quickActionsLabel.Size = new Size(300, 40);
                quickActionsLabel.Location = new Point(280, 500);
                contentPanel.Controls.Add(quickActionsLabel);

                // Кнопки быстрых действий
                CreateQuickActionButton(280, 550, "➕ Новая сделка", new EventHandler((s, e) => ShowAddTransactionForm()));
                CreateQuickActionButton(440, 550, "📥 Импорт CSV", new EventHandler((s, e) => ShowImportCSVForm()));
                CreateQuickActionButton(600, 550, "🔄 Цены Steam", new EventHandler((s, e) => RefreshSteamPrices()));
            }
            else
            {
                // Если нет предметов - показываем обычные быстрые действия
                Label quickActionsLabel = new Label();
                quickActionsLabel.Text = "🚀 Быстрые действия";
                quickActionsLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
                quickActionsLabel.ForeColor = DarkColor;
                quickActionsLabel.Size = new Size(300, 40);
                quickActionsLabel.Location = new Point(280, 280);
                contentPanel.Controls.Add(quickActionsLabel);

                CreateQuickActionButton(280, 330, "➕ Новая сделка", new EventHandler((s, e) => ShowAddTransactionForm()));
                CreateQuickActionButton(440, 330, "📥 Импорт CSV", new EventHandler((s, e) => ShowImportCSVForm()));
                CreateQuickActionButton(600, 330, "🔄 Цены Steam", new EventHandler((s, e) => RefreshSteamPrices()));
            }
        }

        private void CreateStatCard(int x, int y, int width, int height, string title, string value, Color color, string description)
        {
            Panel card = new Panel();
            card.Location = new Point(x, y);
            card.Size = new Size(width, height);
            card.BackColor = Color.White;
            card.BorderStyle = BorderStyle.None;
            card.Padding = new Padding(15);

            // Тень
            card.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle,
                    Color.Transparent, 0, ButtonBorderStyle.None,
                    Color.Transparent, 0, ButtonBorderStyle.None,
                    Color.Transparent, 0, ButtonBorderStyle.None,
                    Color.FromArgb(200, 200, 200), 1, ButtonBorderStyle.Solid);
            };

            Label titleLabel = new Label();
            titleLabel.Text = title;
            titleLabel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            titleLabel.ForeColor = color;
            titleLabel.Location = new Point(15, 15);
            titleLabel.AutoSize = true;
            card.Controls.Add(titleLabel);

            Label valueLabel = new Label();
            valueLabel.Text = value;
            valueLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            valueLabel.ForeColor = DarkColor;
            valueLabel.Location = new Point(15, 40);
            valueLabel.AutoSize = true;
            card.Controls.Add(valueLabel);

            Label descLabel = new Label();
            descLabel.Text = description;
            descLabel.Font = new Font("Segoe UI", 8);
            descLabel.ForeColor = Color.Gray;
            descLabel.Location = new Point(15, 85);
            descLabel.AutoSize = true;
            card.Controls.Add(descLabel);

            contentPanel.Controls.Add(card);
        }

        private void CreateQuickActionButton(int x, int y, string text, EventHandler handler)
        {
            Button btn = new Button();
            btn.Location = new Point(x, y);
            btn.Size = new Size(150, 45);
            btn.Text = text;
            btn.BackColor = Color.White;
            btn.ForeColor = DarkColor;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btn.FlatAppearance.BorderSize = 1;
            btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btn.Click += handler;

            contentPanel.Controls.Add(btn);
        }

        private void ClearContent()
        {
            var controlsToRemove = new List<Control>();
            foreach (Control control in contentPanel.Controls)
            {
                if (!(control is Panel && control.Dock == DockStyle.Top))
                {
                    controlsToRemove.Add(control);
                }
            }

            foreach (var control in controlsToRemove)
            {
                contentPanel.Controls.Remove(control);
                control.Dispose();
            }
        }

        // Обновляем метод получения статистики для заголовка
        private string GetStatsText()
        {
            var stats = dataService.GetStatistics();
            var inventory = dataService.GetInventory();

            // Считаем предметы по играм
            var gamesCount = inventory
                .Where(i => i.Quantity > 0)
                .GroupBy(i => i.Game)
                .Count();

            var totalItems = inventory
                .Where(i => i.Quantity > 0)
                .Sum(i => i.Quantity);

            return $"📊 Статистика: {stats.TotalTransactions} сделок | {gamesCount} игр | {totalItems} предметов | Прибыль: {stats.TotalProfit:0.00} руб.";
        }

        // ===== МЕТОДЫ ПОКАЗА ФОРМ =====

        private async void RefreshSteamPrices()
        {
            try
            {
                await dataService.RefreshSteamPricesAsync();
                RefreshStats();
                ShowDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка при обновлении цен: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowManagePricesForm()
        {
            using (var form = new ManagePricesForm(dataService))
                form.ShowDialog();
        }

        private void ShowSalesAnalysisForm()
        {
            using (var form = new SalesAnalysisForm(dataService))
                form.ShowDialog();
        }

        private void ShowImportCSVForm()
        {
            using (var form = new ImportInventoryForm(dataService))
                form.ShowDialog();
        }

        private void ShowAddTransactionForm()
        {
            using (var form = new AddTransactionForm(dataService))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    dataService.AddTransaction(form.NewTransaction);
                    RefreshStats();
                    ShowDashboard();
                }
            }
        }

        private void ShowTransactions()
        {
            var transactions = dataService.GetTransactions();
            if (transactions.Count == 0)
            {
                MessageBox.Show("История сделок пуста!\nДобавьте первую сделку.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var form = new Form())
            {
                form.Text = "📋 История сделок";
                form.Size = new Size(1000, 600);
                form.StartPosition = FormStartPosition.CenterScreen;
                form.BackColor = Color.White;

                DataGridView dgv = CreateTransactionsGrid(transactions);
                dgv.Dock = DockStyle.Fill;

                form.Controls.Add(dgv);
                form.ShowDialog();
            }
        }

        private void ShowInventoryForm()
        {
            var inventory = dataService.GetInventory();
            if (inventory.Count == 0 || inventory.All(i => i.Quantity == 0))
            {
                MessageBox.Show("Инвентарь пуст!\nДобавьте первую сделку.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var form = new InventoryWithPricesForm(dataService))
            {
                form.ShowDialog();
            }
        }

        private void ShowManageItemsForm()
        {
            using (var form = new ManageItemsForm(dataService))
                form.ShowDialog();
        }

        private void ShowSteamPrices()
        {
            using (var form = new InventoryWithPricesForm(dataService))
                form.ShowDialog();
        }

        private void ShowPortfolioAnalysisForm()
        {
            using (var form = new PortfolioAnalysisForm(dataService))
            {
                form.Size = new Size(1400, 700);
                form.StartPosition = FormStartPosition.CenterScreen;
                form.ShowDialog();
            }
        }

        // НОВЫЙ МЕТОД: Управление кэшем изображений
        private void ShowImageCacheManagement()
        {
            long cacheSize = dataService.GetImageCacheSize();
            string message = $"Размер кэша изображений: {cacheSize / 1024 / 1024} MB\n\n" +
                            "Очистить кэш изображений? Это удалит все загруженные изображения и они будут загружены заново при следующем просмотре.";

            var result = MessageBox.Show(message, "Управление кэшем изображений",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                dataService.ClearImageCache();
                MessageBox.Show("Кэш изображений очищен!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // НОВЫЙ МЕТОД: Управление сделками
        private void ShowManageTransactionsForm()
        {
            var transactions = dataService.GetTransactions();
            if (transactions.Count == 0)
            {
                MessageBox.Show("Нет сделок для управления!", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var form = new ManageTransactionsForm(dataService))
            {
                form.ShowDialog();
            }
        }

        private DataGridView CreateTransactionsGrid(List<Transaction> transactions)
        {
            DataGridView dgv = new DataGridView();
            dgv.BackgroundColor = Color.White;
            dgv.ReadOnly = true;
            dgv.RowHeadersVisible = false;
            dgv.Font = new Font("Arial", 8);
            dgv.AllowUserToAddRows = false;

            string[] columns = { "Дата", "Игра", "Предмет", "Тип", "Кол-во", "Цена", "Сумма" };
            foreach (string col in columns) dgv.Columns.Add(col, col);

            foreach (var t in transactions)
            {
                int rowIndex = dgv.Rows.Add(
                    t.Date.ToString("dd.MM.yyyy HH:mm"),
                    t.Game,
                    t.Item,
                    t.Operation,
                    t.Quantity,
                    t.Price.ToString("0.00") + " руб.",
                    t.Total.ToString("0.00") + " руб."
                );

                Color bgColor = t.Operation == "Покупка" ? Color.LavenderBlush :
                               t.Operation == "Продажа" ? Color.Honeydew : Color.AliceBlue;
                dgv.Rows[rowIndex].DefaultCellStyle.BackColor = bgColor;
            }

            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            return dgv;
        }

        private void RefreshStats()
        {
            var statsLabel = contentPanel.Controls.Find("statsLabel", true).FirstOrDefault() as Label;
            if (statsLabel != null)
            {
                statsLabel.Text = GetStatsText();
            }
        }
    }
}