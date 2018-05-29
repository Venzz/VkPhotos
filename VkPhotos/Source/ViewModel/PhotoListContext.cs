using System;
using System.Collections.Generic;
using System.Linq;
using Venz.Data;
using VkPhotos.Model;

namespace VkPhotos.ViewModel
{
    public class PhotoListContext: ObservableObject
    {
        public String Header { get; private set; }
        public IEnumerable<Group> Items { get; private set; }
        public Int32 PhotoSize => 140;



        public PhotoListContext() { }

        public void Initialize(IEnumerable<Object> objects)
        {
            var itemGroupsResult = CreateItemGroups(objects);
            Items = itemGroupsResult.Item1;
            Header = String.Format(Strings.Page_PhotoList_Header, itemGroupsResult.Item2);
            OnPropertyChanged(nameof(Header), nameof(Items));
        }

        public static Tuple<IEnumerable<Group>, UInt32> CreateItemGroups(IEnumerable<Object> photos)
        {
            var groups = new List<Group>();
            var totalPhotos = 0U;
            foreach (var photoObject in photos)
            {
                var photo = (Photo)photoObject;
                var groupHeader = photo.Date.ToString("MMMM yyyy");
                var existingGroup = groups.FirstOrDefault(a => a.Header == groupHeader);
                if (existingGroup != null)
                {
                    existingGroup.Add(photo);
                }
                else
                {
                    var group = new Group(photo.Date, groupHeader, photo);
                    var index = 0;
                    while ((index < groups.Count) && (group.Date < groups[index].Date))
                        index++;
                    groups.Insert(index, group);
                }
                totalPhotos++;
            }
            return new Tuple<IEnumerable<Group>, UInt32>(groups, totalPhotos);
        }

        public class Group: List<Photo>
        {
            public String Header { get; }
            public DateTime Date { get; }
            public Group(DateTime date, String header, Photo entity): base(new Photo[] { entity }) { Date = date; Header = header; }
        }
    }
}
