namespace AlpacaSpy
{
    public class SessionState
    {
        public event Action? OnChange;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public void SetSize(int width, int height)
        {
            Width = width;
            Height = height;
            OnChange?.Invoke();
        }

    }
}
