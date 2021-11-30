using System;

namespace KDMagical.SUSMachine.Cache
{
    public abstract class Cache<TEvents> where TEvents : struct, System.Enum
    {
        protected class CacheValue<T>
        {
            private Cache<TEvents> cache;
            private readonly int index;
            public T Value { get; private set; }

            public CacheValue(Cache<TEvents> cache, int index)
            {
                this.cache = cache;
                this.index = index;
            }

            public void Trigger(T value)
            {
                this.Value = value;
                cache.TriggerEvent(index);
                this.Value = default;
            }
        }

        private IEventTriggerable<TEvents> eventTarget;
        public Cache(IEventTriggerable<TEvents> eventTarget)
        {
            this.eventTarget = eventTarget;
        }

        protected static void EnumLengthCheck(int index)
        {
            if (System.Enum.GetValues(typeof(TEvents)).Length < index)
                throw new ArgumentException("not enough events in enum for this cache type!");
        }

        protected void TriggerEvent(int enumIndex) =>
            eventTarget.TriggerEvent(
                ((TEvents[])Enum.GetValues(typeof(TEvents)))[enumIndex]
            );
    }

    public class Cache<TEvents, T1> : Cache<TEvents> where TEvents : struct, System.Enum
    {
        private CacheValue<T1> cacheValue1;
        public T1 Value1 => cacheValue1.Value;

        public Cache(IEventTriggerable<TEvents> eventTarget, int index = 0) : base(eventTarget)
        {
            cacheValue1 = new CacheValue<T1>(this, index);
            EnumLengthCheck(index + 1);
        }

        public void TriggerEvent1(T1 value) => cacheValue1.Trigger(value);
    }

    public class Cache<TEvents, T1, T2> : Cache<TEvents, T1> where TEvents : struct, System.Enum
    {
        private CacheValue<T2> cacheValue2;
        public T2 Value2 => cacheValue2.Value;

        public Cache(IEventTriggerable<TEvents> eventTarget, int index = 1) : base(eventTarget, index - 1)
        {
            cacheValue2 = new CacheValue<T2>(this, index);
            EnumLengthCheck(index + 1);
        }

        public void TriggerEvent2(T2 value) => cacheValue2.Trigger(value);
    }

    public class Cache<TEvents, T1, T2, T3> : Cache<TEvents, T1, T2> where TEvents : struct, System.Enum
    {
        private CacheValue<T3> cacheValue3;
        public T3 Value3 => cacheValue3.Value;

        public Cache(IEventTriggerable<TEvents> eventTarget, int index = 2) : base(eventTarget, index - 1)
        {
            cacheValue3 = new CacheValue<T3>(this, index);
            EnumLengthCheck(index + 1);
        }

        public void TriggerEvent3(T3 value) => cacheValue3.Trigger(value);
    }

    public class Cache<TEvents, T1, T2, T3, T4> : Cache<TEvents, T1, T2, T3> where TEvents : struct, System.Enum
    {
        private CacheValue<T4> cacheValue4;
        public T4 Value4 => cacheValue4.Value;

        public Cache(IEventTriggerable<TEvents> eventTarget, int index = 3) : base(eventTarget, index - 1)
        {
            cacheValue4 = new CacheValue<T4>(this, index);
            EnumLengthCheck(index + 1);
        }

        public void TriggerEvent4(T4 value) => cacheValue4.Trigger(value);
    }

    public class Cache<TEvents, T1, T2, T3, T4, T5> : Cache<TEvents, T1, T2, T3, T4> where TEvents : struct, System.Enum
    {
        private CacheValue<T5> cacheValue5;
        public T5 Value5 => cacheValue5.Value;

        public Cache(IEventTriggerable<TEvents> eventTarget, int index = 4) : base(eventTarget, index - 1)
        {
            cacheValue5 = new CacheValue<T5>(this, index);
            EnumLengthCheck(index + 1);
        }

        public void TriggerEvent5(T5 value) => cacheValue5.Trigger(value);
    }

    public class Cache<TEvents, T1, T2, T3, T4, T5, T6> : Cache<TEvents, T1, T2, T3, T4, T5> where TEvents : struct, System.Enum
    {
        private CacheValue<T6> cacheValue6;
        public T6 Value6 => cacheValue6.Value;

        public Cache(IEventTriggerable<TEvents> eventTarget, int index = 5) : base(eventTarget, index - 1)
        {
            cacheValue6 = new CacheValue<T6>(this, index);
            EnumLengthCheck(index + 1);
        }

        public void TriggerEvent6(T6 value) => cacheValue6.Trigger(value);
    }
}