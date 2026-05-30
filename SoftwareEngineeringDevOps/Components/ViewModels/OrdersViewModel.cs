using System.Globalization;
using SoftwareEngineeringDevOps.App.Auth;
using SoftwareEngineeringDevOps.App.Bricks;
using SoftwareEngineeringDevOps.App.BrickOrders;
using SoftwareEngineeringDevOps.App.BrickOrdersReceived;
using SoftwareEngineeringDevOps.App.Validation;

namespace SoftwareEngineeringDevOps.Components.ViewModels
{
    public class OrdersViewModel
    {
        private readonly IBrickOrdersMediator _ordersMediator;
        private readonly IBrickOrdersReceivedMediator _receivedMediator;
        private readonly IBricksMediator _bricksMediator;
        private readonly IAuthService _authService;

        public OrdersViewModel(
            IBrickOrdersMediator ordersMediator,
            IBrickOrdersReceivedMediator receivedMediator,
            IBricksMediator bricksMediator,
            IAuthService authService)
        {
            _ordersMediator = ordersMediator;
            _receivedMediator = receivedMediator;
            _bricksMediator = bricksMediator;
            _authService = authService;
        }

        public IEnumerable<IBrickOrder> AllOrders { get; set; } = Enumerable.Empty<IBrickOrder>();
        public IEnumerable<IBrick> Bricks { get; set; } = Enumerable.Empty<IBrick>();
        public IEnumerable<IGrouping<string, IBrickOrder>> OrderGroups { get; set; } = Enumerable.Empty<IGrouping<string, IBrickOrder>>();
        public string? SelectedOrderNo { get; set; }
        public IEnumerable<IBrickOrder> SelectedOrderLines { get; set; } = Enumerable.Empty<IBrickOrder>();
        public Dictionary<long, IEnumerable<IBrickOrderReceived>> ReceivedByOrderLine { get; set; } = new();
        public Dictionary<long, bool> ExpandedRows { get; set; } = new();

        public NewBrickOrder NewOrderModel { get; set; } = new();
        public EditBrickOrder? EditOrderModel { get; set; }
        public NewBrickOrderReceived NewReceivedModel { get; set; } = new();
        public EditBrickOrderReceived? EditReceivedModel { get; set; }

        public bool IsLoading { get; set; }
        public bool ShowAddOrderModal { get; set; }
        public bool ShowEditOrderModal { get; set; }
        public bool ShowDeleteOrderConfirm { get; set; }
        public bool ShowAddReceivedModal { get; set; }
        public bool ShowEditReceivedModal { get; set; }
        public bool ShowDeleteReceivedConfirm { get; set; }

        public IBrickOrder? OrderToDelete { get; set; }
        public IBrickOrderReceived? ReceivedToDelete { get; set; }

        public List<string> ValidationErrors { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string OrderGroupsSearchTerm { get; set; } = string.Empty;
        public string ReceivedSearchTerm { get; set; } = string.Empty;

        public UserRole CurrentUserRole => _authService.CurrentUser != null
            ? RoleHelper.GetRole(_authService.CurrentUser)
            : UserRole.Standard;

        public IEnumerable<IGrouping<string, IBrickOrder>> FilteredOrderGroups =>
            OrderGroups.Where(group =>
                string.IsNullOrWhiteSpace(OrderGroupsSearchTerm)
                || group.Key.Contains(OrderGroupsSearchTerm, StringComparison.OrdinalIgnoreCase)
                || group.Count().ToString().Contains(OrderGroupsSearchTerm, StringComparison.OrdinalIgnoreCase)
                || group.First().OrderedDate.ToString("dd/MM/yyyy").Contains(OrderGroupsSearchTerm, StringComparison.OrdinalIgnoreCase))
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase);

        public IEnumerable<IBrickOrder> SortedSelectedOrderLines =>
            SelectedOrderLines
                .OrderBy(GetOrderLineSortBucket)
                .ThenByDescending(GetOrderLineSortDate)
                .ThenByDescending(l => l.OrderedDate)
                .ThenBy(l => l.Id);

        public long CurrentUserId => _authService.CurrentUser?.Id ?? 0;
        public DateTime Today => DateTime.Today;
        public string TodayInputValue => Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        public async Task LoadOrders()
        {
            IsLoading = true;
            AllOrders = await _ordersMediator.GetAllBrickOrders();
            Bricks = await _bricksMediator.GetAllBricks();
            OrderGroups = AllOrders.GroupBy(o => o.OrderNo);
            IsLoading = false;
        }

