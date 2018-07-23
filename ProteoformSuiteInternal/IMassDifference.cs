namespace ProteoformSuiteInternal
{
    public interface IMassDifference
    {
        int InstanceId { get; set; }
        double DeltaMass { get; set; }
        ProteoformComparison RelationType { get; set; }
        bool Accepted { get; set; }
    }
}
