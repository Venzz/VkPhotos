using System;

namespace VkPhotos.Model.Map
{
    public interface IMapObjectsFilter
    {
        Boolean IsAccepted(GeoObject geoObject);
    }
}