        public async Task SelectOrderGroup(string orderNo)
        {
            SelectedOrderNo = orderNo;
            SelectedOrderLines = AllOrders.Where(o => o.OrderNo == orderNo);
            ReceivedByOrderLine.Clear();
            ExpandedRows.Clear();

            foreach (var line in SelectedOrderLines)
            {
                var received = await _receivedMediator.GetBrickOrdersReceivedByBrickOrderId(line.Id);
                ReceivedByOrderLine[line.Id] = received;
            }
        }

        public void ToggleRowExpansion(long orderLineId)
        {
            if (ExpandedRows.ContainsKey(orderLineId))
                ExpandedRows[orderLineId] = !ExpandedRows[orderLineId];
            else
                ExpandedRows[orderLineId] = true;
        }

        public bool IsRowExpanded(long orderLineId) =>
            ExpandedRows.TryGetValue(orderLineId, out var expanded) && expanded;

        public decimal GetFulfillmentPercentage(IBrickOrder orderLine)
        {
            ArgumentNullException.ThrowIfNull(orderLine);

            if (orderLine.BricksOrdered <= 0)
            {
                return 0;
            }

            var totalReceived = GetTotalReceived(orderLine);
            if (totalReceived <= 0)
            {
                return 0;
            }

            return Math.Min(100, Math.Round((decimal)totalReceived / orderLine.BricksOrdered * 100, 1));
        }

        public int GetTotalReceived(IBrickOrder orderLine)
        {
            ArgumentNullException.ThrowIfNull(orderLine);

            if (!ReceivedByOrderLine.TryGetValue(orderLine.Id, out var received))
            {
                return 0;
            }

            return received.Sum(r => r.BricksReceived);
        }

        public bool HasReceivedItems(long orderLineId)
        {
            if (!ReceivedByOrderLine.TryGetValue(orderLineId, out var received)) return false;
            return received.Any();
        }

        public bool IsOrderFullyReceived(IBrickOrder orderLine)
        {
            ArgumentNullException.ThrowIfNull(orderLine);

            return orderLine.BricksOrdered > 0 && GetTotalReceived(orderLine) >= orderLine.BricksOrdered;
        }

        public bool IsOrderDueToday(IBrickOrder orderLine)
        {
            ArgumentNullException.ThrowIfNull(orderLine);

            return orderLine.CancelledDate == null
                && !IsOrderFullyReceived(orderLine)
                && orderLine.ExpectedDate.Date == Today;
        }

        public bool IsOrderOverdue(IBrickOrder orderLine)
        {
            ArgumentNullException.ThrowIfNull(orderLine);

            return orderLine.CancelledDate == null
                && !IsOrderFullyReceived(orderLine)
                && orderLine.ExpectedDate.Date < Today;
        }

        public bool CanToggleCancellation(IBrickOrder orderLine) =>
            RoleHelper.CanEdit(CurrentUserRole) && !HasReceivedItems(orderLine.Id);

        public string GetOrderLineStateClass(IBrickOrder orderLine)
        {
            ArgumentNullException.ThrowIfNull(orderLine);

            if (orderLine.CancelledDate != null)
            {
                return "order-line-card-cancelled";
            }

            if (IsOrderDueToday(orderLine))
            {
                return "order-line-card-due-today";
            }

            if (IsOrderOverdue(orderLine))
            {
                return "order-line-card-overdue";
            }

            if (IsOrderFullyReceived(orderLine))
            {
                return "order-line-card-fulfilled";
            }

            return string.Empty;
        }

        public string? GetOrderLineStatusText(IBrickOrder orderLine)
        {
            ArgumentNullException.ThrowIfNull(orderLine);

            if (orderLine.CancelledDate != null)
            {
                return "Cancelled";
            }

            if (IsOrderDueToday(orderLine))
            {
                return "Due today";
            }

            if (IsOrderOverdue(orderLine))
            {
                return "Overdue";
            }

            if (IsOrderFullyReceived(orderLine))
            {
                return "Fully received";
            }

            return null;
        }

