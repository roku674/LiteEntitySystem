using System;
using System.Text;
using LiteEntitySystem.Internal;

namespace LiteEntitySystem.Extensions
{
    public partial class SyncString : SyncableField
    {
        private static readonly UTF8Encoding Encoding = new(false, true);
        private byte[] _stringData;
        private string _string;
        private int _size;

        [BindRpc(nameof(SetNewString))]
        private static RemoteCallSpan<byte> _setStringClientCall;

        public string Value
        {
            get => _string;
            set
            {
                if (_string == value)
                    return;
                _string = value;
                Helpers.ResizeOrCreate(ref _stringData, Encoding.GetMaxByteCount(_string.Length));
                _size = Encoding.GetBytes(_string, 0, _string.Length, _stringData, 0);
                ExecuteRPC(_setStringClientCall, new ReadOnlySpan<byte>(_stringData, 0, _size));
            }
        }

        public override string ToString()
        {
            return _string;
        }

        public static implicit operator string(SyncString s)
        {
            return s.Value;
        }
        
        private void SetNewString(ReadOnlySpan<byte> data)
        {
            _string = Encoding.GetString(data);
        }

        protected internal override void OnSyncRequested()
        {
            ExecuteRPC(_setStringClientCall, new ReadOnlySpan<byte>(_stringData, 0, _size));
        }
    }
}