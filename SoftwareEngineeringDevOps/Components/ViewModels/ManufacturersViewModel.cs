using SoftwareEngineeringDevOps.App.Auth;
using SoftwareEngineeringDevOps.App.Bricks;
using SoftwareEngineeringDevOps.App.BrickOrders;
using SoftwareEngineeringDevOps.App.Manufacturers;
using SoftwareEngineeringDevOps.App.Validation;

namespace SoftwareEngineeringDevOps.Components.ViewModels
{
    public class ManufacturersViewModel
    {
        private readonly IManufacturersMediator _manufacturersMediator;
        private readonly IBricksMediator _bricksMediator;
        private readonly IBrickOrdersMediator _ordersMediator;
        private readonly IAuthService _authService;

        public ManufacturersViewModel(
            IManufacturersMediator manufacturersMediator,
            IBricksMediator bricksMediator,
            IBrickOrdersMediator ordersMediator,
            IAuthService authService)
        {
            _manufacturersMediator = manufacturersMediator;
            _bricksMediator = bricksMediator;
            _ordersMediator = ordersMediator;
            _authService = authService;
        }

        public IEnumerable<IManufacturer> Manufacturers { get; set; } = Enumerable.Empty<IManufacturer>();
        public IManufacturer? SelectedManufacturer { get; set; }
        public IEnumerable<IBrick> SelectedManufacturerBricks { get; set; } = Enumerable.Empty<IBrick>();
        public NewManufacturer NewManufacturerModel { get; set; } = new();
        public EditManufacturer? EditManufacturerModel { get; set; }
        public bool IsLoading { get; set; }
        public bool ShowAddModal { get; set; }
        public bool ShowEditModal { get; set; }
        public bool ShowDeleteConfirm { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public List<string> DeletionBlockedBricks { get; set; } = new();
        public List<string> ChildBrickNames { get; set; } = new();
        public string ManufacturersSearchTerm { get; set; } = string.Empty;

        public UserRole CurrentUserRole => _authService.CurrentUser != null
            ? RoleHelper.GetRole(_authService.CurrentUser)
            : UserRole.Standard;

        public IEnumerable<IManufacturer> FilteredManufacturers =>
            Manufacturers.Where(manufacturer =>
                string.IsNullOrWhiteSpace(ManufacturersSearchTerm)
                || manufacturer.Name.Contains(ManufacturersSearchTerm, StringComparison.OrdinalIgnoreCase)
                || manufacturer.Email.Contains(ManufacturersSearchTerm, StringComparison.OrdinalIgnoreCase)
                || manufacturer.PhoneNo.Contains(ManufacturersSearchTerm, StringComparison.OrdinalIgnoreCase));

        public async Task LoadManufacturers()
        {
            IsLoading = true;
            Manufacturers = await _manufacturersMediator.GetAllManufacturers();
            IsLoading = false;
        }

        public async Task SelectManufacturer(IManufacturer manufacturer)
        {
            SelectedManufacturer = manufacturer;
            SelectedManufacturerBricks = await _bricksMediator.GetBricksByManufacturerId(manufacturer.Id);
        }

        public void OpenAddModal()
        {
            NewManufacturerModel = new NewManufacturer();
            ValidationErrors.Clear();
            ShowAddModal = true;
        }

        public void CloseAddModal()
        {
            ShowAddModal = false;
            ValidationErrors.Clear();
        }

        public async Task<bool> AddManufacturer()
        {
            ValidationErrors.Clear();
            var errors = InputValidator.ValidateManufacturer(NewManufacturerModel);
            if (errors.Count > 0)
            {
                ValidationErrors = errors;
                return false;
            }

            IsLoading = true;
            await _manufacturersMediator.Insert(NewManufacturerModel);
            await LoadManufacturers();
            ShowAddModal = false;
            IsLoading = false;
            return true;
        }

        public void OpenEditModal(IManufacturer manufacturer)
        {
            EditManufacturerModel = new EditManufacturer(manufacturer);
            ValidationErrors.Clear();
            ShowEditModal = true;
        }

        public void CloseEditModal()
        {
            ShowEditModal = false;
            ValidationErrors.Clear();
        }

        public async Task<bool> UpdateManufacturer()
        {
            if (EditManufacturerModel == null) return false;

            ValidationErrors.Clear();
            var errors = InputValidator.ValidateManufacturer(EditManufacturerModel);
            if (errors.Count > 0)
            {
                ValidationErrors = errors;
                return false;
            }

            IsLoading = true;
            await _manufacturersMediator.Update(EditManufacturerModel);
            await LoadManufacturers();
            ShowEditModal = false;

            if (SelectedManufacturer?.Id == EditManufacturerModel.Id)
            {
                SelectedManufacturer = await _manufacturersMediator.GetManufacturerById(EditManufacturerModel.Id);
            }
            IsLoading = false;
            return true;
        }

        public async Task OpenDeleteConfirm(IManufacturer manufacturer)
        {
            SelectedManufacturer = manufacturer;
            ErrorMessage = null;
            DeletionBlockedBricks.Clear();
            ChildBrickNames.Clear();

            var bricks = await _bricksMediator.GetBricksByManufacturerId(manufacturer.Id);
            ChildBrickNames = bricks.Select(b => b.Name).ToList();

            // Check if any child brick has active orders
            foreach (var brick in bricks)
            {
                var orders = await _ordersMediator.GetBrickOrdersByBrickId(brick.Id);
                if (orders.Any(o => o.CancelledDate == null))
                {
                    DeletionBlockedBricks.Add(brick.Name);
                }
            }

            ShowDeleteConfirm = true;
        }

        public void CloseDeleteConfirm()
        {
            ShowDeleteConfirm = false;
            ErrorMessage = null;
        }

        public async Task<bool> DeleteManufacturer()
        {
            if (SelectedManufacturer == null) return false;

            if (DeletionBlockedBricks.Count > 0)
            {
                ErrorMessage = "Cannot delete: some child bricks have active orders.";
                return false;
            }

            IsLoading = true;

            // Delete child bricks first
            var bricks = await _bricksMediator.GetBricksByManufacturerId(SelectedManufacturer.Id);
            foreach (var brick in bricks)
            {
                await _bricksMediator.Delete(brick.Id);
            }

            await _manufacturersMediator.Delete(SelectedManufacturer.Id);
            SelectedManufacturer = null;
            SelectedManufacturerBricks = Enumerable.Empty<IBrick>();
            await LoadManufacturers();
            ShowDeleteConfirm = false;
            IsLoading = false;
            return true;
        }
    }
}
