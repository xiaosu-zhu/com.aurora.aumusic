namespace com.aurora.aumusic.shared.Albums
{
    public class AlbumProgressChangedEventArgs
    {
        public double TotalPercent;
        public double CurrentPercent;


        /// <summary>
        /// caonima
        /// </summary>
        /// <param name="current"></param>
        /// <param name="total"></param>
        public AlbumProgressChangedEventArgs(double current, double total)
        {
            this.CurrentPercent = current;
            this.TotalPercent = total;
        }
    }
}