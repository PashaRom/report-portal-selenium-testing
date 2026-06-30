namespace Core.Structures
{
    public readonly struct Timeouts
    {
        public static readonly TimeSpan Ms300 = TimeSpan.FromMilliseconds(300);
        public static readonly TimeSpan Ms500 = TimeSpan.FromMilliseconds(500);
        public static readonly TimeSpan Sec1  = TimeSpan.FromSeconds(1);
        public static readonly TimeSpan Sec2  = TimeSpan.FromSeconds(2);
        public static readonly TimeSpan Sec3  = TimeSpan.FromSeconds(3);
        public static readonly TimeSpan Sec5  = TimeSpan.FromSeconds(5);
        public static readonly TimeSpan Sec10 = TimeSpan.FromSeconds(10);
        public static readonly TimeSpan Sec20 = TimeSpan.FromSeconds(20);
        public static readonly TimeSpan Sec30 = TimeSpan.FromSeconds(30);
    }
}
