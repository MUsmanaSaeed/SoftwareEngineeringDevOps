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

        private static readonly CultureInfo EnGbCulture = CultureInfo.GetCultureInfo("en-GB");

        // Backing fields for cached/materialized data
        private List<IBrickOrder> _allOrders = new();
        private Dictionary<long, IBrick> _brickById = new();
        private Dictionary<long, List<IBrickOrderReceived>> _allReceivedByOrderLine = new();
        private List<IGrouping<string, IBrickOrder>> _orderGroups = new();
        private List<IBrickOrder> _selectedOrderLines = new();
        private UserRole _currentUserRole;

        // Cached aggregates for selected order (invalidated when selection changes)
        private List<IBrickOrder>? _sortedSelectedOrderLines;
        private DateTime? _selectedOrderFirstOrderedDate;
        private DateTime? _selectedOrderLastExpectedDate;
        private int? _selectedOrderTotalOrdered;
        private int? _selectedOrderTotalReceived;

        public IEnumerable<IBrickOrder> AllOrders => _allOrders;
        public IEnumerable<IBrick> Bricks => _brickById.Values;
        public IEnumerable<IGrouping<string, IBrickOrder>> OrderGroups => _orderGroups;
        public string? SelectedOrderNo { get; set; }
        public IEnumerable<IBrickOrder> SelectedOrderLines => _selectedOrderLines;
        public Dictionary<long, IEnumerable<IBrickOrderReceived>> ReceivedByOrderLine { get; set; } = new();
        public Dictionary<long, bool> ExpandedRows { get; set; } = new();

        public NewBrickOrder NewOrderModel { get; set; } = new();
        public EditBrickOrder? EditOrderModel { get; set; }
        public NewBrickOrderReceived NewReceivedModel { get; set; } = new();
        public EditBrickOrderReceived? EditReceivedModel { get; set; }
        public long? EditReceivedBrickOrderId { get; set; }

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

        public UserRole CurrentUserRole => _currentUserRole;

        public IEnumerable<IGrouping<string, IBrickOrder>> FilteredOrderGroups
        {
            get
            {
                if (string.IsNullOrWhiteSpace(OrderGroupsSearchTerm))
                {
                    return _orderGroups.OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase);
                }

                return _orderGroups
                    .Where(group =>
                    {
                        var first = group.First();
                        return group.Key.Contains(OrderGroupsSearchTerm, StringComparison.OrdinalIgnoreCase)
                            || group.Count().ToString().Contains(OrderGroupsSearchTerm, StringComparison.OrdinalIgnoreCase)
                            || first.OrderedDate.ToString("dd/MM/yyyy").Contains(OrderGroupsSearchTerm, StringComparison.OrdinalIgnoreCase);
                    })
                    .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase);
            }
        }

        public IEnumerable<IBrickOrder> SortedSelectedOrderLines =>
            _sortedSelectedOrderLines ??= _selectedOrderLines
                .OrderBy(GetOrderLineSortBucket)
                .ThenByDescending(GetOrderLineSortDate)
                .ThenByDescending(l => l.OrderedDate)
                .ThenBy(l => l.Id)
                .ToList();

        public DateTime? SelectedOrderFirstOrderedDate =>
            _selectedOrderFirstOrderedDate ??= _selectedOrderLines.Count > 0
                ? _selectedOrderLines.Min(line => line.OrderedDate.Date)
                : null;

        public DateTime? SelectedOrderLastExpectedDate =>
            _selectedOrderLastExpectedDate ??= _selectedOrderLines.Count > 0
                ? _selectedOrderLines.Max(line => line.ExpectedDate.Date)
                : null;

        public int SelectedOrderTotalOrdered =>
            _selectedOrderTotalOrdered ??= _selectedOrderLines.Sum(line => line.BricksOrdered);

        public int SelectedOrderTotalReceived =>
            _selectedOrderTotalReceived ??= _selectedOrderLines.Sum(line => Math.Min(line.BricksOrdered, GetTotalReceived(line)));

        public decimal SelectedOrderFulfillmentPercentage
        {
            get
            {
                if (SelectedOrderTotalOrdered <= 0 || SelectedOrderTotalReceived <= 0)
                {
                    return 0;
                }

                return Math.Min(100, Math.Round((decimal)SelectedOrderTotalReceived / SelectedOrderTotalOrdered * 100, 1));
            }
        }

        public long CurrentUserId => _authService.CurrentUser?.Id ?? 0;
        public DateTime Today => DateTime.Now.ToUkTime();
        public DateTime TodayDate => Today.Date;
        public string TodayInputValue => Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        public string? EditOrderMinExpectedDateInputValue =>
            EditOrderModel?.OrderedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        public async Task LoadOrders()
        {
            IsLoading = true;
            _currentUserRole = _authService.CurrentUser != null
                ? RoleHelper.GetRole(_authService.CurrentUser)
                : UserRole.Standard;

            var ordersTask = _ordersMediator.GetAllBrickOrders();
            var bricksTask = _bricksMediator.GetAllBricks();
            var receivedTask = _receivedMediator.GetAllBrickOrdersReceived();
            await Task.WhenAll(ordersTask, bricksTask, receivedTask);

            _allOrders = (await ordersTask).ToList();
            _brickById = (await bricksTask).ToDictionary(b => b.Id);
            _allReceivedByOrderLine = (await receivedTask)
                .GroupBy(r => r.BrickOrderId)
                .ToDictionary(g => g.Key, g => g.ToList());
            _orderGroups = _allOrders.GroupBy(o => o.OrderNo).ToList();
            IsLoading = false;
        }

        /// <summary>
        /// Synchronously applies the selection so the UI can render immediately.
        /// Call <see cref="LoadSelectedOrderReceivedAsync"/> afterwards to populate received items.
        /// </summary>
        public void ApplyOrderGroupSelection(string orderNo)
        {
            SelectedOrderNo = orderNo;
            _selectedOrderLines = _allOrders.Where(o => o.OrderNo == orderNo).ToList();
            ReceivedByOrderLine.Clear();
            ExpandedRows.Clear();
            InvalidateSelectionCache();
        }

        /// <summary>
        /// Populates ReceivedByOrderLine for the currently selected lines from the in-memory cache.
        /// Returns immediately — no DB calls.
        /// </summary>
        public Task LoadSelectedOrderReceivedAsync()
        {
            foreach (var line in _selectedOrderLines)
            {
                ReceivedByOrderLine[line.Id] = _allReceivedByOrderLine.TryGetValue(line.Id, out var items)
                    ? items
                    : [];
            }

            _selectedOrderTotalReceived = null;
            return Task.CompletedTask;
        }

        public async Task SelectOrderGroup(string orderNo)
        {
            ApplyOrderGroupSelection(orderNo);
            await LoadSelectedOrderReceivedAsync();
        }

        private void InvalidateSelectionCache()
        {
            _sortedSelectedOrderLines = null;
            _selectedOrderFirstOrderedDate = null;
            _selectedOrderLastExpectedDate = null;
            _selectedOrderTotalOrdered = null;
            _selectedOrderTotalReceived = null;
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
                && orderLine.ExpectedDate.Date == TodayDate;
        }

        public bool IsOrderOverdue(IBrickOrder orderLine)
        {
            ArgumentNullException.ThrowIfNull(orderLine);

            return orderLine.CancelledDate == null
                && !IsOrderFullyReceived(orderLine)
                && orderLine.ExpectedDate.Date < TodayDate;
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
            errors.AddRange(ValidateOrderDates(EditOrderModel.OrderedDate, EditOrderModel.ExpectedDate, EditOrderModel.OrderedDate));
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

            // Use the in-memory cache to find child received records — no extra DB call needed
            if (_allReceivedByOrderLine.TryGetValue(OrderToDelete.Id, out var received))
            {
                foreach (var r in received)
                {
                    await _receivedMediator.Delete(r.Id);
                }
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
            errors.AddRange(ValidateReceivedDate(NewReceivedModel.BrickOrderId, NewReceivedModel.ReceivedDate));
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
            EditReceivedBrickOrderId = received.BrickOrderId;
            ValidationErrors.Clear();
            ShowEditReceivedModal = true;
        }

        public void CloseEditReceivedModal()
        {
            ShowEditReceivedModal = false;
            EditReceivedBrickOrderId = null;
            ValidationErrors.Clear();
        }

        public async Task<bool> UpdateReceived()
        {
            if (EditReceivedModel == null) return false;

            ValidationErrors.Clear();
            var errors = InputValidator.ValidateBrickOrderReceived(EditReceivedModel);
            if (EditReceivedBrickOrderId.HasValue)
            {
                errors.AddRange(ValidateReceivedDate(EditReceivedBrickOrderId.Value, EditReceivedModel.ReceivedDate));
            }

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
            EditReceivedBrickOrderId = null;
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

        private List<string> ValidateOrderDates(DateTime orderedDate, DateTime expectedDate, DateTime? minExpectedDate = null)
        {
            var errors = new List<string>();

            if (orderedDate.Date > TodayDate)
            {
                errors.Add("Order date cannot be greater than today.");
            }

            var effectiveMin = (minExpectedDate ?? Today).Date;
            if (expectedDate.Date < effectiveMin)
            {
                errors.Add(effectiveMin == TodayDate
                    ? "Expected date cannot be less than today."
                    : "Expected date cannot be before the order date.");
            }

            return errors;
        }

        private List<string> ValidateReceivedDate(long brickOrderId, DateTime receivedDate)
        {
            var errors = new List<string>();
            var orderLine = AllOrders.FirstOrDefault(order => order.Id == brickOrderId);

            if (orderLine != null && receivedDate.Date < orderLine.OrderedDate.Date)
            {
                errors.Add("Received date cannot be less than the order date.");
            }

            return errors;
        }

        public string FormatPrice(decimal price) => price.ToString("C2", EnGbCulture);

        /// <summary>Returns the brick for the given id in O(1), or null if not found.</summary>
        public IBrick? GetBrick(long brickId) =>
            _brickById.TryGetValue(brickId, out var brick) ? brick : null;
    }
}
