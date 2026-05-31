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
        public string ManufacturerBricksSearchTerm { get; set; } = string.Empty;
        public string? AddNameValidationMessage { get; private set; }
        public string? EditNameValidationMessage { get; private set; }
        public bool HasAddNameConflict => !string.IsNullOrWhiteSpace(AddNameValidationMessage);
        public bool HasEditNameConflict => !string.IsNullOrWhiteSpace(EditNameValidationMessage);

        public UserRole CurrentUserRole => _authService.CurrentUser != null
            ? RoleHelper.GetRole(_authService.CurrentUser)
            : UserRole.Standard;

        public IEnumerable<IManufacturer> FilteredManufacturers =>
            Manufacturers.Where(manufacturer =>
                string.IsNullOrWhiteSpace(ManufacturersSearchTerm)
                || manufacturer.Name.Contains(ManufacturersSearchTerm, StringComparison.OrdinalIgnoreCase)
                || manufacturer.Email.Contains(ManufacturersSearchTerm, StringComparison.OrdinalIgnoreCase)
                || manufacturer.PhoneNo.Contains(ManufacturersSearchTerm, StringComparison.OrdinalIgnoreCase))
            .OrderBy(manufacturer => manufacturer.Name, StringComparer.OrdinalIgnoreCase);

        public IEnumerable<IBrick> FilteredSelectedManufacturerBricks =>
            SelectedManufacturerBricks.Where(brick =>
                string.IsNullOrWhiteSpace(ManufacturerBricksSearchTerm)
                || brick.Name.Contains(ManufacturerBricksSearchTerm, StringComparison.OrdinalIgnoreCase))
            .OrderBy(brick => brick.Name, StringComparer.OrdinalIgnoreCase);

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
            AddNameValidationMessage = null;
            ValidationErrors.Clear();
            ShowAddModal = true;
        }

        public void CloseAddModal()
        {
            ShowAddModal = false;
            AddNameValidationMessage = null;
            ValidationErrors.Clear();
        }

        public async Task<bool> AddManufacturer()
        {
            NormalizeNewManufacturerModel();
            ValidationErrors.Clear();
            var errors = InputValidator.ValidateManufacturer(NewManufacturerModel);
            if (errors.Count > 0)
            {
                ValidationErrors = errors;
                return false;
            }

            if (await ManufacturerNameExists(NewManufacturerModel.Name))
            {
                AddNameValidationMessage = "Name already exists. Please use a different manufacturer name.";
                ValidationErrors.Add(AddNameValidationMessage);
                return false;
            }

            IsLoading = true;
            var createdManufacturer = await _manufacturersMediator.Insert(NewManufacturerModel);
            await LoadManufacturers();
            await SelectManufacturer(createdManufacturer);
            ShowAddModal = false;
            IsLoading = false;
            return true;
        }

        public void OpenEditModal(IManufacturer manufacturer)
        {
            EditManufacturerModel = new EditManufacturer(manufacturer);
            EditNameValidationMessage = null;
            ValidationErrors.Clear();
            ShowEditModal = true;
        }

        public void CloseEditModal()
        {
            ShowEditModal = false;
            EditNameValidationMessage = null;
            ValidationErrors.Clear();
        }

        public async Task<bool> UpdateManufacturer()
        {
            if (EditManufacturerModel == null) return false;

            NormalizeEditManufacturerModel();
            ValidationErrors.Clear();
            var errors = InputValidator.ValidateManufacturer(EditManufacturerModel);
            if (errors.Count > 0)
            {
                ValidationErrors = errors;
                return false;
            }

            if (await ManufacturerNameExists(EditManufacturerModel.Name, EditManufacturerModel.Id))
            {
                EditNameValidationMessage = "Name already exists. Please use a different manufacturer name.";
                ValidationErrors.Add(EditNameValidationMessage);
                return false;
            }

            IsLoading = true;
            var updatedManufacturer = await _manufacturersMediator.Update(EditManufacturerModel);
            await LoadManufacturers();
            await SelectManufacturer(updatedManufacturer);
            ShowEditModal = false;
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

        static void NormalizeNewManufacturerModel(NewManufacturer model)
        {
            model.Name = (model.Name ?? string.Empty).ToTitleCase();
            model.Address1 = (model.Address1 ?? string.Empty).ToTitleCase();
            model.Address2 = string.IsNullOrWhiteSpace(model.Address2) ? string.Empty : model.Address2.ToTitleCase();
            model.Postcode = (model.Postcode ?? string.Empty).ToUpperInvariant();
            model.PhoneNo = SanitizePhone(model.PhoneNo);
            model.Email = (model.Email ?? string.Empty).Trim();
        }

        void NormalizeNewManufacturerModel()
        {
            NormalizeNewManufacturerModel(NewManufacturerModel);
        }

        static void NormalizeEditManufacturerModel(EditManufacturer model)
        {
            model.Name = (model.Name ?? string.Empty).ToTitleCase();
            model.Address1 = (model.Address1 ?? string.Empty).ToTitleCase();
            model.Address2 = string.IsNullOrWhiteSpace(model.Address2) ? string.Empty : model.Address2.ToTitleCase();
            model.Postcode = (model.Postcode ?? string.Empty).ToUpperInvariant();
            model.PhoneNo = SanitizePhone(model.PhoneNo);
            model.Email = (model.Email ?? string.Empty).Trim();
        }

        void NormalizeEditManufacturerModel()
        {
            if (EditManufacturerModel == null) return;
            NormalizeEditManufacturerModel(EditManufacturerModel);
        }

        public static string SanitizePhone(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            var input = value.Trim();
            var chars = new List<char>(input.Length);
            var hasLeadingPlus = false;

            foreach (var character in input)
            {
                if (character == '+' && !hasLeadingPlus && chars.Count == 0)
                {
                    chars.Add(character);
                    hasLeadingPlus = true;
                    continue;
                }

                if (char.IsDigit(character))
                {
                    chars.Add(character);
                }
            }

            return new string(chars.ToArray());
        }

        public void ValidateAddManufacturerName()
        {
            AddNameValidationMessage = ManufacturerNameExistsInLoadedList(NewManufacturerModel.Name)
                ? "Name already exists. Please use a different manufacturer name."
                : null;
        }

        public void ValidateEditManufacturerName()
        {
            if (EditManufacturerModel == null)
            {
                EditNameValidationMessage = null;
                return;
            }

            EditNameValidationMessage = ManufacturerNameExistsInLoadedList(EditManufacturerModel.Name, EditManufacturerModel.Id)
                ? "Name already exists. Please use a different manufacturer name."
                : null;
        }

        bool ManufacturerNameExistsInLoadedList(string? name, long? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;

            var normalizedName = name.Trim();
            return Manufacturers.Any(manufacturer =>
                manufacturer.Id != excludeId
                && string.Equals(manufacturer.Name.Trim(), normalizedName, StringComparison.OrdinalIgnoreCase));
        }

        async Task<bool> ManufacturerNameExists(string? name, long? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;

            var normalizedName = name.Trim();
            var manufacturers = await _manufacturersMediator.GetAllManufacturers();

            return manufacturers.Any(manufacturer =>
                manufacturer.Id != excludeId
                && string.Equals(manufacturer.Name.Trim(), normalizedName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
