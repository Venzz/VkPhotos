using System;
using System.Collections.Generic;
using System.Linq;

namespace VkPhotos.Model.Map
{
    public class TileMatrix<T>
    {
        private UInt16 Width;
        private UInt16 Height;
        private GeoTileWithObjects<T>[,] Matrix;

        /// <param name="width">Final = width * 2 + 1</param>
        /// <param name="height">Final = height * 2 + 1</param>
        public TileMatrix(UInt16 width, UInt16 height, Double zoomLevel, GeoPoint center)
        {
            Apply(width, height, zoomLevel, center, recreate: false);
        }

        public Changes Apply(UInt16 width, UInt16 height, Double zoomLevel, GeoPoint center, Boolean recreate)
        {
            var previousTiles = new List<GeoTileWithObjects<T>>(GetTiles());
            var tileWidth = 360.0 / (1 << (Byte)zoomLevel);
            var tileHeight = 180.0 / (1 << (Byte)zoomLevel);
            Width = width;
            Height = height;

            Matrix = new GeoTileWithObjects<T>[Height * 2 + 1, Width * 2 + 1];
            Matrix[Height, Width] = new GeoTileWithObjects<T>(center, (Byte)zoomLevel);
            var centerPoint = Matrix[Height, Width].Tile.Point;
            for (var i = -Width; i <= Width; i++)
            {
                for (var j = -Height; j <= Height; j++)
                {
                    if ((i == 0) && (j == 0))
                        continue;
                    Matrix[j + Height, i + Width] = new GeoTileWithObjects<T>(new GeoTile(new GeoPoint(centerPoint.Latitude + tileHeight * j, centerPoint.Longitude + tileWidth * i), (Byte)zoomLevel));
                }
            }

            var newTiles = new List<GeoTileWithObjects<T>>(GetTiles());
            var notChangedTiles = recreate ? new List<GeoTileWithObjects<T>>() : previousTiles.Intersect(newTiles).ToList();
            foreach (var notChangedTile in notChangedTiles)
            {
                var previousTile = previousTiles[previousTiles.IndexOf(notChangedTile)];
                var newTile = newTiles[newTiles.IndexOf(notChangedTile)];
                foreach (var obj in previousTile.Objects)
                    newTile.Objects.Add(obj);
            }
            var removedTiles = previousTiles.Except(notChangedTiles).ToList();
            var addedTiles = GetTiles().Except(notChangedTiles).ToList();
            return new Changes(removedTiles, addedTiles, notChangedTiles);
        }

        public IEnumerable<Object> GetObjects()
        {
            if (Matrix != null)
                for (var i = -Width; i <= Width; i++)
                    for (var j = -Height; j <= Height; j++)
                        if (Matrix[j + Height, i + Width] != null)
                            foreach (var obj in Matrix[j + Height, i + Width].Objects)
                                yield return obj;
        }

        public IEnumerable<GeoTileWithObjects<T>> GetTiles()
        {
            if (Matrix != null)
                for (var i = -Width; i <= Width; i++)
                    for (var j = -Height; j <= Height; j++)
                        if (Matrix[j + Height, i + Width] != null)
                            yield return Matrix[j + Height, i + Width];
        }

        public class Changes
        {
            public IEnumerable<GeoTileWithObjects<T>> Removed { get; }
            public IEnumerable<GeoTileWithObjects<T>> Added { get; }
            public IEnumerable<GeoTileWithObjects<T>> NotChanged { get; }

            public Changes(IEnumerable<GeoTileWithObjects<T>> removed, IEnumerable<GeoTileWithObjects<T>> added, IEnumerable<GeoTileWithObjects<T>> notChanged)
            {
                Removed = removed;
                Added = added;
                NotChanged = notChanged;
            }
        }
    }
}
