namespace SoftwareEngineeringDevOps.Components.Shared
{
    public enum ToastSeverity
    {
        Success,
        Warning,
        Error
    }

    public record ToastMessage(Guid Id, string Message, ToastSeverity Severity);

    public interface IToastService
    {
        IReadOnlyList<ToastMessage> Toasts { get; }
        event Action? OnChange;
        void Show(string message, ToastSeverity severity = ToastSeverity.Success);
        void Dismiss(Guid id);
    }

    public sealed class ToastService : IToastService
    {
        private readonly List<ToastMessage> _toasts = new();
        private readonly TimeProvider _timeProvider;

        public ToastService() : this(TimeProvider.System) { }

        internal ToastService(TimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
        }

        public IReadOnlyList<ToastMessage> Toasts => _toasts;
        public event Action? OnChange;

        public void Show(string message, ToastSeverity severity = ToastSeverity.Success)
        {
            var toast = new ToastMessage(Guid.NewGuid(), message, severity);
            _toasts.Add(toast);
            OnChange?.Invoke();

            _ = AutoDismissAsync(toast.Id);
        }

        public void Dismiss(Guid id)
        {
            var toast = _toasts.FirstOrDefault(t => t.Id == id);
            if (toast is null) return;
            _toasts.Remove(toast);
            OnChange?.Invoke();
        }

        private async Task AutoDismissAsync(Guid id)
        {
            await Task.Delay(4000);
            Dismiss(id);
        }
    }
}
