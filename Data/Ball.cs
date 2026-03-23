namespace Data
{
    public class Ball
    {
        public double posX { get; set; }
        public double posY { get; set; }
        public double speedX { get; set; }
        public double speedY { get; set; }
        public double radius { get; set; }
        public double weight { get; set; }
        public Ball(double posX, double posY, double speedX, double speedY)
        {
            this.posX = posX;
            this.posY = posY;
            this.speedX = speedX;
            this.speedY = speedY;
        }
    }
}
