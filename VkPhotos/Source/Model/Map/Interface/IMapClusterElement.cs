using System.Collections.Generic;

namespace VkPhotos.Model.Map
{
    public interface IMapClusterElement
    {
        void SetContext(IReadOnlyCollection<GeoObject> context);
        void Show();
        void Hide();
    }
}
