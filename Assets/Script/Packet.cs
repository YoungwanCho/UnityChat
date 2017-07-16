using System;
using System.Net;
using System.Net.Sockets;
using NetworkLibrary.PacketType;

namespace NetworkLibrary
{
    public class Packet : IDisposable
    {
        public PInteger Sup { get { return this._sup; } }
        public PInteger Sub { get { return this._sub; } }

        protected PInteger _sup = null;
        protected PInteger _sub = null;

        protected PrimitiveType[] _field;

        private bool disposed = false;

        public Packet(int sup, int sub, int fieldCount)
        {
            this._sup = new PInteger(sup);
            this._sub = new PInteger(sub);
            this._field = new PrimitiveType[fieldCount];
        }

        ~Packet()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {

            }
            _field = null;
        }

        public byte[] ToBytes()
        {
            byte[] buff = new byte[this.GetSize()];
            int offset = 0;
            _sup.ToBytes(buff, ref offset);
            _sub.ToBytes(buff, ref offset);

            for(int i=0; i<_field.Length; i++)
            {
                _field[i].ToBytes(buff, ref offset);
            }

            return buff;
        }

        public int GetSize()
        {
            int result = 0;

            result += _sup.GetSize();
            result += _sub.GetSize();

            for(int i=0; i<_field.Length; i++)
            {
                result += _field[i].GetSize();
            }

            return result;
        }

    }
}
