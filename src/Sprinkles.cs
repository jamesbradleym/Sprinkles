using Elements;
using Elements.Geometry;
using Elements.Geometry.Solids;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sprinkles
{
    public static class Sprinkles
    {
        public static SprinklesOutputs Execute(Dictionary<string, Model> inputModels, SprinklesInputs input)
        {
            var output = new SprinklesOutputs();

            // Set grid and section dimensions
            int gridWidth = 10;
            int gridHeight = 10;
            int sectionWidth = 10;
            int sectionHeight = 10;

            // Initialize random number generator
            Random random = new Random((int)input.Seed);

            // Create materials dictionary for each letter
            Dictionary<string, Material> mats = Enumerable.Range('A', 'G' - 'A' + 1)
                .ToDictionary(c => ((char)c).ToString(), c => new Material(((char)c).ToString(), random.NextColor()));

            // Generate space boundaries
            List<SpaceBoundary> spaces = new List<SpaceBoundary>();
            for (int i = 0; i < gridHeight; i++)
            {
                for (int j = 0; j < gridWidth; j++)
                {
                    if (random.NextDouble() < 0.5) // Randomly create a space boundary
                    {
                        int centerX = j * sectionWidth + sectionWidth / 2;
                        int centerY = i * sectionHeight + sectionHeight / 2;

                        Profile profile = Polygon.Rectangle(sectionWidth, sectionHeight);
                        string randomLetter = ((char)('A' + random.Next(7))).ToString();
                        SpaceBoundary sb = new SpaceBoundary()
                        {
                            Boundary = profile,
                            Area = profile.Area(),
                            ProgramGroup = "Letters",
                            Name = randomLetter,
                            Material = mats[randomLetter],
                            Height = 10,
                            Transform = new Transform().Moved(new Vector3(centerX, centerY, 0)),
                            Representation = new Representation(new[] { new Lamina(profile.Perimeter, false) })
                        };
                        spaces.Add(sb);
                    }
                }
            }
            output.Model.AddElements(spaces);

            // Group spaces by name
            var spaceGroups = spaces.GroupBy(s => s.Name);

            // Generate SVG donut chart representing spaces
            var svg = new SVG();
            double radius = 50;
            double thickness = radius * 0.3;
            double startAngle = 0.0;

            // Add frame buffer
            svg.AddGeometry(Polygon.Rectangle(radius * 2 + 20, radius * 2 + 20), null, new SVG.Style { Fill = null, StrokeWidth = 0 });

            // Draw donut segments and add text
            int fontSize = 6;
            foreach (var group in spaceGroups)
            {
                double groupAngle = 360.0 * group.Count() / spaces.Count; // arc angle in degrees

                var arc = new Arc(new Vector3(0, 0), radius, startAngle, startAngle + groupAngle);
                svg.AddGeometry(arc.ToPolyline(50), null, new SVG.Style { Stroke = mats[group.Key].Color, Fill = null, StrokeWidth = thickness });
                svg.AddText(arc.Mid() - (0, 2), $"{group.Key}: {group.Count()}", null, "middle", new SVG.Style { Fill = Colors.White, Stroke = null, FontFamily = "Arial", FontSize = fontSize.ToString() });

                startAngle += groupAngle;
            }

            output.Model.AddElement(new SVGGraphic(svg.SvgString(), "Donut"));

            return output;
        }
    }
}
