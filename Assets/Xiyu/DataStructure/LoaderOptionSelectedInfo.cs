using System.Collections.Generic;
using JetBrains.Annotations;

namespace Xiyu.DataStructure
{
    public readonly struct LoaderOptionSelectedInfo
    {
        public LoaderOptionSelectedInfo(string firstEncounter, string atThisTime, string atThatTime, string examination, string conclusion)
        {
            FirstEncounter = firstEncounter;
            AtThisTime = atThisTime;
            AtThatTime = atThatTime;
            Examination = examination;
            Conclusion = conclusion;
        }

        [CanBeNull] public string FirstEncounter { get; }
        [CanBeNull] public string AtThisTime { get; }
        [CanBeNull] public string AtThatTime { get; }
        [CanBeNull] public string Examination { get; }


        [NotNull] public string Conclusion { get; }

        public IEnumerable<string> GetSelectedOptions(string other)
        {
            var set = new HashSet<string> { other };

            if (!string.IsNullOrEmpty(Conclusion) && !Conclusion.StartsWith("未定义-") && set.Add(Conclusion))
                yield return Conclusion;

            if (!string.IsNullOrEmpty(FirstEncounter) && !FirstEncounter.StartsWith("未定义-") && set.Add(FirstEncounter))
                yield return FirstEncounter;


            if (!string.IsNullOrEmpty(AtThisTime) && !AtThisTime.StartsWith("未定义-") && set.Add(AtThisTime))
                yield return AtThisTime;

            if (!string.IsNullOrEmpty(AtThatTime) && !AtThatTime.StartsWith("未定义-") && set.Add(AtThatTime))
                yield return AtThatTime;

            if (!string.IsNullOrEmpty(Examination) && !Examination.StartsWith("未定义-") && set.Add(Examination))
                yield return Examination;
        }

        public static LoaderOptionSelectedInfo None => new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        public static bool IsNone(LoaderOptionSelectedInfo loaderOptionSelectedInfo) => string.IsNullOrEmpty(loaderOptionSelectedInfo.Conclusion);
    }
}