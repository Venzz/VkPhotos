using System;
using System.Collections.Generic;

namespace VkPhotos.Model.Map
{
    public class MapObjectsFilter
    {
        private IMapObjectsFilter Filter;



        public MapObjectsFilter() { }

        public void Set(IMapObjectsFilter filter) => Filter = filter;

        public Boolean IsAccepted(GeoObject geoObject) => (Filter == null) ? true : Filter.IsAccepted(geoObject);

        public IReadOnlyCollection<GeoObject> Apply(IList<GeoObject> geoObjects)
        {
            var filteredObjects = new List<GeoObject>();
            foreach (var geoObject in geoObjects)
                if (IsAccepted(geoObject))
                    filteredObjects.Add(geoObject);
            return filteredObjects;
        }
    }
}
