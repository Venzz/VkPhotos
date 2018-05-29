using System;
using System.Collections.Generic;

namespace VkPhotos.Model.Map.Clusterization
{
    public class Node
    {
        private Node[,] Matrix;

        public GeoTile Tile { get; private set; }
        public Node Parent { get; private set; }
        public List<GeoObject> GeoObjects { get; private set; }



        public Node(GeoTile tile)
        {
            GeoObjects = new List<GeoObject>();
            Tile = tile;
        }

        public void AddObject(GeoObject geoObject)
        {
            if (Matrix == null)
            {
                GeoObjects.Add(geoObject);
                return;
            }

            var x = (Int32)((geoObject.Location.Longitude - Tile.Point.Longitude) / (Tile.Width / 2));
            var y = (Int32)((Tile.Point.Latitude - geoObject.Location.Latitude) / (Tile.Height / 2));
            Matrix[x, y].AddObject(geoObject);
        }

        public Node GetChildNode(Int32 x, Int32 y)
        {
            if ((x < 0) || (x >= 2) || (y < 0) || (y >= 2))
                throw new ArgumentOutOfRangeException();

            if (Matrix == null)
                SpreadObjects();
            return Matrix[x, y];
        }

        private void SpreadObjects()
        {
            if (Matrix != null)
                return;

            var width = Tile.Width / 2;
            var height = Tile.Height / 2;

            Matrix = new Node[2, 2];
            for (var x = 0; x < 2; x++)
            {
                for (var y = 0; y < 2; y++)
                {
                    var geoTile = new GeoTile(new GeoPoint(Tile.Point.Latitude - y * height, Tile.Point.Longitude + x * width), width, height);
                    Matrix[x, y] = new Node(geoTile);
                }
            }

            foreach (var geoObject in GeoObjects)
            {
                var x = (Int32)((geoObject.Location.Longitude - Tile.Point.Longitude) / width);
                var y = (Int32)((Tile.Point.Latitude - geoObject.Location.Latitude) / height);
                Matrix[x, y].AddObject(geoObject);
            }
            GeoObjects.Clear();
        }

        public void GatherObjects()
        {
            if (Matrix == null)
                return;

            for (var x = 0; x < 2; x++)
            {
                for (var y = 0; y < 2; y++)
                {
                    Matrix[x, y].GatherObjects();
                    GeoObjects.AddRange(Matrix[x, y].GeoObjects);
                }
            }
            Matrix = null;
        }

        public GeoPoint GetCenter()
        {
            if (GeoObjects.Count == 0)
            {
                return new GeoPoint(Tile.Point.Latitude - Tile.Height / 2, Tile.Point.Longitude + Tile.Width / 2);
            }
            else
            {
                var longitude = 0.0;
                var latitude = 0.0;
                foreach (var geoObject in GeoObjects)
                {
                    longitude += geoObject.Location.Longitude;
                    latitude += geoObject.Location.Latitude;
                }
                return new GeoPoint(latitude / GeoObjects.Count, longitude / GeoObjects.Count);
            }
        }
    }
}
