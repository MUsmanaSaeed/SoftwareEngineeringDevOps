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
        public string? AddBrickNameValidationMessage { get; private set; }
        public string? EditBrickNameValidationMessage { get; private set; }
        public string? AddNumericValidationMessage { get; private set; }
        public string? EditNumericValidationMessage { get; private set; }
        public bool HasAddBrickNameConflict => !string.IsNullOrWhiteSpace(AddBrickNameValidationMessage);
        public bool HasEditBrickNameConflict => !string.IsNullOrWhiteSpace(EditBrickNameValidationMessage);
        public bool HasAddNumericInputError => !string.IsNullOrWhiteSpace(AddNumericValidationMessage);
        public bool HasEditNumericInputError => !string.IsNullOrWhiteSpace(EditNumericValidationMessage);
        public bool CanSubmitAddBrick => !HasAddBrickNameConflict && !HasAddNumericInputError;
        public bool CanSubmitEditBrick => !HasEditBrickNameConflict && !HasEditNumericInputError;
        public decimal MaxDimensionLimit => 10000m;

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
                || FormatPrice(brick.Price).Contains(BricksSearchTerm, StringComparison.OrdinalIgnoreCase))
            .OrderBy(brick => brick.Name, StringComparer.OrdinalIgnoreCase);

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
            AddBrickNameValidationMessage = null;
            AddNumericValidationMessage = null;
            ValidationErrors.Clear();
            ShowAddModal = true;
        }

        public void CloseAddModal()
        {
            ShowAddModal = false;
            AddBrickNameValidationMessage = null;
            AddNumericValidationMessage = null;
            ValidationErrors.Clear();
        }

        public async Task<bool> AddBrick()
        {
            NormalizeNewBrickModel();
            ValidateAddNumericInputs();
            ValidationErrors.Clear();
            var errors = InputValidator.ValidateBrick(NewBrickModel);
            if (errors.Count > 0)
            {
                ValidationErrors = errors;
                return false;
            }

            if (HasAddNumericInputError)
            {
                ValidationErrors.Add(AddNumericValidationMessage!);
                return false;
            }

            if (await BrickNameExists(NewBrickModel.Name))
            {
                AddBrickNameValidationMessage = "Name already exists. Please use a different brick name.";
                ValidationErrors.Add(AddBrickNameValidationMessage);
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
            EditBrickNameValidationMessage = null;
            EditNumericValidationMessage = null;
            ValidationErrors.Clear();
            ShowEditModal = true;
        }

        public void CloseEditModal()
        {
            ShowEditModal = false;
            EditBrickNameValidationMessage = null;
            EditNumericValidationMessage = null;
            ValidationErrors.Clear();
        }

        public async Task<bool> UpdateBrick()
        {
            if (EditBrickModel == null) return false;

            NormalizeEditBrickModel();
            ValidateEditNumericInputs();
            ValidationErrors.Clear();
            var errors = InputValidator.ValidateBrick(EditBrickModel);
            if (errors.Count > 0)
            {
                ValidationErrors = errors;
                return false;
            }

            if (HasEditNumericInputError)
            {
                ValidationErrors.Add(EditNumericValidationMessage!);
                return false;
            }

            if (await BrickNameExists(EditBrickModel.Name, EditBrickModel.Id))
            {
                EditBrickNameValidationMessage = "Name already exists. Please use a different brick name.";
                ValidationErrors.Add(EditBrickNameValidationMessage);
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

        public string FormatVoidsPercent(decimal voids) => $"{(voids * 100m):0.##}%";

        public string VoidsPercentInputValue(decimal voids) => (voids * 100m).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

        public static decimal ParseVoidsPercentInput(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0m;
            if (!decimal.TryParse(value, out var percent)) return 0m;

            var normalizedPercent = Math.Clamp(percent, 0m, 100m);
            return normalizedPercent / 100m;
        }

        public void ValidateAddBrickName()
        {
            AddBrickNameValidationMessage = BrickNameExistsInLoadedList(NewBrickModel.Name)
                ? "Name already exists. Please use a different brick name."
                : null;
        }

        public void ValidateAddNumericInputs()
        {
            AddNumericValidationMessage = BuildNumericInputValidationMessage(
                NewBrickModel.Price,
                NewBrickModel.Strength,
                NewBrickModel.Width,
                NewBrickModel.Height,
                NewBrickModel.Depth);
        }

        public void ValidateEditBrickName()
        {
            if (EditBrickModel == null)
            {
                EditBrickNameValidationMessage = null;
                return;
            }

            EditBrickNameValidationMessage = BrickNameExistsInLoadedList(EditBrickModel.Name, EditBrickModel.Id)
                ? "Name already exists. Please use a different brick name."
                : null;
        }

        public void ValidateEditNumericInputs()
        {
            if (EditBrickModel == null)
            {
                EditNumericValidationMessage = null;
                return;
            }

            EditNumericValidationMessage = BuildNumericInputValidationMessage(
                EditBrickModel.Price,
                EditBrickModel.Strength,
                EditBrickModel.Width,
                EditBrickModel.Height,
                EditBrickModel.Depth);
        }

        static void NormalizeNewBrickModel(NewBrick model)
        {
            model.Name = (model.Name ?? string.Empty).ToTitleCase();
            model.Colour = (model.Colour ?? string.Empty).ToTitleCase();
            model.Material = (model.Material ?? string.Empty).ToTitleCase();
            model.Type = (model.Type ?? string.Empty).ToTitleCase();
            model.Price = Math.Max(model.Price, 0m);
            model.Strength = Math.Max(model.Strength, 0m);
            model.Width = Math.Clamp(model.Width, 0m, 10000m);
            model.Height = Math.Clamp(model.Height, 0m, 10000m);
            model.Depth = Math.Clamp(model.Depth, 0m, 10000m);
            model.Voids = Math.Clamp(model.Voids, 0m, 1m);
        }

        void NormalizeNewBrickModel()
        {
            NormalizeNewBrickModel(NewBrickModel);
        }

        static void NormalizeEditBrickModel(EditBrick model)
        {
            model.Name = (model.Name ?? string.Empty).ToTitleCase();
            model.Colour = (model.Colour ?? string.Empty).ToTitleCase();
            model.Material = (model.Material ?? string.Empty).ToTitleCase();
            model.Type = (model.Type ?? string.Empty).ToTitleCase();
            model.Price = Math.Max(model.Price, 0m);
            model.Strength = Math.Max(model.Strength, 0m);
            model.Width = Math.Clamp(model.Width, 0m, 10000m);
            model.Height = Math.Clamp(model.Height, 0m, 10000m);
            model.Depth = Math.Clamp(model.Depth, 0m, 10000m);
            model.Voids = Math.Clamp(model.Voids, 0m, 1m);
        }

        void NormalizeEditBrickModel()
        {
            if (EditBrickModel == null) return;
            NormalizeEditBrickModel(EditBrickModel);
        }

        static string? BuildNumericInputValidationMessage(decimal price, decimal strength, decimal width, decimal height, decimal depth)
        {
            if (price < 0) return "Price cannot be negative.";
            if (strength < 0) return "Strength cannot be negative.";
            if (width < 0) return "Width cannot be negative.";
            if (height < 0) return "Height cannot be negative.";
            if (depth < 0) return "Depth cannot be negative.";
            if (width > 10000m || height > 10000m || depth > 10000m) return "Dimensions must be 10,000 mm or less.";
            return null;
        }

        public static decimal ParseDecimalInput(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0m;
            return decimal.TryParse(value, out var parsedValue) ? parsedValue : 0m;
        }

        bool BrickNameExistsInLoadedList(string? name, long? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;

            var normalizedName = name.Trim();
            return Bricks.Any(brick =>
                brick.Id != excludeId
                && string.Equals(brick.Name.Trim(), normalizedName, StringComparison.OrdinalIgnoreCase));
        }

        async Task<bool> BrickNameExists(string? name, long? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;

            var normalizedName = name.Trim();
            var bricks = await _bricksMediator.GetAllBricks();

            return bricks.Any(brick =>
                brick.Id != excludeId
                && string.Equals(brick.Name.Trim(), normalizedName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
