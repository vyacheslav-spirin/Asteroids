using System.IO;

namespace Asteroids.Game.Match
{
    public struct EntityPointer
    {
        public static readonly EntityPointer Null = new EntityPointer(0, 0);

        public readonly uint id;

        public readonly ushort fastAccessId;

        public EntityPointer(uint id, ushort fastAccessId)
        {
            this.id = id;

            this.fastAccessId = fastAccessId;
        }

        public bool Equals(EntityPointer obj)
        {
            return id == obj.id && fastAccessId == obj.fastAccessId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is EntityPointer pointer && Equals(pointer);
        }

        //generated
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = id.GetHashCode();
                hashCode = (hashCode * 397) ^ fastAccessId.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator !=(EntityPointer pointer1, EntityPointer pointer2)
        {
            return pointer1.id != pointer2.id || pointer1.fastAccessId != pointer2.fastAccessId;
        }

        public static bool operator ==(EntityPointer pointer1, EntityPointer pointer2)
        {
            return pointer1.id == pointer2.id && pointer1.fastAccessId == pointer2.fastAccessId;
        }

        public static void Save(EntityPointer entityPointer, BinaryWriter writer)
        {
            writer.Write(entityPointer.id);

            writer.Write(entityPointer.fastAccessId);
        }

        public static EntityPointer Load(BinaryReader reader)
        {
            var id = reader.ReadUInt32();
            var fastAccessId = reader.ReadUInt16();

            return new EntityPointer(id, fastAccessId);
        }
    }
}