namespace WezzelNL.Util
{
    public class FloatDataStream
    {
        private readonly float[] buffer;
        private int writePos = 0;
        private int readPos = 0;
        private int count = 0;
        private int bufferDelta = 0;
        private readonly object lockObj = new();

        public FloatDataStream(int capacity)
        {
            buffer = new float[capacity];
        }

        public void Write(float[] samples)
        {
            lock (lockObj)
            {
                bufferDelta += samples.Length;
                foreach (var sample in samples)
                {
                    buffer[writePos] = sample;
                    writePos = (writePos + 1) % buffer.Length;
                    if (count < buffer.Length)
                        count++;
                    else
                        readPos = (readPos + 1) % buffer.Length;
                }
            }
        }

        public int Read(float[] targetBuffer)
        {
            int readSamples = 0;
            lock (lockObj)
            {
                for (int i = 0; i < targetBuffer.Length; i++)
                {
                    if (count == 0)
                        break;

                    targetBuffer[i] = buffer[readPos];
                    readPos = (readPos + 1) % buffer.Length;
                    count--;
                    readSamples++;
                }
            }
        
            for (int i = readSamples; i < targetBuffer.Length; i++) targetBuffer[i] = 0f;

            return readSamples;
        }

        public int ReadBufferDelta()
        {
            var copy = bufferDelta;
            bufferDelta = 0;
            return copy;
        }
    }
}