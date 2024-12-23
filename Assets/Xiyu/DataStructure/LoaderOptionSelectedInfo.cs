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


        public static LoaderOptionSelectedInfo None => new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        public static bool IsNone(LoaderOptionSelectedInfo loaderOptionSelectedInfo) => string.IsNullOrEmpty(loaderOptionSelectedInfo.Conclusion);
    }
}