using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoCGAL.Utilities
{
    public class RandomColor
    {
        
        public static Color GetRandomColor()
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());

            List<Color> colors = new List<Color>();
            colors.Add(Color.Red);
            colors.Add(Color.Orange);
            colors.Add(Color.Yellow);
            colors.Add(Color.Green);
            colors.Add(Color.Cyan);
            colors.Add(Color.Blue);
            colors.Add(Color.Purple);
            colors.Add(Color.Pink);
            colors.Add(Color.Brown);
            colors.Add(Color.Navy);
            colors.Add(Color.GreenYellow);

            return colors[random.Next(colors.Count)];
        }
    }
}
