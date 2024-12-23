namespace Xiyu.DataStructure
{
    [System.Serializable]
    public struct LoaderMapItem
    {
        [UnityEngine.SerializeField] private string addressableName;

        public string AddressableName => addressableName;
    }
}