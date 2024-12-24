using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Xiyu.AntiShake
{
    /// <summary>
    /// <code>
    /// *文档说明由 文心大模型3.5生成*
    ///这个AntiShakeManager类是一个用于防止重复操作（例如防止按钮在短时间内被多次点击）的工具类。它使用ConcurrentDictionary来存储每个对象的记录信息，包括开始时间和容忍的毫秒数。下面是如何使用这个类的详细说明：
    ///
    ///    1. 初始化
    ///    首先，你需要在你的项目中创建这个类的实例。通常，你可能会在全局或者某个管理类中创建一个单例来管理这个AntiShakeManager。
    ///
    ///    csharp
    ///    AntiShakeManager antiShakeManager = new AntiShakeManager();
    ///    2. 记录操作
    ///    当你想要记录一个操作时，你需要调用Record方法。这个方法接受两个参数：一个唯一标识符（可以是UnityEngine.Object的实例ID或者任何你自定义的ID）和容忍的毫秒数。
    ///
    ///    使用UnityEngine.Object的实例ID：
    ///    csharp
    ///    int recordedId = antiShakeManager.Record(myGameObject, 500); // 500毫秒内不允许重复操作
    ///    使用自定义ID：
    ///    csharp
    ///    int customId = 12345; // 假设这是你的自定义ID
    ///    int recordedId = antiShakeManager.Record(customId, 500); // 500毫秒内不允许重复操作
    ///    3. 检查操作
    ///    在尝试执行操作之前，你应该调用Query（注意这里可能是个拼写错误，应该是Check）方法来检查是否允许执行。这个方法接受一个ID作为参数，并返回一个布尔值。
    ///
    ///    检查是否允许操作：
    ///    csharp
    ///    bool isAllowed = antiShakeManager.Query(recordedId); // 注意：应为Check，如果类定义中是Chack则保持一致
    ///    if (isAllowed)
    ///    {
    ///        // 执行操作
    ///    }
    ///    else
    ///    {
    ///        // 操作被阻止
    ///    }
    ///    注意事项
    ///        线程安全：AntiShakeManager类使用了ConcurrentDictionary和锁来确保线程安全，因此在多线程环境下也是安全的。
    ///    性能：虽然使用了ConcurrentDictionary和锁，但在极端情况下（例如非常频繁地调用Record和Chack方法），仍然可能会遇到性能瓶颈。
    ///    拼写错误：Chack方法名可能是一个拼写错误，通常应该是Check。如果在实际使用中，建议统一方法名以避免混淆。
    ///    ID生成：GetID方法通过生成随机ID来确保唯一性，但在极端情况下（例如非常多的ID被使用），可能会遇到ID冲突的风险。虽然这种情况非常罕见，但在设计系统时需要考虑这种可能性。
    ///    总结
    ///        AntiShakeManager类是一个实用的工具，可以帮助你防止用户在短时间内重复执行某些操作。通过记录操作的时间和容忍的毫秒数，你可以有效地控制用户的操作频率，提升用户体验。
    /// </code>
    /// </summary>
    public class AntiShakeManager
    {
        private class TimeSpanInfo
        {
            public TimeSpanInfo(DateTime startTime, int tolerateMillisecond)
            {
                StartTime = startTime;
                TolerateMillisecond = tolerateMillisecond;
            }

            public DateTime StartTime { get; set; }
            public int TolerateMillisecond { get; }
        }

        private readonly object _lock = new { };

        private readonly ConcurrentDictionary<int, TimeSpanInfo> _shakeTimeMap = new();


        private readonly ThreadLocal<Random> _threadLocalRandom = new(() => new Random(Guid.NewGuid().GetHashCode()));


        /// <summary>
        /// 传入一个 <see cref="System.Object"/> 对象实例调用 GetHashCode() 记录一个状态，约定一定时间内不可重复点击。
        /// <code>
        ///    var id = antiShakeManager.Record(this, 1000);
        /// </code>
        /// </summary>
        /// <param name="obj">内部唯一标识。</param>
        /// <param name="tolerateMillisecond">约定时间间隔（毫秒）。</param>
        /// <returns>如果重复会生成一个新的 id。 </returns>
        [JetBrains.Annotations.PublicAPI]
        public int Record(object obj, int tolerateMillisecond) => RecordID(obj.GetHashCode(), tolerateMillisecond);

        /// <summary>
        /// 传入一个 <see cref="UnityEngine.Object"/> 对象实例调用 GetInstance() 记录一个状态，约定一定时间内不可重复点击。
        /// <code>
        ///    var id = antiShakeManager.Record(this, 1000);
        /// </code>
        /// </summary>
        /// <param name="obj">内部唯一标识。</param>
        /// <param name="tolerateMillisecond">约定时间间隔（毫秒）。</param>
        /// <returns>如果重复会生成一个新的 id </returns>
        [JetBrains.Annotations.PublicAPI]
        public int Record(UnityEngine.Object obj, int tolerateMillisecond) => RecordID(obj.GetInstanceID(), tolerateMillisecond);

        /// <summary>
        /// 记录一个状态，约定一定时间内不可重复点击。
        /// <code>
        ///    var id = antiShakeManager.Record(System.Guid.NewGuid().GetHashCode(), 1000);
        /// </code>
        /// </summary>
        /// <param name="id">内部唯一标识</param>
        /// <param name="tolerateMillisecond">约定时间间隔（毫秒）</param>
        /// <returns>如果重复会生成一个新的<see cref="id"/></returns>
        [JetBrains.Annotations.PublicAPI]
        public int Record(int id, int tolerateMillisecond) => RecordID(id, tolerateMillisecond);

        /// <summary>
        /// 记录一个状态，约定一定时间内不可重复点击。
        /// <code>
        ///    var id = antiShakeManager.Record(1000);
        /// </code>
        /// </summary>
        /// <param name="tolerateMillisecond">约定时间间隔（毫秒）。</param>
        /// <returns>如果重复会生成一个新的 id。</returns>
        [JetBrains.Annotations.PublicAPI]
        private int Record(int tolerateMillisecond) => RecordID(GetID(), tolerateMillisecond);


        /// <summary>
        /// 查询一个状态是否可以点击。如果可以点击则返回 true，否则返回 false。
        /// </summary>
        /// <param name="id">内部唯一 id 标识。</param>
        /// <returns></returns>
        [JetBrains.Annotations.PublicAPI]
        public bool Query(int id) => QueryID(id);

        private int RecordID(int id, int tolerateMillisecond)
        {
            if (_shakeTimeMap.TryAdd(id, new TimeSpanInfo(DateTime.Now, tolerateMillisecond)))
                return id;

            var newId = GetID();
            while (!_shakeTimeMap.TryAdd(newId, new TimeSpanInfo(DateTime.Now, tolerateMillisecond)))
            {
                newId = GetID();
            }

            return newId;
        }

        private bool QueryID(int id)
        {
            if (!_shakeTimeMap.TryGetValue(id, out var timeSpanInfo))
            {
                UnityEngine.Debug.LogWarning("此ID不存在，默认返回True");
                return true;
            }

            lock (_lock)
            {
                var timeSpan = DateTime.Now - timeSpanInfo.StartTime;
                if (timeSpan.TotalMilliseconds <= timeSpanInfo.TolerateMillisecond)
                {
                    return false;
                }

                timeSpanInfo.StartTime = DateTime.Now;
                return true;
            }
        }

        private int GetID()
        {
            int id;
            // 尝试一定次数，如果还找不到，可能需要重新考虑算法
            for (var i = 0; i < 100; i++)
            {
                // 使用线程局部存储的随机数生成器
                id = _threadLocalRandom.Value.Next(int.MinValue, int.MaxValue);
                if (!_shakeTimeMap.ContainsKey(id))
                {
                    return id;
                }
            }

            // 如果多次尝试后仍然找不到，使用 Guid 生成唯一的 id
            id = Guid.NewGuid().GetHashCode();
            return id;
        }
    }
}