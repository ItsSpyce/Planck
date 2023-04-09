namespace Planck
{
    public interface IPlanckSplashscreen
    {
        string Source { get; set; }
        bool IsActive { get; }

        void Show();
        void Close();
    }
}
