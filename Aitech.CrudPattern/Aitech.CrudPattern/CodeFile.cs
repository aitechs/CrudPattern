namespace AiTech.CrudPattern
{
    public enum RecordStatus
    {
        NewRecord,
        ModifiedRecord,
        DeletedRecord,
        NoChanges
    }

    public interface IRecordStatus
    {
        RecordStatus RecordStatus { get; set; }        
    }
}