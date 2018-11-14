using System;

namespace DataGate.Com
{
    
    /// <summary>
    /// 利用定时器实现的简单心跳
    /// </summary>
    public class HeartBeat:IDisposable
    {
        System.Timers.Timer timer;
        bool inprocess = false;
        public string Name { get; set; }

        /// <summary>
        /// 根据周期（秒数）新建心跳对象
        /// </summary>
        /// <param name="interval">周期（秒数）</param>
        public HeartBeat(string name, int interval, Action beat, Action<Exception> error = null)
        {
            this.Name = name;
            Interval = interval;
            timer = new System.Timers.Timer(interval * 1000);
            timer.Elapsed += (s, e) =>
            {
                if (beat != null && !inprocess)
                {
                    inprocess = true;
                    try
                    {
                        beat();
                        LastTime = DateTime.Now;
                    }
                    catch (Exception ex)
                    {
                        if (error != null) error(ex);
                    }
                }
                inprocess = false;
            };
        }

        DateTime LastTime;
        public override string ToString()
        {
            return String.Format("{0}:{1}s={2},{3}", Name, Interval, Enabled, LastTime);
        }

        /// <summary>
        /// 心跳是否在进行中
        /// </summary>
        public bool Enabled
        {
            get { return timer.Enabled; }
            set { timer.Enabled = value; }
        }

        /// <summary>
        /// 开始心跳
        /// </summary>
        public void Start()
        {
            timer.Start();
        }

        /// <summary>
        /// 停止心跳
        /// </summary>
        public void Stop()
        {
            timer.Stop();
        }


        /// <summary>
        /// 心跳时间间隔，以秒为单位
        /// </summary>
        int Interval
        {
            get;
            set;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    timer.Dispose();
                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~HeartBeat() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
