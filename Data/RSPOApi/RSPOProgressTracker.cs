namespace mapa_back.Data.RSPOApi
{
    public class RSPOProgressTracker
    {
        public int CurrentPage { get; set; } = 0;
        public int MaxPage { get; set; } = 0;
        public bool IsSyncInProgress { get; set; } = false;
        public List<int> InvalidRspoNumbers { get; set; } = new();
        public List<string> Exceptions { get; set; } = new();
    }
}
