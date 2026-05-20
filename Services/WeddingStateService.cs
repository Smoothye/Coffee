namespace WeddingPlannerApp.Services
{
    public class WeddingStateService
    {
        public event Action? OnChange;
        void Notify() => OnChange?.Invoke();

        public DateTime WeddingDate { get; set; } = DateTime.Today;
        public string VenueName { get; set; } = "";
        public int ExpectedGuests { get; set; }
        public string SelectedMenu { get; set; } = "";

        public int BudgetTotal { get; set; }

        private List<BudgetItem> _budgetItems = [];

        public IReadOnlyList<BudgetItem> BudgetItems => _budgetItems;

        public int BudgetSpent => _budgetItems.Sum(i => i.Actual);
        public int BudgetEstimated => _budgetItems.Sum(i => i.Estimated);
        public int BudgetPaid => _budgetItems.Where(i => i.Paid).Sum(i => i.Actual);
        public int BudgetUnpaid => _budgetItems.Where(i => !i.Paid).Sum(i => i.Actual > 0 ? i.Actual : i.Estimated);
        public int BudgetRemaining => BudgetTotal - BudgetEstimated;
        public int BudgetPercent => BudgetTotal > 0 ? BudgetEstimated * 100 / BudgetTotal : 0;

        public void UpdateBudgetTotal(int total)
        {
            BudgetTotal = Math.Max(0, total);
            Notify();
        }

        public void AddBudgetItem(BudgetItem item)
        {
            item.Id = _budgetItems.Count > 0 ? _budgetItems.Max(i => i.Id) + 1 : 1;
            _budgetItems.Add(item);
            Notify();
        }

        public void UpdateBudgetItem(BudgetItem item)
        {
            var idx = _budgetItems.FindIndex(i => i.Id == item.Id);
            if (idx >= 0) { _budgetItems[idx] = item; Notify(); }
        }

        public void RemoveBudgetItem(int id)
        {
            _budgetItems.RemoveAll(i => i.Id == id && !i.IsProtected);
            Notify();
        }

        public void TogglePaid(int id)
        {
            var item = _budgetItems.FirstOrDefault(i => i.Id == id);
            if (item != null) { item.Paid = !item.Paid; Notify(); }
        }

        public void UpsertSupplierBudgetItem(int supplierId, string name, string category, int estimated, int actual, bool paid)
        {
            var item = _budgetItems.FirstOrDefault(i => i.SupplierId == supplierId);
            if (item is null)
            {
                if (estimated <= 0 && actual <= 0) return;

                AddBudgetItem(new BudgetItem
                {
                    SupplierId = supplierId,
                    Name = name,
                    Category = category,
                    Estimated = estimated,
                    Actual = actual,
                    Paid = paid
                });
                return;
            }

            item.Name = name;
            item.Category = category;
            item.Estimated = estimated;
            item.Actual = actual;
            item.Paid = paid;
            Notify();
        }

        public void RemoveSupplierBudgetItem(int supplierId)
        {
            var removed = _budgetItems.RemoveAll(i => i.SupplierId == supplierId);
            if (removed > 0) Notify();
        }

        const int MenuBudgetSourceId = -1000;
        const int VenueBudgetSourceId = -1001;

        public void UpsertMenuBudgetItem(int estimated)
        {
            var item = _budgetItems.FirstOrDefault(i => i.SupplierId == MenuBudgetSourceId);
            if (estimated <= 0)
            {
                if (item is not null)
                    RemoveBudgetItem(item.Id);
                return;
            }

            if (item is null)
            {
                AddBudgetItem(new BudgetItem
                {
                    SupplierId = MenuBudgetSourceId,
                    Name = "Menus estimate",
                    Category = "Catering",
                    Estimated = estimated,
                    Actual = estimated,
                    Paid = false
                });
                return;
            }

            item.Name = "Menus estimate";
            item.Category = "Catering";
            item.Estimated = estimated;
            item.Actual = estimated;
            Notify();
        }

        public void UpsertVenueBudgetItem(string venueName, int estimated)
        {
            var item = _budgetItems.FirstOrDefault(i => i.SupplierId == VenueBudgetSourceId);
            if (estimated <= 0)
                return;

            var name = string.IsNullOrWhiteSpace(venueName) ? "Venue estimate" : $"Venue: {venueName}";

            if (item is null)
            {
                AddBudgetItem(new BudgetItem
                {
                    SupplierId = VenueBudgetSourceId,
                    Name = name,
                    Category = "Venue",
                    Estimated = estimated,
                    Actual = estimated,
                    Paid = false
                });
                return;
            }

            item.Name = name;
            item.Category = "Venue";
            item.Estimated = estimated;
            item.Actual = estimated;
            Notify();
        }

        private List<WeddingTask> _tasks = [];

        public IReadOnlyList<WeddingTask> Tasks => _tasks;

        public List<WeddingTask> UrgentTasks =>
            _tasks.Where(t => !t.Done && (t.DueDate - DateTime.Today).Days <= 14)
                  .OrderBy(t => t.DueDate).ToList();

        public void AddTask(WeddingTask task)
        {
            task.Id = _tasks.Count > 0 ? _tasks.Max(t => t.Id) + 1 : 1;
            _tasks.Add(task);
            Notify();
        }

        public void ToggleTask(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task != null) { task.Done = !task.Done; Notify(); }
        }

        public void RemoveTask(int id)
        {
            _tasks.RemoveAll(t => t.Id == id);
            Notify();
        }

        public string SelectedMenuId { get; private set; } = "";
        public string SelectedMenuName { get; private set; } = "";

        public void SelectMenu(string id, string name)
        {
            SelectedMenuId = id;
            SelectedMenuName = name;
            SelectedMenu = name;
            Notify();
        }
    }

    public class BudgetItem
    {
        public int Id { get; set; }
        public int? SupplierId { get; set; }
        public bool IsProtected => SupplierId is < 0;
        public string Name { get; set; } = "";
        public string Category { get; set; } = "Other";
        public int Estimated { get; set; }
        public int Actual { get; set; }
        public bool Paid { get; set; }
    }

    public class WeddingTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Category { get; set; } = "General";
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(7);
        public string Priority { get; set; } = "medium";
        public bool Done { get; set; }

        public bool IsUrgent => !Done && (DueDate - DateTime.Today).Days <= 14;
        public bool IsOverdue => !Done && DueDate < DateTime.Today;
    }
}
