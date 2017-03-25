namespace Aitech.CrudPattern
{

    public interface IRecordStatus
    {
        RecordStatus RecordStatus { get; set; }
    }

    public interface ITransaction
    {
        void BeginTransaction();
        void CommitTransaction();
        void RollBackTransaction();
    }
}