        private int GetOrderLineSortBucket(IBrickOrder orderLine)
        {
            ArgumentNullException.ThrowIfNull(orderLine);

            if (orderLine.CancelledDate != null)
            {
                return 4;
            }

            if (IsOrderDueToday(orderLine))
            {
                return 0;
            }

            if (IsOrderOverdue(orderLine))
            {
                return 1;
            }

            if (IsOrderFullyReceived(orderLine))
            {
                return 3;
            }

            return 2;
        }

        private DateTime GetOrderLineSortDate(IBrickOrder orderLine)
        {
            ArgumentNullException.ThrowIfNull(orderLine);

            return orderLine.CancelledDate?.Date
                ?? orderLine.ExpectedDate.Date;
        }

        public async Task<bool> ToggleOrderCancellation(IBrickOrder orderLine)
        {
            ArgumentNullException.ThrowIfNull(orderLine);

            IsLoading = true;

            if (orderLine.CancelledDate == null)
            {
                await _ordersMediator.Cancel(orderLine.Id);
            }
            else
            {
                await _ordersMediator.Uncancel(orderLine.Id);
            }

            await LoadOrders();
            if (SelectedOrderNo != null) await SelectOrderGroup(SelectedOrderNo);
            IsLoading = false;
            return true;
        }

        public IEnumerable<IBrickOrderReceived> FilterReceivedItems(IEnumerable<IBrickOrderReceived> receivedItems) =>
            receivedItems.Where(rcv =>
                string.IsNullOrWhiteSpace(ReceivedSearchTerm)
                || rcv.BricksReceived.ToString().Contains(ReceivedSearchTerm, StringComparison.OrdinalIgnoreCase)
                || rcv.ReceivedDate.ToString("dd/MM/yyyy").Contains(ReceivedSearchTerm, StringComparison.OrdinalIgnoreCase)
                || $"{rcv.ReceivedBy.FirstName} {rcv.ReceivedBy.LastName}".Contains(ReceivedSearchTerm, StringComparison.OrdinalIgnoreCase));
        // Order CRUD
        public void OpenAddOrderModal()
        {
            OpenAddOrderModal(null);
        }

        public void OpenAddOrderModal(string? orderNo)
        {
            NewOrderModel = new NewBrickOrder
            {
                OrderNo = orderNo ?? string.Empty,
                OrderedDate = Today,
                ExpectedDate = Today.AddDays(14),
                CreatedById = CurrentUserId
            };
            ValidationErrors.Clear();
            ShowAddOrderModal = true;
        }

        public void CloseAddOrderModal()
        {
            ShowAddOrderModal = false;
            ValidationErrors.Clear();
        }

        public async Task<bool> AddOrder()
        {
            ValidationErrors.Clear();
            NewOrderModel.CreatedById = CurrentUserId;
            var errors = InputValidator.ValidateBrickOrder(NewOrderModel);
            errors.AddRange(ValidateOrderDates(NewOrderModel.OrderedDate, NewOrderModel.ExpectedDate));
            if (errors.Count > 0)
            {
                ValidationErrors = errors;
                return false;
            }

            IsLoading = true;
            await _ordersMediator.Insert(NewOrderModel);
            await LoadOrders();
            if (SelectedOrderNo != null) await SelectOrderGroup(SelectedOrderNo);
            ShowAddOrderModal = false;
            IsLoading = false;
            return true;
        }

        public void OpenEditOrderModal(IBrickOrder order)
        {
            EditOrderModel = new EditBrickOrder(order);
            ValidationErrors.Clear();
            ShowEditOrderModal = true;
        }

        public void CloseEditOrderModal()
        {
            ShowEditOrderModal = false;
            ValidationErrors.Clear();
        }

        public async Task<bool> UpdateOrder()
        {
            if (EditOrderModel == null) return false;

            ValidationErrors.Clear();
            var errors = InputValidator.ValidateBrickOrder(EditOrderModel);
            errors.AddRange(ValidateOrderDates(EditOrderModel.OrderedDate, EditOrderModel.ExpectedDate));
            if (errors.Count > 0)
            {
                ValidationErrors = errors;
                return false;
            }

            IsLoading = true;
            await _ordersMediator.Update(EditOrderModel);
            await LoadOrders();
            if (SelectedOrderNo != null) await SelectOrderGroup(SelectedOrderNo);
            ShowEditOrderModal = false;
            IsLoading = false;
            return true;
        }

