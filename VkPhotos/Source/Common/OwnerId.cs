using System;

namespace VkPhotos
{
    public class OwnerId
    {
        public UInt32 Value { get; private set; }
        public OwnerType Type { get; private set; }



        private OwnerId() { }

        public static OwnerId Create(UInt32 value, OwnerType type) => new OwnerId() { Value = value, Type = type };

        public static OwnerId Create(Int32 value) => new OwnerId() { Value = (UInt32)Math.Abs(value), Type = (value < 0) ?  OwnerType.Group : OwnerType.User };

        public static OwnerId CreateInvariant() => new OwnerId() { Value = 0, Type = OwnerType.Invariant };

        public Uri GetUri() => new Uri($"https://vk.com/{ToString()}", UriKind.Absolute);

        public override String ToString()
        {
            var prefix = Type == OwnerType.Group ? "club" : "id";
            return $"{prefix}{Value}";
        }

        public static Boolean operator ==(OwnerId owner1, OwnerId owner2)
        {
            if (((Object)owner1 == null) && ((Object)owner2 == null))
                return true;
            if (((Object)owner1 == null) || ((Object)owner2 == null))
                return false;
            return (owner1.Value == owner2.Value) && (owner1.Type == owner2.Type);
        }

        public static Boolean operator !=(OwnerId owner1, OwnerId owner2)
        {
            if (((Object)owner1 == null) && ((Object)owner2 == null))
                return false;
            if (((Object)owner1 == null) || ((Object)owner2 == null))
                return true;
            return (owner1.Value != owner2.Value) || (owner1.Type != owner2.Type);
        }

        public override Boolean Equals(Object obj)
        {
            var owner = obj as OwnerId;
            return this == owner;
        }

        public override Int32 GetHashCode()
        {
            unchecked
            {
                Int32 hash = 17;
                hash = hash * 23 + Value.GetHashCode();
                hash = hash * 23 + Type.GetHashCode();
                return hash;
            }
        }

        public enum OwnerType { Invariant, User, Group }
    }
}
