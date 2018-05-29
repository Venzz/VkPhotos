using System;
using Windows.Foundation;

namespace VkPhotos.Model.Map
{
    public interface IMapObjectElement
    {
        UInt32 Id { get; }
        Point NormalizedAnchorPoint { get; }

        Object GetContext();
        void SetContext(Object context);
        void Show();
        void Hide();
    }
}