        public void OpenDeleteOrderConfirm(IBrickOrder order)
        {
            OrderToDelete = order;
            ErrorMessage = null;
            ShowDeleteOrderConfirm = true;
        }

        public void CloseDeleteOrderConfirm()
        {
            ShowDeleteOrderConfirm = false;
            ErrorMessage = null;
        }

        public async Task<bool> DeleteOrder()
        {
            if (OrderToDelete == null) return false;

            IsLoading = true;

            // Delete child received records first
            var received = await _receivedMediator.GetBrickOrdersReceivedByBrickOrderId(OrderToDelete.Id);
            foreach (var r in received)
            {
                await _receivedMediator.Delete(r.Id);
            }

            await _ordersMediator.Delete(OrderToDelete.Id);
            await LoadOrders();
            if (SelectedOrderNo != null) await SelectOrderGroup(SelectedOrderNo);
            ShowDeleteOrderConfirm = false;
            OrderToDelete = null;
            IsLoading = false;
            return true;
        }

        // Received CRUD
        public void OpenAddReceivedModal(long brickOrderId)
        {
            NewReceivedModel = new NewBrickOrderReceived
            {
                BrickOrderId = brickOrderId,
                ReceivedDate = DateTime.UtcNow,
                ReceivedById = CurrentUserId
            };
            ValidationErrors.Clear();
            ShowAddReceivedModal = true;
        }

        public void CloseAddReceivedModal()
        {
            ShowAddReceivedModal = false;
            ValidationErrors.Clear();
        }

        public async Task<bool> AddReceived()
        {
            ValidationErrors.Clear();
            NewReceivedModel.ReceivedById = CurrentUserId;
            var errors = InputValidator.ValidateBrickOrderReceived(NewReceivedModel);
            if (errors.Count > 0)
            {
                ValidationErrors = errors;
                return false;
            }

            IsLoading = true;
            await _receivedMediator.Insert(NewReceivedModel);
            await LoadOrders();
            if (SelectedOrderNo != null) await SelectOrderGroup(SelectedOrderNo);
            ShowAddReceivedModal = false;
            IsLoading = false;
            return true;
        }

        public void OpenEditReceivedModal(IBrickOrderReceived received)
        {
            EditReceivedModel = new EditBrickOrderReceived(received);
            ValidationErrors.Clear();
            ShowEditReceivedModal = true;
        }

        public void CloseEditReceivedModal()
        {
            ShowEditReceivedModal = false;
            ValidationErrors.Clear();
        }

        public async Task<bool> UpdateReceived()
        {
            if (EditReceivedModel == null) return false;

            ValidationErrors.Clear();
            var errors = InputValidator.ValidateBrickOrderReceived(EditReceivedModel);
            if (errors.Count > 0)
            {
                ValidationErrors = errors;
                return false;
            }

            IsLoading = true;
            await _receivedMediator.Update(EditReceivedModel);
            await LoadOrders();
            if (SelectedOrderNo != null) await SelectOrderGroup(SelectedOrderNo);
            ShowEditReceivedModal = false;
            IsLoading = false;
            return true;
        }

        public void OpenDeleteReceivedConfirm(IBrickOrderReceived received)
        {
            ReceivedToDelete = received;
            ShowDeleteReceivedConfirm = true;
        }

        public void CloseDeleteReceivedConfirm()
        {
            ShowDeleteReceivedConfirm = false;
        }

        public async Task<bool> DeleteReceived()
        {
            if (ReceivedToDelete == null) return false;

            IsLoading = true;
            await _receivedMediator.Delete(ReceivedToDelete.Id);
            await LoadOrders();
            if (SelectedOrderNo != null) await SelectOrderGroup(SelectedOrderNo);
            ShowDeleteReceivedConfirm = false;
            ReceivedToDelete = null;
            IsLoading = false;
            return true;
        }

        private List<string> ValidateOrderDates(DateTime orderedDate, DateTime expectedDate)
        {
            var errors = new List<string>();

            if (orderedDate.Date > Today)
            {
                errors.Add("Order date cannot be greater than today.");
            }

            if (expectedDate.Date < Today)
            {
                errors.Add("Expected date cannot be less than today.");
            }

            return errors;
        }

        public string FormatPrice(decimal price) => price.ToString("C2", CultureInfo.GetCultureInfo("en-GB"));
    }
}
