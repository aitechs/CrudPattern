using System;
using System.Collections.Generic;

namespace AiTech.LiteOrm
{
    public interface IRecordInfo
    {
        string CreatedBy { get; set; }
        string ModifiedBy { get; set; }
        DateTime Created { get; set; }
        DateTime Modified { get; set; }
    }


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



    public interface ITrackableObject
    {
        Dictionary<string, object> OriginalValues { get; }
        void StartTrackingChanges();
        Dictionary<string, object> GetChangedValues();
    }


    public enum RecordStatus
    {
        NoChanges = 0,
        DeletedRecord = 1,
        ModifiedRecord = 2,
        NewRecord = 3
    }
}
