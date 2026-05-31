namespace AlpacaSpy
{
    public class PerBrowserState
    {
        private bool _isAuthenticated;
        private bool _mustChangePassword;

        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            set { _isAuthenticated = value; AuthStateChanged?.Invoke(); }
        }

        public bool MustChangePassword
        {
            get => _mustChangePassword;
            set { _mustChangePassword = value; AuthStateChanged?.Invoke(); }
        }

        public event Action? AuthStateChanged;
    }
}
