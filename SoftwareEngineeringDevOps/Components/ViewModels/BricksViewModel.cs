using SoftwareEngineeringDevOps.App.Auth;
using SoftwareEngineeringDevOps.App.Bricks;
using SoftwareEngineeringDevOps.App.BrickOrders;
using SoftwareEngineeringDevOps.App.Manufacturers;
using SoftwareEngineeringDevOps.App.Validation;

namespace SoftwareEngineeringDevOps.Components.ViewModels
{
    public class BricksViewModel
    {
        private readonly IBricksMediator _bricksMediator;
        private readonly IManufacturersMediator _manufacturersMediator;
        private readonly IBrickOrdersMediator _ordersMediator;
        private readonly IAuthService _authService;

        public BricksViewModel(
            IBricksMediator bricksMediator,
            IManufacturersMediator manufacturersMediator,
            IBrickOrdersMediator ordersMediator,
            IAuthService authService)
        {
            _bricksMediator = bricksMediator;
            _manufacturersMediator = manufacturersMediator;
            _ordersMediator = ordersMediator;
            _authService = authService;
        }

        public IEnumerable<IBrick> Bricks { get; set; } = Enumerable.Empty<IBrick>();
        public IEnumerable<IManufacturer> Manufacturers { get; set; } = Enumerable.Empty<IManufacturer>();
        public IBrick? SelectedBrick { get; set; }
        public IEnumerable<IBrickOrder> SelectedBrickOrders { get; set; } = Enumerable.Empty<IBrickOrder>();
        public NewBrick NewBrickModel { get; set; } = new();
        public EditBrick? EditBrickModel { get; set; }
        public bool IsLoading { get; set; }
        public bool ShowAddModal { get; set; }
        public bool ShowEditModal { get; set; }
        public bool ShowDeleteConfirm { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string ManufacturerSearch { get; set; } = string.Empty;
        public string BricksSearchTerm { get; set; } = string.Empty;
        public string BrickOrdersSearchTerm { get; set; } = string.Empty;

        public UserRole CurrentUserRole => _authService.CurrentUser != null
            ? RoleHelper.GetRole(_authService.CurrentUser)
            : UserRole.Standard;

        public IEnumerable<IManufacturer> FilteredManufacturers =>
            string.IsNullOrWhiteSpace(ManufacturerSearch)
                ? Manufacturers
                : Manufacturers.Where(m => m.Name.Contains(ManufacturerSearch, StringComparison.OrdinalIgnoreCase));

        public IEnumerable<IBrick> FilteredBricks =>
            Bricks.Where(brick =>
                string.IsNullOrWhiteSpace(BricksSearchTerm)
                || brick.Name.Contains(BricksSearchTerm, StringComparison.OrdinalIgnoreCase)
                || brick.Manufacturer.Name.Contains(BricksSearchTerm, StringComparison.OrdinalIgnoreCase)
                || FormatPrice(brick.Price).Contains(BricksSearchTerm, StringComparison.OrdinalIgnoreCase));

        public IEnumerable<IBrickOrder> FilteredSelectedBrickOrders =>
            SelectedBrickOrders.Where(order =>
                string.IsNullOrWhiteSpace(BrickOrdersSearchTerm)
                || order.OrderNo.Contains(BrickOrdersSearchTerm, StringComparison.OrdinalIgnoreCase)
                || order.BricksOrdered.ToString().Contains(BrickOrdersSearchTerm, StringComparison.OrdinalIgnoreCase)
                || order.OrderedDate.ToString("dd/MM/yyyy").Contains(BrickOrdersSearchTerm, StringComparison.OrdinalIgnoreCase));

        public async Task LoadBricks()
        {
            IsLoading = true;
            Bricks = await _bricksMediator.GetAllBricks();
            Manufacturers = await _manufacturersMediator.GetAllManufacturers();
            IsLoading = false;
        }

        public async Task SelectBrick(IBrick brick)
        {
            SelectedBrick = brick;
            SelectedBrickOrders = await _ordersMediator.GetBrickOrdersByBrickId(brick.Id);
        }

        public async Task SelectBrickById(long id)
        {
            var brick = await _bricksMediator.GetBrickById(id);
            if (brick != null)
            {
                await SelectBrick(brick);
            }
        }

        public void OpenAddModal()
        {
            NewBrickModel = new NewBrick();
            ManufacturerSearch = string.Empty;
            ValidationErrors.Clear();
            ShowAddModal = true;
        }

        public void CloseAddModal()
        {
            ShowAddModal = false;
            ValidationErrors.Clear();
        }

        public async Task<bool> AddBrick()
        {
            ValidationErrors.Clear();
            var errors = InputValidator.ValidateBrick(NewBrickModel);
            if (errors.Count > 0)
            {
                ValidationErrors = errors;
                return false;
            }

            IsLoading = true;
            await _bricksMediator.Insert(NewBrickModel);
            await LoadBricks();
            ShowAddModal = false;
            IsLoading = false;
            return true;
        }

        public void OpenEditModal(IBrick brick)
        {
            EditBrickModel = new EditBrick(brick);
            ManufacturerSearch = string.Empty;
            ValidationErrors.Clear();
            ShowEditModal = true;
        }

        public void CloseEditModal()
        {
            ShowEditModal = false;
            ValidationErrors.Clear();
        }

        public async Task<bool> UpdateBrick()
        {
            if (EditBrickModel == null) return false;

            ValidationErrors.Clear();
            var errors = InputValidator.ValidateBrick(EditBrickModel);
            if (errors.Count > 0)
            {
                ValidationErrors = errors;
                return false;
            }

            IsLoading = true;
            await _bricksMediator.Update(EditBrickModel);
            await LoadBricks();
            ShowEditModal = false;

            if (SelectedBrick?.Id == EditBrickModel.Id)
            {
                SelectedBrick = await _bricksMediator.GetBrickById(EditBrickModel.Id);
            }
            IsLoading = false;
            return true;
        }

        public async Task OpenDeleteConfirm(IBrick brick)
        {
            SelectedBrick = brick;
            ErrorMessage = null;

            var orders = await _ordersMediator.GetBrickOrdersByBrickId(brick.Id);
            if (orders.Any())
            {
                ErrorMessage = "Cannot delete: this brick has existing order records.";
            }

            ShowDeleteConfirm = true;
        }

        public void CloseDeleteConfirm()
        {
            ShowDeleteConfirm = false;
            ErrorMessage = null;
        }

        public async Task<bool> DeleteBrick()
        {
            if (SelectedBrick == null) return false;

            var orders = await _ordersMediator.GetBrickOrdersByBrickId(SelectedBrick.Id);
            if (orders.Any())
            {
                ErrorMessage = "Cannot delete: this brick has existing order records.";
                return false;
            }

            IsLoading = true;
            await _bricksMediator.Delete(SelectedBrick.Id);
            SelectedBrick = null;
            SelectedBrickOrders = Enumerable.Empty<IBrickOrder>();
            await LoadBricks();
            ShowDeleteConfirm = false;
            IsLoading = false;
            return true;
        }

        public string FormatPrice(decimal price) => price.ToString("C2", System.Globalization.CultureInfo.GetCultureInfo("en-GB"));
    }
}
