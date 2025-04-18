namespace Cloud_Storage_Platform
{
    public class AsyncOnlyStreamWrapper : Stream
    {
        private readonly Stream _inner;
        public AsyncOnlyStreamWrapper(Stream inner) => _inner = inner;

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => _inner.CanWrite;
        public override long Length => _inner.Length;
        public override long Position
        {
            get => _inner.Position;
            set => _inner.Position = value;
        }

        public override void Flush() => _inner.Flush();
        public override Task FlushAsync(CancellationToken ct)
            => _inner.FlushAsync(ct);

        public override int Read(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();
        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
        {
            // route the synchronous call to the async method
            _ = _inner.WriteAsync(buffer, offset, count);
        }

        public override Task WriteAsync(
            byte[] buffer, int offset, int count, CancellationToken ct)
            => _inner.WriteAsync(buffer, offset, count, ct);
    }

}
